namespace BeatTogether.DedicatedServer.Messaging.Enums
{
    public enum MultiplayerPlayerLevelEndReason
    {
        Cleared,
        Failed,
        GivenUp,
        Quit,
        HostEndedLevel,
        WasInactive,
        StartupFailed,
        ConnectedAfterLevelEnded
    }
}
