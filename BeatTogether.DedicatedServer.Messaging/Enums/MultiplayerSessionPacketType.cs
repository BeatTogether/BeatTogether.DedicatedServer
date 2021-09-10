namespace BeatTogether.DedicatedServer.Messaging.Enums
{
    public enum MultiplayerSessionPacketType : byte
    {
        MenuRpc = 0,
        GameplayRpc = 1,
        NodePoseSyncState = 2,
        ScoreSyncState = 3,
        NodePoseSyncStateDelta = 4,
        ScoreSyncStateDelta = 5
    }
}
