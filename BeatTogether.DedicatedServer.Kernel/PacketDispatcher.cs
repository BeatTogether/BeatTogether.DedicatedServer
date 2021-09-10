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
        private readonly IPacketWriter _packetWriter;
        private readonly ILogger _logger = Log.ForContext<PacketDispatcher>();

        private const byte _localConnectionId = 0;
        private const byte _allConnectionIds = 127;

        public PacketDispatcher(
            IMatchmakingServer server,
            IPacketWriter packetWriter)
        {
            _server = server;
            _packetWriter = packetWriter;
        }

        public void SendToPlayer(IPlayer player, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(PlayerId={player.ConnectionId})."
            );

            var writer = new NetDataWriter();
            writer.PutRoutingHeader(_localConnectionId, _localConnectionId);
            _packetWriter.WriteTo(writer, packet);
            player.NetPeer.Send(writer, deliveryMethod);
        }

        public void SendFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}' " +
                $"(SenderId={fromPlayer.ConnectionId} PlayerId={toPlayer.ConnectionId})."
            );

            var writer = new NetDataWriter();
            writer.PutRoutingHeader(_localConnectionId, _localConnectionId);
            _packetWriter.WriteTo(writer, packet);
            toPlayer.NetPeer.Send(writer, deliveryMethod);
        }

        public void SendToNearbyPlayers(INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            _logger.Debug(
                $"Sending packet of type '{packet.GetType().Name}'"
            );

            var writer = new NetDataWriter();
            writer.PutRoutingHeader(_localConnectionId, _localConnectionId);
            _packetWriter.WriteTo(writer, packet);
            _server.SendToAll(writer, deliveryMethod);
        }
    }
}
