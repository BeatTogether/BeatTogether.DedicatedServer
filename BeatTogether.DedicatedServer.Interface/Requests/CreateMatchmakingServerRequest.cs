using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.DedicatedServer.Interface.Requests
{
    public record CreateMatchmakingServerRequest(
        string Secret,
        string ManagerId,
        GameplayServerConfiguration Configuration,
        bool permanentManager = false,
        float Timeout = 0f);
}
