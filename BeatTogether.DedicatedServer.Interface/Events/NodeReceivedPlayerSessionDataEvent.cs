namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record NodeReceivedPlayerSessionDataEvent(
        string EndPoint,
        string PlayerSessionId);
}