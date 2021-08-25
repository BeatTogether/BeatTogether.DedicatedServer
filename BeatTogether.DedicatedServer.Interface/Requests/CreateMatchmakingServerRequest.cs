using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.DedicatedServer.Interface.Requests
{
    public record CreateMatchmakingServerRequest(
        string Secret,
        string ManagerId,
        GameplayServerConfiguration Configuration);
}
