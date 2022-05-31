namespace BeatTogether.DedicatedServer.Interface.Responses
{
    public record StopMatchmakingServerResponse(bool Stopped, bool ServerExists)
    {
        public bool Success => Stopped;
    }
}
