using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Util;
using BeatTogether.DedicatedServer.Kernel.ENet;
using BeatTogether.DedicatedServer.Messaging.Registries;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class PacketDispatcher : IPacketDispatcher
    {
        public const byte LocalConnectionId = 0;
        public const byte ServerId = 0;
        public const byte AllConnectionIds = 127;

        private readonly IVersionedPacketRegistry _packetRegistry;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly ENetServer _serverInstance;
        private readonly Internal_Logger _logger;

        public PacketDispatcher(
            IVersionedPacketRegistry packetRegistry,
            IPlayerRegistry playerRegistry,
            Kernel_Logger kernel_Logger,
            ENetServer serverInstance)
        {
            _packetRegistry = packetRegistry;
            _playerRegistry = playerRegistry;
            _serverInstance = serverInstance;
            _logger = kernel_Logger.ForContext<PacketDispatcher>(); ;
        }

        private void SendInternal(IPlayer player, ref SpanBuffer writer, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Verbose($"Sending packet (SenderId={ServerId}) to player {player.ConnectionId} with UserId {player.HashedUserId}, packet len: {writer.Data.Length}", false);
            _serverInstance.Send(player, writer.Data, deliveryMethod);
        }

        #region Sends
        public void SendToNearbyPlayers(INetSerializable packet, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={ServerId})"
            , false);
            //Is fine to re-use buffer as packets get copied into another array by Enet
            var writer = new SpanBuffer(stackalloc byte[412]);
            foreach (var version_number in _playerRegistry.GetPlayerVersions())
            {
                writer.SetOffset(0);
                writer.WriteRoutingHeader(ServerId, LocalConnectionId);
                WriteOne(ref writer, version_number, packet);
                foreach (var player in _playerRegistry.GetPlayersOnGameVersion(version_number))
                {
                    SendInternal(player, ref writer, deliveryMethod);
                }
            }
                
                    
        }
        public void SendToNearbyPlayers(INetSerializable[] packets, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(SenderId={ServerId})"
            , false);

            var writer = new SpanBuffer(stackalloc byte[1024]);

            foreach (var version_number in _playerRegistry.GetPlayerVersions())
            {
                writer.SetOffset(0);
                writer.WriteRoutingHeader(ServerId, LocalConnectionId);
                WriteMany(ref writer, version_number, packets);
                foreach (var player in _playerRegistry.GetPlayersOnGameVersion(version_number))
                {
                    SendInternal(player, ref writer, deliveryMethod);
                }
            }
        }
        public void SendToPlayers(IPlayer[] players, INetSerializable packet, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' to specific players" +
                $"(SenderId={ServerId})"
            , false);

            var writer = new SpanBuffer(stackalloc byte[412]);
            foreach (var player in players)
            {
                writer.SetOffset(0);
                writer.WriteRoutingHeader(ServerId, LocalConnectionId);
                WriteOne(ref writer, player.Version_number, packet);
                SendInternal(player, ref writer, deliveryMethod);
            }

        }
        public void SendToPlayers(IPlayer[] players, INetSerializable[] packets, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket to specific players" +
                $"(SenderId={ServerId})"
            , false);

            var writer = new SpanBuffer(stackalloc byte[1024]);
            foreach (var player in players)
            {
                writer.SetOffset(0);
                writer.WriteRoutingHeader(ServerId, LocalConnectionId);
                WriteMany(ref writer, player.Version_number, packets);
                SendInternal(player, ref writer, deliveryMethod);
            }
        }

        public void SendExcludingPlayer(IPlayer excludedPlayer, INetSerializable packet, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(ExcludedId={excludedPlayer.ConnectionId})"
            , false);

            var writer = new SpanBuffer(stackalloc byte[412]);

            foreach (var version_number in _playerRegistry.GetPlayerVersions())
            {
                writer.SetOffset(0);
                writer.WriteRoutingHeader(ServerId, LocalConnectionId);
                WriteOne(ref writer, version_number, packet);
                foreach (var player in _playerRegistry.GetPlayersOnGameVersion(version_number))
                {
                    if (player.ConnectionId != excludedPlayer.ConnectionId)
                        SendInternal(player, ref writer, deliveryMethod);
                }
            }
        }

        public void SendExcludingPlayer(IPlayer excludedPlayer, INetSerializable[] packets, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(ExcludedId={excludedPlayer.ConnectionId})"
            , false);
            //if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Verbose))
            //    for (int i = 0; i < packets.Length; i++)
            //    {
            //        _logger.Verbose(
            //            $"Packet {i} is of type '{packets[i].GetType().Name}' "
            //        );
            //    }

            var writer = new SpanBuffer(stackalloc byte[1024]);
            foreach (var version_number in _playerRegistry.GetPlayerVersions())
            {
                writer.SetOffset(0);
                writer.WriteRoutingHeader(ServerId, LocalConnectionId);
                WriteMany(ref writer, version_number, packets);
                foreach (var player in _playerRegistry.GetPlayersOnGameVersion(version_number))
                {
                    if (player.ConnectionId != excludedPlayer.ConnectionId)
                        SendInternal(player, ref writer, deliveryMethod);
                }
            }

        }

        public void RouteExcludingPlayer(IPlayer excludedPlayer, ref SpanBuffer writer, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending routed packet " +
                $"(ExcludedId={excludedPlayer.ConnectionId})"
            , false);

            foreach (IPlayer player in _playerRegistry.Players)
                if (player.ConnectionId != excludedPlayer.ConnectionId)
                    SendInternal(player, ref writer, deliveryMethod);
        }


        public void SendFromPlayer(IPlayer fromPlayer, INetSerializable packet, IgnoranceChannelTypes deliveryMethod)
		{
			_logger.Debug(
			    $"Sending packet of type '{packet.GetType().Name}' " + 
                $"(SenderId={fromPlayer.ConnectionId})"
            , false);

            var writer = new SpanBuffer(stackalloc byte[412]);

            foreach (var version_number in _playerRegistry.GetPlayerVersions())
            {
                writer.SetOffset(0);
                writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
                WriteOne(ref writer, version_number, packet);
                foreach (var player in _playerRegistry.GetPlayersOnGameVersion(version_number))
                {
                    SendInternal(player, ref writer, deliveryMethod);
                }
            }
        }
        public void SendFromPlayer(IPlayer fromPlayer, INetSerializable[] packets, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(SenderId={fromPlayer.ConnectionId})"
            , false);

            var writer = new SpanBuffer(stackalloc byte[1024]);
            foreach (var version_number in _playerRegistry.GetPlayerVersions())
            {
                writer.SetOffset(0);
                writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
                WriteMany(ref writer, version_number, packets);
                foreach (var player in _playerRegistry.GetPlayersOnGameVersion(version_number))
                {
                    SendInternal(player, ref writer, deliveryMethod);
                }
            }
        }

        public void SendFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable packet, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={fromPlayer.ConnectionId}, ReceiverId={LocalConnectionId})."
            , false);

            var writer = new SpanBuffer(stackalloc byte[412]);
            writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            WriteOne(ref writer, toPlayer.Version_number, packet);
            SendInternal(toPlayer, ref writer, deliveryMethod);
        }

        public void SendFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable[] packets, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket" +
                $"(SenderId={fromPlayer.ConnectionId}, ReceiverId={LocalConnectionId})."
            , false);

            var writer = new SpanBuffer(stackalloc byte[1024]);
            writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            WriteMany(ref writer, toPlayer.Version_number, packets);
            SendInternal(toPlayer, ref writer, deliveryMethod);
        }

        public void RouteFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, ref SpanBuffer writer, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending routed packet " +
                $"(SenderId={fromPlayer.ConnectionId}, ReceiverId={LocalConnectionId})."
            , false);

            SendInternal(toPlayer, ref writer, deliveryMethod);
        }

        public void SendToPlayer(IPlayer player, INetSerializable packet, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={ServerId}, ReceiverId={LocalConnectionId})."
            , false);

            var writer = new SpanBuffer(stackalloc byte[412]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteOne(ref writer, player.Version_number, packet);
            SendInternal(player, ref writer, deliveryMethod);
        }
        public void SendToPlayer(IPlayer player, INetSerializable[] packets, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(SenderId={ServerId}, ReceiverId={LocalConnectionId})."
            , false);

            var writer = new SpanBuffer(stackalloc byte[1024]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteMany(ref writer, player.Version_number, packets);
            SendInternal(player, ref writer, deliveryMethod);
        }
        #endregion

        #region Writers
        public void WriteOne(ref SpanBuffer writer, int version_number, INetSerializable packet)
        {
            var type = packet.GetType();
            var packetWriter = new SpanBuffer(stackalloc byte[412]);

            if (_packetRegistry.TryGetPacketIds(type, version_number, out var packetIds))
            {
                foreach (byte packetId in packetIds)
                    packetWriter.WriteUInt8(packetId);
            }
            else if (MultiplayerCorePacketRegistry.GetIsMpCorePacket(type))
            {
                packetWriter.WriteUInt8(7);
                packetWriter.WriteUInt8(100);
                packetWriter.WriteString(type.Name);
            }
            else
            {
                _logger.Error($"Packet IDs could not be found: {type.Name}, cannot be written");
                return;
            }
            packet.WriteTo(ref packetWriter);
            writer.WriteVarUInt((uint)packetWriter.Size);
            writer.WriteBytes(packetWriter.Data.ToArray());
        }
        public void WriteMany(ref SpanBuffer writer, int version_number, INetSerializable[] packets)
        {
            for (int i = 0; i < packets.Length; i++)
                WriteOne(ref writer, version_number, packets[i]);
        }

        #endregion

    }
}
