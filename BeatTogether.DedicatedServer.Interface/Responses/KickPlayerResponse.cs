namespace BeatTogether.DedicatedServer.Interface.Responses
{
    public record KickPlayerResponse(bool Kicked)
    {
        public bool Success => Kicked;
    }
}
