
namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record PlayerLeaveServerEvent(
        string Secret,
        string HashedUserId);
}
