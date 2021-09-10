using LiteNetLib;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IPacketDispatcher
    {
        void SendToPlayer(IPlayer player, INetSerializable packet, DeliveryMethod deliveryMethod);
        void SendFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable packet, DeliveryMethod deliveryMethod);
        void SendToNearbyPlayers(INetSerializable packet, DeliveryMethod deliveryMethod);
    }
}
