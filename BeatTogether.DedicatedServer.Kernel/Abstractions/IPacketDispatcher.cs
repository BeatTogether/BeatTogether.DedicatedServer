using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Enums;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IPacketDispatcher
    {
        void SendToPlayer(IPlayer player, INetSerializable packet, DeliveryMethod deliveryMethod);
        void SendToPlayer(IPlayer player, INetSerializable[] packets, DeliveryMethod deliveryMethod);
        void SendFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable packet, DeliveryMethod deliveryMethod);
        void SendFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable[] packets, DeliveryMethod deliveryMethod);
        void SendToNearbyPlayers(INetSerializable packet, DeliveryMethod deliveryMethod);
        void SendToNearbyPlayers(INetSerializable[] packets, DeliveryMethod deliveryMethod);
		void SendExcludingPlayer(IPlayer excludedPlayer, INetSerializable packet, DeliveryMethod deliveryMethod);
		void SendExcludingPlayer(IPlayer excludedPlayer, INetSerializable[] packets, DeliveryMethod deliveryMethod);
		void SendFromPlayer(IPlayer fromPlayer, INetSerializable packet, DeliveryMethod deliveryMethod);
		void SendFromPlayer(IPlayer fromPlayer, INetSerializable[] packets, DeliveryMethod deliveryMethod);

        /*
        Task SendToNearbyPlayersAndAwait(INetSerializable packet, DeliveryMethod deliveryMethod);
        Task SendToNearbyPlayersAndAwait(INetSerializable[] packets, DeliveryMethod deliveryMethod);
        Task SendExcludingPlayerAndAwait(IPlayer excludedPlayer, INetSerializable packet, DeliveryMethod deliveryMethod);
        Task SendExcludingPlayerAndAwait(IPlayer excludedPlayer, INetSerializable[] packets, DeliveryMethod deliveryMethod);
        Task SendFromPlayerAndAwait(IPlayer fromPlayer, INetSerializable packet, DeliveryMethod deliveryMethod);
        Task SendFromPlayerAndAwait(IPlayer fromPlayer, INetSerializable[] packets, DeliveryMethod deliveryMethod);
        Task SendFromPlayerToPlayerAndAwait(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable packet, DeliveryMethod deliveryMethod);
        Task SendFromPlayerToPlayerAndAwait(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable[] packets, DeliveryMethod deliveryMethod);
        Task SendToPlayerAndAwait(IPlayer player, INetSerializable packet, DeliveryMethod deliveryMethod);
        Task SendToPlayerAndAwait(IPlayer player, INetSerializable[] packets, DeliveryMethod deliveryMethod);
        */
    }
}
