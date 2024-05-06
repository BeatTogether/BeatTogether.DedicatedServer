
namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record NodeOnlineEvent(
        string EndPoint,
        string NodeVersion);
}