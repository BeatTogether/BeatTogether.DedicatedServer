using System;
using System.Net;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using BeatTogether.DedicatedServer.Messaging.Registries;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Enums;
using BeatTogether.LiteNetLib.Util;
using Krypton.Buffers;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class PacketSource
    {
        public const byte LocalConnectionId = 0;
        public const byte AllConnectionIds = 127;

        private readonly IServiceProvider _serviceProvider;
        private readonly IPacketRegistry _packetRegistry;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly PacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<PacketSource>();
        private readonly InstanceConfiguration _configuration;

        public PacketSource(
            IServiceProvider serviceProvider,
            IPacketRegistry packetRegistry,
            IPlayerRegistry playerRegistry,
            PacketDispatcher packetDispatcher,
            InstanceConfiguration instconfiguration)
        {
            _serviceProvider = serviceProvider;
            _packetRegistry = packetRegistry;
            _playerRegistry = playerRegistry;
            _packetDispatcher = packetDispatcher;
            _configuration = instconfiguration;
        }

        public void OnReceive(EndPoint remoteEndPoint, ref SpanBuffer reader, DeliveryMethod method)
        {
            if (!reader.TryReadRoutingHeader(out var routingHeader))
            {
                _logger.Warning(
                    "Failed to read routing header " +
                    $"(RemoteEndPoint='{remoteEndPoint}')."
                );
                return;
            }

            if (!_playerRegistry.TryGetPlayer(remoteEndPoint, out var sender))
            {
                _logger.Warning(
                    "Sender is not in this instance" +
                    $"(RemoteEndPoint='{remoteEndPoint}')."
                );
                return;
            }
            SpanBuffer HandleRead = new(reader.RemainingData.ToArray());


            while (HandleRead.RemainingSize > 0)
            {
                uint length;
                try { length = HandleRead.ReadVarUInt(); }
                catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); goto RoutePacket; }
                if (HandleRead.RemainingSize < length)
                {
                    _logger.Warning($"Packet fragmented (RemainingSize={HandleRead.RemainingSize}, Expected={length}).");
                    goto RoutePacket;
                }

                int prevPosition = HandleRead.Offset;
                INetSerializable? packet;
                IPacketRegistry packetRegistry = _packetRegistry;
                while (true)
                {
                    if (packetRegistry is not MultiplayerCorePacketRegistry MPCoreRegistry)
                    {
                        byte packetId;
                        try
                        { packetId = HandleRead.ReadByte(); }
                        catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); goto RoutePacket; }
                        if (packetRegistry.TryCreatePacket(packetId, out packet))
                            break;
                        if (packetRegistry.TryGetSubPacketRegistry(packetId, out var subPacketRegistry))
                        {
                            packetRegistry = subPacketRegistry;
                            continue;
                        }
                    }
                    else
                    {
                        string MPCpacketId;
                        try
                        { MPCpacketId = HandleRead.ReadString(); }
                        catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); goto RoutePacket; }
                        if (MPCoreRegistry.TryCreatePacket(MPCpacketId, out packet))
                            break;
                    }
                    break;
                }

                if (packet == null)
                {
                    _logger.Debug($"Failed to create packet.");
                    // skip any unprocessed bytes
                    var processedBytes = HandleRead.Offset - prevPosition;
                    try { HandleRead.SkipBytes((int)length - processedBytes); }
                    catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); goto RoutePacket; }
                    continue;
                }
                if(packet is NoteSpawnPacket || packet is ObstacleSpawnPacket || packet is SliderSpawnPacket) //Note packet logic
                {
                    if (_configuration.DisableNotes || (_playerRegistry.GetPlayerCount() > 16) && !_configuration.ForceEnableNotes)
                        return;
                    method = DeliveryMethod.Unreliable;
                    break;
                }
                if (packet is NodePoseSyncStatePacket)
                {
                    if ((DateTime.UtcNow.Ticks - sender.TicksAtLastSyncState) / TimeSpan.TicksPerMillisecond < _playerRegistry.GetMillisBetweenSyncStatePackets())
                    {
                        _logger.Verbose($"Skipping sync state packet from {sender.ConnectionId} (Secret='{sender.Instance._configuration.Secret}').");
                        return;
                    }
                    method = DeliveryMethod.Unreliable;
                    sender.TicksAtLastSyncState = DateTime.UtcNow.Ticks;
                }
                if (packet is NodePoseSyncStateDeltaPacket)
                {
                    if ((DateTime.UtcNow.Ticks - sender.TicksAtLastSyncStateDelta) / TimeSpan.TicksPerMillisecond < _playerRegistry.GetMillisBetweenSyncStatePackets())
                    {
                        _logger.Verbose($"Skipping sync state packet from {sender.ConnectionId} (Secret='{sender.Instance._configuration.Secret}').");
                        return;
                    }
                    sender.TicksAtLastSyncStateDelta = DateTime.UtcNow.Ticks;
                }
                var packetType = packet.GetType();
                var packetHandlerType = typeof(Abstractions.IPacketHandler<>)
                    .MakeGenericType(packetType);
                var packetHandler = _serviceProvider.GetService(packetHandlerType);
                if (packetHandler is null)
                {
                    //if (!packetType.Name.StartsWith("NodePoseSyncState"))
                        _logger.Verbose($"No handler exists for packet of type '{packetType.Name}'.");

                    // skip any unprocessed bytes
                    var processedBytes = HandleRead.Offset - prevPosition;
                    try { HandleRead.SkipBytes((int)length - processedBytes); }
                    catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); goto RoutePacket; }
                    continue;
                }

                try
                {
                    packet.ReadFrom(ref HandleRead);
                }
                catch
                {
                    // skip any unprocessed bytes
                    _logger.Debug($"Failed to read packet of type '{packetType.Name}'.");
                    var processedBytes = HandleRead.Offset - prevPosition;
                    try { HandleRead.SkipBytes((int)length - processedBytes); }
                    catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); goto RoutePacket; }
                    continue;
                }

                ((Abstractions.IPacketHandler)packetHandler).Handle(sender, packet);
            }
            RoutePacket:
            //Is this packet meant to be routed?
            if (routingHeader.ReceiverId != 0)
                RoutePacket(sender, routingHeader, ref reader, method);
        }
        
        #region Private Methods

        private void RoutePacket(IPlayer sender,
            (byte SenderId, byte ReceiverId, PacketOption PacketOption) routingHeader,
            ref SpanBuffer reader, DeliveryMethod deliveryMethod)
        {
            routingHeader.SenderId = sender.ConnectionId;
            var writer = new SpanBuffer(stackalloc byte[412]);
            if (routingHeader.ReceiverId == AllConnectionIds)
            {
                writer.WriteRoutingHeader(routingHeader.SenderId, routingHeader.ReceiverId, routingHeader.PacketOption);
                writer.WriteBytes(reader.RemainingData);

                _logger.Verbose(
                    $"Routing packet from {routingHeader.SenderId} -> all players " +
                    $"PacketOption='{routingHeader.PacketOption}' " +
                    $"(Secret='{sender.Instance._configuration.Secret}', DeliveryMethod={deliveryMethod})."
                );
                _packetDispatcher.RouteExcludingPlayer(sender, ref writer, deliveryMethod);
            }
            else
            {
                writer.WriteRoutingHeader(routingHeader.SenderId, LocalConnectionId, routingHeader.PacketOption);
                writer.WriteBytes(reader.RemainingData);

                if (!_playerRegistry.TryGetPlayer(routingHeader.ReceiverId, out var receiver))
                {
                    _logger.Warning(
                        "Failed to retrieve receiver " +
                        $"(Secret='{sender.Instance._configuration.Secret}', ReceiverId={routingHeader.ReceiverId})."
                    );
                    return;
                }
                _logger.Verbose(
                    $"Routing packet from {routingHeader.SenderId} -> {routingHeader.ReceiverId} " +
                    $"PacketOption='{routingHeader.PacketOption}' " +
                    $"(Secret='{sender.Instance._configuration.Secret}', DeliveryMethod={deliveryMethod})."
                );
                _packetDispatcher.RouteFromPlayerToPlayer(sender, receiver, ref writer, deliveryMethod);
            }
        }

        #endregion
    }
}
