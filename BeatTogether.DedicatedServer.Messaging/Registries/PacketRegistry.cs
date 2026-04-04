using BeatTogether.Core.Models;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets;
using Microsoft.VisualBasic;

namespace BeatTogether.DedicatedServer.Messaging.Registries
{
    public sealed class PacketRegistry : BaseVersionedPacketRegistry
    {
        public override void Register()
        {
            AddPacket<SyncTimePacket>(PacketType.SyncTime);
            AddPacket<PlayerConnectedPacket>(PacketType.PlayerConnected);
            AddPacket<PlayerIdentityPacket_1_40_8>(PacketType.PlayerIdentity, new VersionRange(){ MaxVersion = "1.40.8"});
            AddPacket<PlayerIdentityPacket>(PacketType.PlayerIdentity, new VersionRange() { MinVersion = "1.42.0" });
            AddPacket<PlayerLatencyPacket>(PacketType.PlayerLatencyUpdate);
            AddPacket<PlayerDisconnectedPacket>(PacketType.PlayerDisconnected);
            AddPacket<PlayerSortOrderPacket>(PacketType.PlayerSortOrderUpdate);
            AddPacket<PlayerAvatarPacket>(PacketType_1_40_8.PlayerAvatarUpdate, new VersionRange(){MaxVersion = "1.40.8"});
            AddPacket<KickPlayerPacket>(PacketType.KickPlayer);
            AddPacket<PlayerStatePacket>(PacketType.PlayerStateUpdate);
            AddSubPacketRegistry<MultiplayerSessionPacketRegistry>(PacketType.MultiplayerSession);
            AddPacket<PingPacket>(PacketType_1_40_8.Ping, new VersionRange() { MaxVersion = "1.40.8" });
            AddPacket<PongPacket>(PacketType_1_40_8.Pong, new VersionRange() { MaxVersion = "1.40.8" });
            AddPacket<PingPacket>(PacketType.Ping, new VersionRange() { MinVersion = "1.42.0" });
            AddPacket<PongPacket>(PacketType.Pong, new VersionRange() { MinVersion = "1.42.0" });
        }
    }
}
