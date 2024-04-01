using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets;
using BeatTogether.DedicatedServer.Messaging.Abstractions;

namespace BeatTogether.DedicatedServer.Messaging.Registries
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
            AddPacket<PlayerAvatarPacket>(PacketType.PlayerAvatarUpdate);
            AddPacket<KickPlayerPacket>(PacketType.KickPlayer);
            AddPacket<PlayerStatePacket>(PacketType.PlayerStateUpdate);
            AddSubPacketRegistry<MultiplayerSessionPacketRegistry>(PacketType.MultiplayerSession);
            AddPacket<PingPacket>(PacketType.Ping);
            AddPacket<PongPacket>(PacketType.Pong);
        }
    }
}
