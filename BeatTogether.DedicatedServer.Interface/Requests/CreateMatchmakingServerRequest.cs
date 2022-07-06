using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.DedicatedServer.Interface.Requests
{
    public record CreateMatchmakingServerRequest(
            string Secret,
            string ManagerId,
            GameplayServerConfiguration Configuration,
            bool PermanentManager = false,//If a user links there account to discord and uses a bot to make a lobby, then can enter there userId
            float Timeout = 0f,
            string ServerName = "",
            float resultScreenTime = 20.0f,
            float BeatmapStartTime = 5.0f,
            float PlayersReadyCountdownTime = 0f,
            bool AllowPerPlayerModifiers = false,
            bool AllowPerPlayerDifficulties = false,
            bool AllowPerPlayerBeatmaps = false, //This option allows the above by default
            bool AllowChroma = true,
            bool AllowME = true,
            bool AllowNE = false
            );
}
