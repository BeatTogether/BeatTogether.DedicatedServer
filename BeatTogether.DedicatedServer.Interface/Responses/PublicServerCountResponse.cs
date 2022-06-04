namespace BeatTogether.DedicatedServer.Interface.Responses
{
    public record PublicServerCountResponse(int Count)
    {
        public bool Success => Count >= 0;
    }
}
