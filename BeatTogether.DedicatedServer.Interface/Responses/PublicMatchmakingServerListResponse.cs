namespace BeatTogether.DedicatedServer.Interface.Responses
{
    public record PublicMatchmakingServerListResponse(string[] PublicInstances)
    {
        public bool Success => PublicInstances != null;
    }
}
