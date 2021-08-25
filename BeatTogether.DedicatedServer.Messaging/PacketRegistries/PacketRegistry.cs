using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets;

namespace BeatTogether.DedicatedServer.Messaging.PacketRegistries
{
    public sealed class PacketRegistry : BasePacketRegistry
    {
        public override void Register()
        {
            AddPacket<SyncTimePacket>(PacketType.SyncTime);
            AddPacket<PlayerConnectedPacket>(PacketType.PlayerConnected);
            AddPacket<PlayerIdentityPacket>(PacketType.PlayerIdentity);
            AddPacket<PlayerLatencyPacket>(PacketType.PlayerLatencyUpdate);
            AddPacket<PlayerDisconnectedPacket>(PacketType.PlayerDisconnected);
            AddPacket<PlayerSortOrderPacket>(PacketType.PlayerSortOrderUpdate);
            AddPacket<KickPlayerPacket>(PacketType.KickPlayer);
            AddSubPacketRegistry<MultiplayerSessionPacketRegistry>(PacketType.MultiplayerSession);
        }
    }
}
