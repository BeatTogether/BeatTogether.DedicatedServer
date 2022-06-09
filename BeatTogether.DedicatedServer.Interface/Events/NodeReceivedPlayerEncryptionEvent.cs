
namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record NodeReceivedPlayerEncryptionEvent(string endPoint, string PlayerEndPoint);
}