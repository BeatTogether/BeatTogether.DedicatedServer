using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.DedicatedServer.Interface.Requests
{
    public record CreateMatchmakingServerRequest(
            string Secret,
            string ManagerId,
            GameplayServerConfiguration Configuration,
            bool PermanentManager = true,
            float Timeout = 0f,
            string ServerName = "",
            float resultScreenTime = 20.0f,
            float BeatmapStartTime = 5.0f,
            float PlayersReadyCountdownTime = 0f,
            bool AllowPerPlayerModifiers = false,
            bool AllowPerPlayerDifficulties = false,
            bool AllowPerPlayerBeatmaps = false,
            bool AllowChroma = true,
            bool AllowME = true,
            bool AllowNE = true
            );
}
