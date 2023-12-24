using System;
using System.Net;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets.Legacy;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using BeatTogether.DedicatedServer.Messaging.Registries;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Configuration;
using BeatTogether.LiteNetLib.Enums;
using BeatTogether.LiteNetLib.Sources;
using BeatTogether.LiteNetLib.Util;
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

        public override void OnReceive(EndPoint remoteEndPoint, ref SpanBuffer reader, DeliveryMethod method)
        {
            if (!_playerRegistry.TryGetPlayer(remoteEndPoint, out var sender))
            {
                _logger.Warning(
                    "Sender is not in this instance" +
                    $"(RemoteEndPoint='{remoteEndPoint}')."
                );
                return;
            }

            Version clientVersion = sender.ClientVersion;
            bool senderIsLegacyPlayer = clientVersion < ClientVersions.NewPacketVersion;

            if (!reader.TryReadRoutingHeader(senderIsLegacyPlayer, out var routingHeader))
            {
                _logger.Warning(
                    "Failed to read routing header " +
                    $"(RemoteEndPoint='{remoteEndPoint}')."
                );
                return;
            }
            SpanBuffer HandleRead = new(reader.RemainingData.ToArray());

            _logger.Verbose(
                $"Received packet from {routingHeader.SenderId} ({sender.ClientVersionString}) " +
                $"-> {routingHeader.ReceiverId} " +
                $"PacketOption='{routingHeader.PacketOption}' " +
                $"(Secret='{sender.Secret}', DeliveryMethod={method})."
            );

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

                    //Is packet meant to be routed?
                    if (routingHeader.ReceiverId != 0)
                    {
                        // Route packet to other players and skip read bytes
                        //var processedBytes = HandleRead.Offset - prevPosition;
                        //var bytesToRead = Math.Min((int)length - processedBytes, HandleRead.RemainingSize);
                        //var readerSlice = new SpanBuffer(HandleRead.ReadBytes(bytesToRead));
                        //reader.SkipBytes(bytesToRead);
                        //RoutePacket(sender, routingHeader, ref readerSlice, method);

                        // TODO: Check packet registry
                        HandleRead.SkipBytes(-1); // Rewind by 1 byte so we can include packet id
                        var processedBytes = Math.Min(HandleRead.Offset - prevPosition, HandleRead.RemainingSize);
                        var bytesToRead = Math.Min((int)length, HandleRead.RemainingSize);
                        _logger.Verbose(
                            $"Attempting to Route unhandled packet from {routingHeader.SenderId} -> {routingHeader.ReceiverId} " +
                            $"PacketOption='{routingHeader.PacketOption}' " +
                            $"ProcessedBytes='{processedBytes}' BytesToRead='{bytesToRead}' " +
                            $"(Secret='{sender.Secret}', DeliveryMethod={method})."
                        );
                        var readerSlice = new SpanBuffer(HandleRead.ReadBytes(bytesToRead));
                        RoutePacket(sender, routingHeader, ref readerSlice, method);
                    }
                    else
                    {
                        //skip any unprocessed bytes
                        var processedBytes = HandleRead.Offset - prevPosition;
                        try { HandleRead.SkipBytes((int)length - processedBytes); }
                        catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); goto RoutePacket; }
                    }
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
                        _logger.Verbose($"Skipping sync state packet from {sender.ConnectionId} (Secret='{sender.Secret}').");
                        return;
                    }
                    method = DeliveryMethod.Unreliable;
                    sender.TicksAtLastSyncState = DateTime.UtcNow.Ticks;
                }
                if (packet is NodePoseSyncStateDeltaPacket)
                {
                    if ((DateTime.UtcNow.Ticks - sender.TicksAtLastSyncStateDelta) / TimeSpan.TicksPerMillisecond < _playerRegistry.GetMillisBetweenSyncStatePackets())
                    {
                        _logger.Verbose($"Skipping sync state packet from {sender.ConnectionId} (Secret='{sender.Secret}').");
                        return;
                    }
                    sender.TicksAtLastSyncStateDelta = DateTime.UtcNow.Ticks;
                }
                var packetType = packet.GetType();
                var packetHandlerType = typeof(Abstractions.IPacketHandler<>)
                    .MakeGenericType(packetType);
                var packetHandler = _serviceProvider.GetService(packetHandlerType);

                if (packetHandler is null && packet is not IVersionedNetSerializable)
                {
                    //if (!packetType.Name.StartsWith("NodePoseSyncState"))
                        _logger.Verbose($"No handler exists for packet of type '{packetType.Name}'.");

                    // Is packet meant to be routed?
                    if (routingHeader.ReceiverId != 0)
                    {
                        // Route packet to other players and skip read bytes
                        //var bytesToRead = Math.Min((int)length, HandleRead.RemainingSize);
                        //var remainingData = reader.RemainingData;
                        //var readerSlice = new SpanBuffer(HandleRead.Data.Slice(HandleRead.Offset, bytesToRead));
                        // TODO: Check packet registry may not handle MpCore packets properly
                        HandleRead.SkipBytes(-1); // Rewind by 1 byte so we can include packet id
                        var processedBytes = Math.Min(HandleRead.Offset - prevPosition, HandleRead.RemainingSize);
                        var bytesToRead = Math.Min((int)length, HandleRead.RemainingSize);
                        _logger.Verbose(
                            $"Attempting to Route unhandled packet from {routingHeader.SenderId} -> {routingHeader.ReceiverId} " +
                            $"PacketOption='{routingHeader.PacketOption}' " +
                            $"ProcessedBytes='{processedBytes}' BytesToRead='{bytesToRead}' " +
                            $"(Secret='{sender.Secret}', DeliveryMethod={method})."
                        );
                        var readerSlice = new SpanBuffer(HandleRead.ReadBytes(bytesToRead));
                        //reader.SkipBytes(processedBytes);
                        RoutePacket(sender, routingHeader, ref readerSlice, method);
                    }
                    else
                    {
                        //skip any unprocessed bytes
                        var processedBytes = HandleRead.Offset - prevPosition;
                        try { HandleRead.SkipBytes((int)length - processedBytes); }
                        catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); goto RoutePacket; }
                    }
                    continue;
                }
                else if (packetHandler is null && packet is IVersionedNetSerializable versionedPacket)
                {
                    try
                    {
                        _logger.Debug($"Reading versioned packet of type '{packetType.Name}' with version '{clientVersion}'.");
                        versionedPacket.ReadFrom(ref HandleRead, clientVersion);
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Failed to read packet of type '{packetType.Name}' with version '{clientVersion}'.");
                        _logger.Error(e.Message);
                        _logger.Error(e.StackTrace);
                        // skip any unprocessed bytes
                        var processedBytes = HandleRead.Offset - prevPosition;
                        try { HandleRead.SkipBytes((int)length - processedBytes); }
                        catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); goto RoutePacket; }
                        continue;
                    }
                    if (routingHeader.ReceiverId == AllConnectionIds)
                        _packetDispatcher.SendExcludingPlayer(sender, versionedPacket, method);
                    else if (routingHeader.ReceiverId != 0)
                    {
                        if (!_playerRegistry.TryGetPlayer(routingHeader.ReceiverId, out var receiver))
                        {
                            _logger.Warning(
                                "Failed to retrieve receiver " +
                                $"(SenderIsLegacyPlayer='{senderIsLegacyPlayer}', Secret='{sender.Secret}', ReceiverId={routingHeader.ReceiverId})."
                            );
                            return;
                        }
                        _packetDispatcher.SendFromPlayerToPlayer(sender, receiver, versionedPacket, method);
                    }
                    continue;
                }

                try
                {
                    if (packet is IVersionedNetSerializable versionedPacket)
                    {
                        _logger.Debug($"Reading versioned packet of type '{packetType.Name}' with version '{clientVersion}'.");
                        versionedPacket.ReadFrom(ref HandleRead, clientVersion);
                    }
                    else
                    {
                        _logger.Debug($"Reading unversioned packet of type '{packetType.Name}'.");
                        packet.ReadFrom(ref HandleRead);
                    }
                }
                catch
                {
                    // skip any unprocessed bytes
                    _logger.Error($"Failed to read packet of type '{packetType.Name}'.");
                    var processedBytes = HandleRead.Offset - prevPosition;
                    try { HandleRead.SkipBytes((int)length - processedBytes); }
                    catch (EndOfBufferException) { _logger.Warning("Packet was an incorrect length"); goto RoutePacket; }
                    continue;
                }

                ((Abstractions.IPacketHandler)packetHandler).Handle(sender, packet);
            }
            return;
            RoutePacket:
            //Is this packet meant to be routed ?
            if (routingHeader.ReceiverId != 0)
            {
                _logger.Warning(
                    $"Reached route packet function {routingHeader.SenderId} -> {routingHeader.ReceiverId} " +
                    $"PacketOption='{routingHeader.PacketOption}' " +
                    $"(Secret='{sender.Secret}', DeliveryMethod={method}, RemainingSize={reader.RemainingSize})."
                );
                RoutePacket(sender, routingHeader, ref reader, method);
            }
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
                var legacyWriter = new SpanBuffer(stackalloc byte[412]);
                legacyWriter.WriteLegacyRoutingHeader(routingHeader.SenderId, routingHeader.ReceiverId, routingHeader.PacketOption);
                writer.WriteRoutingHeader(routingHeader.SenderId, routingHeader.ReceiverId, routingHeader.PacketOption);
                var remainingData = reader.RemainingData;
                legacyWriter.WriteBytes(remainingData);
                writer.WriteBytes(remainingData);

                _logger.Verbose(
                    $"Routing packet from {routingHeader.SenderId} -> all players " +
                    $"PacketOption='{routingHeader.PacketOption}' " +
                    $"(Secret='{sender.Secret}', DeliveryMethod={deliveryMethod})."
                );
                _packetDispatcher.RouteExcludingPlayer(sender, ref writer, deliveryMethod, ClientVersions.NewPacketVersion);
                _packetDispatcher.RouteExcludingPlayer(sender, ref legacyWriter, deliveryMethod, ClientVersions.DefaultVersion, ClientVersions.NewPacketVersion);
            }
            else
            {
                if (!_playerRegistry.TryGetPlayer(routingHeader.ReceiverId, out var receiver))
                {
                    _logger.Warning(
                        "Failed to retrieve receiver " +
                        $"(Secret='{sender.Secret}', ReceiverId={routingHeader.ReceiverId})."
                    );
                    return;
                }

                if (receiver.ClientVersion < ClientVersions.NewPacketVersion)
                    writer.WriteLegacyRoutingHeader(routingHeader.SenderId, LocalConnectionId, routingHeader.PacketOption);
                else
                    writer.WriteRoutingHeader(routingHeader.SenderId, LocalConnectionId, routingHeader.PacketOption);
                writer.WriteBytes(reader.RemainingData);


                _logger.Verbose(
                    $"Routing packet from {routingHeader.SenderId} ({sender.ClientVersionString}) -> {routingHeader.ReceiverId} ({receiver.ClientVersionString}) " +
                    $"PacketOption='{routingHeader.PacketOption}' " +
                    $"(Secret='{sender.Secret}', DeliveryMethod={deliveryMethod})."
                );
                _packetDispatcher.RouteFromPlayerToPlayer(sender, receiver, ref writer, deliveryMethod);
            }
        }

        private static Version? TryParseGameVersion(string versionText)
        {
            var idxUnderscore = versionText.IndexOf('_');

            if (idxUnderscore >= 0)
                versionText = versionText.Substring(0, idxUnderscore);

            return Version.TryParse(versionText, out var version) ? version : null;
        }

        #endregion
    }
}
