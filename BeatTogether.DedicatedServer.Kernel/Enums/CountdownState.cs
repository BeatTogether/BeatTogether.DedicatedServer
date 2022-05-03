namespace BeatTogether.DedicatedServer.Kernel.Enums
{
    public enum CountdownState : byte
    {
        NotCountingDown = 0,
        CountingDown = 1,
        StartBeatmapCountdown = 2,
        WaitingForEntitlement = 3
    }
}
