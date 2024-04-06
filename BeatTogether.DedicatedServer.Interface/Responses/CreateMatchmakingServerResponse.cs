namespace BeatTogether.DedicatedServer.Interface.Responses
{
    public enum CreateMatchmakingServerError
    {
        None = 0,
        NoAvailableSlots = 1,
        InvalidSecret = 2
    }

    public record CreateMatchmakingServerResponse(
        CreateMatchmakingServerError Error,
        string RemoteEndPoint,
        byte[] Random,
        byte[] PublicKey)
    {
        public bool Success => Error == default;
    }
}
