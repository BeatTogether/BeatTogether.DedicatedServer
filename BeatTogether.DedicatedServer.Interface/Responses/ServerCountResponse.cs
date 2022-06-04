namespace BeatTogether.DedicatedServer.Interface.Responses
{
    public record ServerCountResponse(int Count)
    {
        public bool Success => Count >= 0;
    }
}
