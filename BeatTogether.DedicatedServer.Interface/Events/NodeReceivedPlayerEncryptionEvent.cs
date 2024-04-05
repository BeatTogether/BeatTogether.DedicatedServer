namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record NodeReceivedPlayerEncryptionEvent(string EndPoint, string PlayerEndPoint);
}