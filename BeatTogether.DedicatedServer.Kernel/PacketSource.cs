using System;
using System.Collections.Generic;
using System.Net;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession;
using BeatTogether.DedicatedServer.Messaging.Registries;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Configuration;
using BeatTogether.LiteNetLib.Enums;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Sources;
using Krypton.Buffers;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class PacketSource : ConnectedMessageSource
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
            InstanceConfiguration instconfiguration,
            LiteNetConfiguration configuration,
            LiteNetServer server)
            : base (
                  configuration,
                  server)
        {
            _serviceProvider = serviceProvider;
            _packetRegistry = packetRegistry;
            _playerRegistry = playerRegistry;
            _packetDispatcher = packetDispatcher;
            _configuration = instconfiguration;
        }

        public override void OnReceive(EndPoint remoteEndPoint, ref SpanBufferReader reader, DeliveryMethod method)
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

            //Do magic to not send certain packets if 

            // Is this packet meant to be routed?

            if (routingHeader.ReceiverId != 0)
            {

                ReadOnlySpan<byte> PacketContent = reader.RemainingData;

                if (routingHeader.ReceiverId != AllConnectionIds)
                {
                    RoutePacket(sender, routingHeader, ref PacketContent, method);
                }

                SpanBufferReader PacketCopy = new(PacketContent);
                bool RouteIt = false;

                while (PacketCopy.RemainingSize > 0)
                {
                    uint length;
                    try { length = PacketCopy.ReadVarUInt(); }
                    catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); RouteIt = true; goto EndOfPacketCheck; }
                    if (PacketCopy.RemainingSize < length)
                    {
                        _logger.Warning($"Packet fragmented (RemainingSize={PacketCopy.RemainingSize}, Expected={length}).");
                        continue;
                    }


                    var prevPosition = PacketCopy.Offset;
                    INetSerializable? packet;
                    IPacketRegistry packetRegistry = _packetRegistry;
                    byte packetId = 0;
                    while (true)
                    {
                        if (!(packetRegistry is MultiplayerCorePacketRegistry MPCoreRegistry))
                        {
                            try
                            { packetId = PacketCopy.ReadByte(); }
                            catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); RouteIt = true; goto EndOfPacketCheck; }
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
                            { MPCpacketId = PacketCopy.ReadString(); }
                            catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); RouteIt = true; goto EndOfPacketCheck; }
                            if (MPCoreRegistry.TryCreatePacket(MPCpacketId, out packet))
                                break;
                        }
                        break;
                    }


                    if (!((_configuration.DisableNotes || _playerRegistry.Players.Count > 25 || SendOverloaded) && packetRegistry.GetType() == typeof(GameplayRpcPacketRegistry) && (packetId == 12 || packetId == 13 || packetId == 14)))
                    {
                        RouteIt = true;
                    }
                    if (!((_configuration.DisablePlayerMovement || _playerRegistry.Players.Count > 25 || SendOverloaded) && packetRegistry.GetType() == typeof(MultiplayerSessionPacketRegistry) && (packetId == 2 || packetId == 4)))
                    {
                        RouteIt = true;
                    }

                    // skip any unprocessed bytes
                    var processedBytes = PacketCopy.Offset - prevPosition;
                    try { PacketCopy.SkipBytes((int)length - processedBytes); }
                    catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); RouteIt = true; goto EndOfPacketCheck; }
                    continue;
                }

                EndOfPacketCheck:

                if (RouteIt)
                    RoutePacket(sender, routingHeader, ref PacketContent, method);
            }


            while (reader.RemainingSize > 0)
            {
                uint length;
                try { length = reader.ReadVarUInt(); }
                catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); return; }
                if (reader.RemainingSize < length)
                {
                    _logger.Warning($"Packet fragmented (RemainingSize={reader.RemainingSize}, Expected={length}).");
                    return;
                }

                var prevPosition = reader.Offset;
                INetSerializable? packet;
                IPacketRegistry packetRegistry = _packetRegistry;
                while (true)
                {
                    if (!(packetRegistry is MultiplayerCorePacketRegistry MPCoreRegistry))
                    {
                        byte packetId;
                        try
                        { packetId = reader.ReadByte(); }
                        catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); return; }
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
                        { MPCpacketId = reader.ReadString(); }
                        catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); return; }
                        if (MPCoreRegistry.TryCreatePacket(MPCpacketId, out packet))
                            break;
                    }
                    break;
                }

                if (packet == null)
                {
                    // skip any unprocessed bytes
                    var processedBytes = reader.Offset - prevPosition;
                    try { reader.SkipBytes((int)length - processedBytes); }
                    catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); return; }
                    continue;
                }

                var packetType = packet.GetType();
                var packetHandlerType = typeof(Abstractions.IPacketHandler<>)
                    .MakeGenericType(packetType);
                var packetHandler = _serviceProvider.GetService(packetHandlerType);
                if (packetHandler is null)
                {
                    _logger.Verbose($"No handler exists for packet of type '{packetType.Name}'.");

                    // skip any unprocessed bytes
                    var processedBytes = reader.Offset - prevPosition;
                    try { reader.SkipBytes((int)length - processedBytes); }
                    catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); return; }
                    continue;
                }

                try
                {
                    packet.ReadFrom(ref reader);
                }
                catch
                {
                    // skip any unprocessed bytes
                    var processedBytes = reader.Offset - prevPosition;
                    reader.SkipBytes((int)length - processedBytes);
                    continue;
                }

                ((Abstractions.IPacketHandler)packetHandler).Handle(sender, packet);
            }
        }


        /*
        public override void OnReceive(EndPoint remoteEndPoint, ref SpanBufferReader reader, DeliveryMethod method)
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

            //Do magic to not send certain packets if 

            // Is this packet meant to be routed?
            if (routingHeader.ReceiverId != 0)
                RoutePacket(sender, routingHeader, ref reader, method);

            while (reader.RemainingSize > 0)
            {
                uint length;
                try { length = reader.ReadVarUInt(); }
                catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); return; }
                if (reader.RemainingSize < length)
                {
                    _logger.Warning($"Packet fragmented (RemainingSize={reader.RemainingSize}, Expected={length}).");
                    return;
                }

                var prevPosition = reader.Offset;
                INetSerializable? packet;
                IPacketRegistry packetRegistry = _packetRegistry;
                while (true)
                {
                    if (!(packetRegistry is MultiplayerCorePacketRegistry MPCoreRegistry))
                    {
                        byte packetId;
                        try
                        { packetId = reader.ReadByte(); }
                        catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); return; }
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
                        { MPCpacketId = reader.ReadString(); }
                        catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); return; }
                        if (MPCoreRegistry.TryCreatePacket(MPCpacketId, out packet))
                            break;
                    }
                    break;
                }

                if (packet == null)
                {
                    // skip any unprocessed bytes
                    var processedBytes = reader.Offset - prevPosition;
                    try { reader.SkipBytes((int)length - processedBytes); }
                    catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); return; }
                    continue;
                }

                var packetType = packet.GetType();
                var packetHandlerType = typeof(Abstractions.IPacketHandler<>)
                    .MakeGenericType(packetType);
                var packetHandler = _serviceProvider.GetService(packetHandlerType);
                if (packetHandler is null)
                {
                    _logger.Verbose($"No handler exists for packet of type '{packetType.Name}'.");

                    // skip any unprocessed bytes
                    var processedBytes = reader.Offset - prevPosition;
                    try { reader.SkipBytes((int)length - processedBytes); }
                    catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); return; }
                    continue;
                }

                try
                {
                    packet.ReadFrom(ref reader);
                }
                catch
                {
                    // skip any unprocessed bytes
                    var processedBytes = reader.Offset - prevPosition;
                    reader.SkipBytes((int)length - processedBytes);
                    continue;
                }

                ((Abstractions.IPacketHandler)packetHandler).Handle(sender, packet);
            }
        }
        */
        #region Private Methods

        private void RoutePacket(IPlayer sender,
            (byte SenderId, byte ReceiverId) routingHeader,
            ref /*SpanBufferReader*/ReadOnlySpan<byte> reader, DeliveryMethod deliveryMethod)
        {
            routingHeader.SenderId = sender.ConnectionId;
            var writer = new SpanBufferWriter(stackalloc byte[412]);
            if (routingHeader.ReceiverId == AllConnectionIds)
            {
                writer.WriteRoutingHeader(routingHeader.SenderId, routingHeader.ReceiverId);
                writer.WriteBytes(reader/*.RemainingData*/);

                _logger.Verbose(
                    $"Routing packet from {routingHeader.SenderId} -> all players " +
                    $"(Secret='{sender.Secret}', DeliveryMethod={deliveryMethod})."
                );
                foreach (var player in _playerRegistry.Players)
                    if (player != sender)
                        _packetDispatcher.Send(player.Endpoint, writer, deliveryMethod);
            }
            else
            {
                writer.WriteRoutingHeader(routingHeader.SenderId, LocalConnectionId);
                writer.WriteBytes(reader/*.RemainingData*/);

                if (!_playerRegistry.TryGetPlayer(routingHeader.ReceiverId, out var receiver))
                {
                    _logger.Warning(
                        "Failed to retrieve receiver " +
                        $"(Secret='{sender.Secret}', ReceiverId={routingHeader.ReceiverId})."
                    );
                    return;
                }
                _logger.Verbose(
                    $"Routing packet from {routingHeader.SenderId} -> {routingHeader.ReceiverId} " +
                    $"(Secret='{sender.Secret}', DeliveryMethod={deliveryMethod})."
                );
                _packetDispatcher.Send(receiver.Endpoint, writer, deliveryMethod);
            }
        }

        #endregion
    }
}
