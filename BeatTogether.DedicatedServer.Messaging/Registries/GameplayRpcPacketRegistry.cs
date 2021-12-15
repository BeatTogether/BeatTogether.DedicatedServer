using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using BeatTogether.LiteNetLib.Abstractions;

namespace BeatTogether.DedicatedServer.Messaging.Registries
{
    public sealed class GameplayRpcPacketRegistry : BasePacketRegistry
    {
        public override void Register()
        {
            AddPacket<GetGameplaySceneReadyPacket>(GameplayRpcPacketType.GetGameplaySceneReady);
            AddPacket<SetGameplaySceneReadyPacket>(GameplayRpcPacketType.SetGameplaySceneReady);
            AddPacket<GetGameplaySongReadyPacket>(GameplayRpcPacketType.GetGameplaySongReady);
            AddPacket<SetGameplaySongReadyPacket>(GameplayRpcPacketType.SetGameplaySongReady);
            AddPacket<SetGameplaySceneSyncFinishedPacket>(GameplayRpcPacketType.SetGameplaySceneSyncFinish);
            AddPacket<SetPlayerDidConnectLatePacket>(GameplayRpcPacketType.SetActivePlayerFailedToConnect);
            AddPacket<SetSongStartTimePacket>(GameplayRpcPacketType.SetSongStartTime);
            AddPacket<ReturnToMenuPacket>(GameplayRpcPacketType.ReturnToMenu);
            AddPacket<RequestReturnToMenuPacket>(GameplayRpcPacketType.RequestReturnToMenu);
            AddPacket<LevelFinishedPacket>(GameplayRpcPacketType.LevelFinished);
            AddPacket<NoteCutPacket>(GameplayRpcPacketType.NoteCut);
            AddPacket<NoteMissPacket>(GameplayRpcPacketType.NoteMissed);
        }
    }
}
