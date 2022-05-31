namespace BeatTogether.DedicatedServer.Interface.Responses
{
    public record StopMatchmakingServerResponse(bool Stopped)
    {
        public bool Success => Stopped;
    }
}
