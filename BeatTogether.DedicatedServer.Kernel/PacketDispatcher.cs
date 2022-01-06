using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Configuration;
using BeatTogether.LiteNetLib.Dispatchers;
using BeatTogether.LiteNetLib.Enums;
using Krypton.Buffers;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class PacketDispatcher : ConnectedMessageDispatcher, IPacketDispatcher
    {
        public const byte LocalConnectionId = 0;
        public const byte AllConnectionIds = 127;

        private readonly IPlayerRegistry _playerRegistry;
        private readonly ILogger _logger = Log.ForContext<PacketDispatcher>();

        public PacketDispatcher(
            IPlayerRegistry playerRegistry,
            LiteNetConfiguration configuration,
            LiteNetServer server)
            : base (
                  configuration,
                  server)
        {
            _playerRegistry = playerRegistry;
        }

        public void SendToNearbyPlayers(INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={LocalConnectionId})"
            );

            var writer = new SpanBufferWriter();
            writer.WriteRoutingHeader(LocalConnectionId, LocalConnectionId);
            packet.WriteTo(ref writer);

            foreach (var player in _playerRegistry.Players)
                Send(player.Endpoint, writer.Data, deliveryMethod);
        }

        public void SendExcludingPlayer(IPlayer excludedPlayer, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(ExcludedId={excludedPlayer.ConnectionId})"
            );

            var writer = new SpanBufferWriter();
            writer.WriteRoutingHeader(LocalConnectionId, LocalConnectionId);
            packet.WriteTo(ref writer);
            foreach (IPlayer player in _playerRegistry.Players)
                if (player.ConnectionId != excludedPlayer.ConnectionId)
                    Send(player.Endpoint, writer.Data, deliveryMethod);
        }

        public void SendFromPlayer(IPlayer fromPlayer, INetSerializable packet, DeliveryMethod deliveryMethod)
		{
			_logger.Debug(
			    $"Sending packet of type '{packet.GetType().Name}' " + 
                $"(SenderId={fromPlayer.ConnectionId})"
            );

            var writer = new SpanBufferWriter();
            writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            packet.WriteTo(ref writer);

            foreach (var player in _playerRegistry.Players)
                Send(player.Endpoint, writer.Data, deliveryMethod);
		}

        public void SendFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={fromPlayer.ConnectionId}, ReceiverId={LocalConnectionId})."
            );

            var writer = new SpanBufferWriter();
            writer.WriteRoutingHeader(fromPlayer.ConnectionId, LocalConnectionId);
            packet.WriteTo(ref writer);
            Send(toPlayer.Endpoint, writer.Data, deliveryMethod);
        }

        public void SendToPlayer(IPlayer player, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={LocalConnectionId}, ReceiverId={LocalConnectionId})."
            );

            var writer = new SpanBufferWriter();
            writer.WriteRoutingHeader(LocalConnectionId, LocalConnectionId);
            packet.WriteTo(ref writer);
            Send(player.Endpoint, writer, deliveryMethod);
        }
    }
}
