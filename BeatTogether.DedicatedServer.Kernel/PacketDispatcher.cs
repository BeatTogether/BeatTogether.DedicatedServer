using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Util;
using Serilog;
using BeatTogether.DedicatedServer.Kernel.ENet;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class PacketDispatcher : IPacketDispatcher
    {
        public const byte LocalConnectionId = 0;
        public const byte ServerId = 0;
        public const byte AllConnectionIds = 127;

        private readonly IPacketRegistry _packetRegistry;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly ENetServer _serverInstance;
        private readonly ILogger _logger = Log.ForContext<PacketDispatcher>();

        public PacketDispatcher(
            IPacketRegistry packetRegistry,
            IPlayerRegistry playerRegistry,
            ENetServer serverInstance)
        {
            _packetRegistry = packetRegistry;
            _playerRegistry = playerRegistry;
            _serverInstance = serverInstance;
        }

        private void SendInternal(IPlayer player, ref SpanBuffer writer, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Verbose($"Sending packet (SenderId={ServerId}) to player {player.ConnectionId} with UserId {player.UserId}");
            _serverInstance.Send(player, writer.Data, deliveryMethod);
        }

        #region Sends
        public void SendToNearbyPlayers(INetSerializable packet, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={ServerId})"
            );

            var writer = new SpanBuffer(stackalloc byte[412]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteOne(ref writer, packet);
            foreach (var player in _playerRegistry.Players)
                SendInternal(player, ref writer, deliveryMethod);
                    
        }
        public void SendToNearbyPlayers(INetSerializable[] packets, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(SenderId={ServerId})"
            );

            var writer = new SpanBuffer(stackalloc byte[1024]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteMany(ref writer, packets);
            foreach (var player in _playerRegistry.Players)
                SendInternal(player, ref writer, deliveryMethod);
        }
        public void SendToPlayers(IPlayer[] players, INetSerializable packet, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' to specific players" +
                $"(SenderId={ServerId})"
            );

            var writer = new SpanBuffer(stackalloc byte[412]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteOne(ref writer, packet);
            foreach (var player in players)
                SendInternal(player, ref writer, deliveryMethod);
                    
        }
        public void SendToPlayers(IPlayer[] players, INetSerializable[] packets, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket to specific players" +
                $"(SenderId={ServerId})"
            );

            var writer = new SpanBuffer(stackalloc byte[1024]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteMany(ref writer, packets);
            foreach (var player in players)
                SendInternal(player, ref writer, deliveryMethod);
        }

        public void SendExcludingPlayer(IPlayer excludedPlayer, INetSerializable packet, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(ExcludedId={excludedPlayer.ConnectionId})"
            );

            var writer = new SpanBuffer(stackalloc byte[412]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteOne(ref writer, packet);

            foreach (IPlayer player in _playerRegistry.Players)
                if (player.ConnectionId != excludedPlayer.ConnectionId)
                    SendInternal(player, ref writer, deliveryMethod);
        }

        public void SendExcludingPlayer(IPlayer excludedPlayer, INetSerializable[] packets, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(ExcludedId={excludedPlayer.ConnectionId})"
            );
            if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Verbose))
                for (int i = 0; i < packets.Length; i++)
                {
                    _logger.Verbose(
                        $"Packet {i} is of type '{packets[i].GetType().Name}' "
                    );
                }

            var writer = new SpanBuffer(stackalloc byte[1024]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteMany(ref writer, packets);

            foreach (IPlayer player in _playerRegistry.Players)
                if (player.ConnectionId != excludedPlayer.ConnectionId)
                    SendInternal(player, ref writer, deliveryMethod);
        }

        public void RouteExcludingPlayer(IPlayer excludedPlayer, ref SpanBuffer writer, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending routed packet " +
                $"(ExcludedId={excludedPlayer.ConnectionId})"
            );

            foreach (IPlayer player in _playerRegistry.Players)
                if (player.ConnectionId != excludedPlayer.ConnectionId)
                    SendInternal(player, ref writer, deliveryMethod);
        }


        public void SendFromPlayer(IPlayer fromPlayer, INetSerializable packet, IgnoranceChannelTypes deliveryMethod)
		{
			_logger.Debug(
			    $"Sending packet of type '{packet.GetType().Name}' " + 
                $"(SenderId={fromPlayer.ConnectionId})"
            );

            var writer = new SpanBuffer(stackalloc byte[412]);
            writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            WriteOne(ref writer, packet);

            foreach (var player in _playerRegistry.Players)
                SendInternal(player, ref writer, deliveryMethod);
		}
        public void SendFromPlayer(IPlayer fromPlayer, INetSerializable[] packets, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(SenderId={fromPlayer.ConnectionId})"
            );

            var writer = new SpanBuffer(stackalloc byte[1024]);
            writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            WriteMany(ref writer, packets);

            foreach (var player in _playerRegistry.Players)
                SendInternal(player, ref writer, deliveryMethod);
        }

        public void SendFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable packet, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={fromPlayer.ConnectionId}, ReceiverId={LocalConnectionId})."
            );

            var writer = new SpanBuffer(stackalloc byte[412]);
            writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            WriteOne(ref writer, packet);
            SendInternal(toPlayer, ref writer, deliveryMethod);
        }

        public void SendFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable[] packets, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket" +
                $"(SenderId={fromPlayer.ConnectionId}, ReceiverId={LocalConnectionId})."
            );

            var writer = new SpanBuffer(stackalloc byte[1024]);
            writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            WriteMany(ref writer, packets);
            SendInternal(toPlayer, ref writer, deliveryMethod);
        }

        public void RouteFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, ref SpanBuffer writer, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending routed packet " +
                $"(SenderId={fromPlayer.ConnectionId}, ReceiverId={LocalConnectionId})."
            );

            SendInternal(toPlayer, ref writer, deliveryMethod);
        }

        public void SendToPlayer(IPlayer player, INetSerializable packet, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={ServerId}, ReceiverId={LocalConnectionId})."
            );

            var writer = new SpanBuffer(stackalloc byte[412]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteOne(ref writer, packet);
            SendInternal(player, ref writer, deliveryMethod);
        }
        public void SendToPlayer(IPlayer player, INetSerializable[] packets, IgnoranceChannelTypes deliveryMethod)
        {
            _logger.Debug(
                $"Sending MultiPacket " +
                $"(SenderId={ServerId}, ReceiverId={LocalConnectionId})."
            );

            var writer = new SpanBuffer(stackalloc byte[1024]);
            writer.WriteRoutingHeader(ServerId, LocalConnectionId);
            WriteMany(ref writer, packets);
            SendInternal(player, ref writer, deliveryMethod);
        }
        #endregion

        #region Writers
        public void WriteOne(ref SpanBuffer writer, INetSerializable packet)
        {
            var type = packet.GetType();
            var packetWriter = new SpanBuffer(stackalloc byte[412]);

            if (_packetRegistry.TryGetPacketIds(type, out var packetIds))
            {
                foreach (byte packetId in packetIds)
                    packetWriter.WriteUInt8(packetId);
            }
            else
            {

                packetWriter.WriteUInt8(7);
                packetWriter.WriteUInt8(100);
                packetWriter.WriteString(type.Name);
                //Presume it is a mpcore packet and use the mpcore packet ID, would thow an exeption here if not
                //throw new Exception($"Failed to retrieve identifier for packet of type '{type.Name}'");
                //this should be fine as its only for packets sent from the server
            }
            packet.WriteTo(ref packetWriter);
            writer.WriteVarUInt((uint)packetWriter.Size);
            writer.WriteBytes(packetWriter.Data.ToArray());
        }
        public void WriteMany(ref SpanBuffer writer, INetSerializable[] packets)
        {
            for (int i = 0; i < packets.Length; i++)
                WriteOne(ref writer, packets[i]);
        }

        #endregion

    }
}
