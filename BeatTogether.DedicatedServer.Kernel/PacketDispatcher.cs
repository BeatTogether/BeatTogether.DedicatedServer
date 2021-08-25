using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.Extensions;
using LiteNetLib;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class PacketDispatcher : IPacketDispatcher
    {
        private readonly IPacketWriter _packetWriter;

        private const byte _localConnectionId = 0;
        private const byte _allConnectionIds = 127;

        public PacketDispatcher(IPacketWriter packetWriter)
        {
            _packetWriter = packetWriter;
        }

        public void SendToPlayer(IPlayer player, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            var writer = new NetDataWriter();
            writer.PutRoutingHeader(_localConnectionId, _localConnectionId);
            _packetWriter.WriteTo(writer, packet);
            player.NetPeer.Send(writer, deliveryMethod);
        }

        public void SendFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            var writer = new NetDataWriter();
            writer.PutRoutingHeader(_localConnectionId, _localConnectionId);
            _packetWriter.WriteTo(writer, packet);
            toPlayer.NetPeer.Send(writer, deliveryMethod);
        }

        public void SendToNearbyPlayers(IPlayer player, INetSerializable packet, DeliveryMethod deliveryMethod)
        {
            var writer = new NetDataWriter();
            writer.PutRoutingHeader(_localConnectionId, _localConnectionId);
            _packetWriter.WriteTo(writer, packet);
            player.NetPeer.NetManager.SendToAll(writer, deliveryMethod);
        }
    }
}
