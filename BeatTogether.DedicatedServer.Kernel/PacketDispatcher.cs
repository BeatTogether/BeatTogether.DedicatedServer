using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.Extensions;
using LiteNetLib;
using LiteNetLib.Utils;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class PacketDispatcher : IPacketDispatcher
    {
        private readonly IMatchmakingServer _server;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IPacketWriter _packetWriter;
        private readonly ILogger _logger = Log.ForContext<PacketDispatcher>();

        private const byte _localConnectionId = 0;
        private const byte _allConnectionIds = 127;

        public PacketDispatcher(
            IMatchmakingServer server,
            IPlayerRegistry playerRegistry,
            IPacketWriter packetWriter)
        {
            _server = server;
            _playerRegistry = playerRegistry;
            _packetWriter = packetWriter;
        }

        public void SendToNearbyPlayers(INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={_localConnectionId})"

            );

            var writer = new NetDataWriter();
            writer.PutRoutingHeader(_localConnectionId, _localConnectionId);
            _packetWriter.WriteTo(writer, packet);
            _server.SendToAll(writer, deliveryMethod);
        }

        public void SendExcludingPlayer(IPlayer excludedPlayer, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(ExcludedId={excludedPlayer.ConnectionId})"
            );

            var writer = new NetDataWriter();
            writer.PutRoutingHeader(_localConnectionId, _localConnectionId);
            _packetWriter.WriteTo(writer, packet);
            foreach (IPlayer player in _playerRegistry.Players)
            {
                if (player.ConnectionId != excludedPlayer.ConnectionId)
                {
                    player.NetPeer.Send(writer, deliveryMethod);
                }
            }
        }

        public void SendFromPlayer(IPlayer fromPlayer, INetSerializable packet, DeliveryMethod deliveryMethod)
		{
			_logger.Debug(
			    $"Sending packet of type '{packet.GetType().Name}' " + 
                $"(SenderId={fromPlayer.ConnectionId})"
            );

            var writer = new NetDataWriter();
            writer.PutRoutingHeader(fromPlayer.ConnectionId, _localConnectionId);
            _packetWriter.WriteTo(writer, packet);
            _server.SendToAll(writer, deliveryMethod);
		}

        public void SendFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={fromPlayer.ConnectionId}, ReceiverId={toPlayer.ConnectionId})."
            );

            var writer = new NetDataWriter();
            writer.PutRoutingHeader(fromPlayer.ConnectionId, toPlayer.ConnectionId);
            _packetWriter.WriteTo(writer, packet);
            toPlayer.NetPeer.Send(writer, deliveryMethod);
        }

        public void SendToPlayer(IPlayer player, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={_localConnectionId}, ReceiverId={player.ConnectionId})."
            );

            var writer = new NetDataWriter();
            writer.PutRoutingHeader(_localConnectionId, player.ConnectionId);
            _packetWriter.WriteTo(writer, packet);
            player.NetPeer.Send(writer, deliveryMethod);
        }
    }
}
