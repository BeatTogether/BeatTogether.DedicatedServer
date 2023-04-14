namespace BeatTogether.DedicatedServer.Messaging.Enums
{
    public enum GameLiftMessageType : uint
    {
        AuthenticateUserRequest,
        AuthenticateUserResponse,
        MessageReceivedAcknowledge,
        MultipartMessage
    }
}