namespace BeatTogether.DedicatedServer.Messaging.Requests
{
    public record GetAvailableRelayServerRequest(string SourceEndPoint, string TargetEndPoint);
}
