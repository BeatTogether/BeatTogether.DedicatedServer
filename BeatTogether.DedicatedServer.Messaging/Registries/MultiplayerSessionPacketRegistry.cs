using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession;
using BeatTogether.LiteNetLib.Abstractions;

namespace BeatTogether.DedicatedServer.Messaging.Registries
{
    public sealed class MultiplayerSessionPacketRegistry : BasePacketRegistry
    {
        public override void Register()
        {
            AddSubPacketRegistry<MenuRpcPacketRegistry>(MultiplayerSessionPacketType.MenuRpc);
            AddSubPacketRegistry<GameplayRpcPacketRegistry>(MultiplayerSessionPacketType.GameplayRpc);
            AddPacket<NodePoseSyncStatePacket>(MultiplayerSessionPacketType.NodePoseSyncState);
            AddPacket<NodePoseSyncStateDeltaPacket>(MultiplayerSessionPacketType.NodePoseSyncStateDelta);
            AddPacket<ScoreSyncStatePacket>(MultiplayerSessionPacketType.ScoreSyncState);
            AddPacket<ScoreSyncStateDeltaPacket>(MultiplayerSessionPacketType.ScoreSyncStateDelta);
        }
    }
}
