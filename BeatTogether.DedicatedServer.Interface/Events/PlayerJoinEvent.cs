
namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record PlayerJoinEvent(
        string Secret,
        string HashedUserId);
}
