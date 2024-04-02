using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Messaging.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IPacketDispatcher
    {
        void SendToPlayer(IPlayer player, INetSerializable packet, IgnoranceChannelTypes deliveryMethod);
        void SendToPlayer(IPlayer player, INetSerializable[] packets, IgnoranceChannelTypes deliveryMethod);
        void SendFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable packet, IgnoranceChannelTypes deliveryMethod);
        void SendFromPlayerToPlayer(IPlayer fromPlayer, IPlayer toPlayer, INetSerializable[] packets, IgnoranceChannelTypes deliveryMethod);
        void SendToNearbyPlayers(INetSerializable packet, IgnoranceChannelTypes deliveryMethod);
        void SendToNearbyPlayers(INetSerializable[] packets, IgnoranceChannelTypes deliveryMethod);
		void SendExcludingPlayer(IPlayer excludedPlayer, INetSerializable packet, IgnoranceChannelTypes deliveryMethod);
		void SendExcludingPlayer(IPlayer excludedPlayer, INetSerializable[] packets, IgnoranceChannelTypes deliveryMethod);
		void SendFromPlayer(IPlayer fromPlayer, INetSerializable packet, IgnoranceChannelTypes deliveryMethod);
		void SendFromPlayer(IPlayer fromPlayer, INetSerializable[] packets, IgnoranceChannelTypes deliveryMethod);
    }
}
