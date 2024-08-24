
namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record NodeStartedEvent(
        string EndPoint,
        string NodeVersion);
}