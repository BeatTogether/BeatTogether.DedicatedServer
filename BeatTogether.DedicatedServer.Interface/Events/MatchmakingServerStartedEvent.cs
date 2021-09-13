using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.DedicatedServer.Interface.Events
{
    public sealed record MatchmakingServerStartedEvent(
        string Secret,
        string ManagerId,
        GameplayServerConfiguration Configuration);
}
