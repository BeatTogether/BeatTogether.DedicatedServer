namespace BeatTogether.DedicatedServer.Messaging.Responses
{
    public enum GetAvailableRelayServerError
    {
        None = 0,
        NoAvailableRelayServers = 1,
        FailedToStartRelayServer = 2
    }

    public record GetAvailableRelayServerResponse(
        GetAvailableRelayServerError Error,
        string? RemoteEndPoint = null)
    {
        public bool Success => Error == default;
    }
}
