using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;

namespace BeatTogether.DedicatedServer.Kernel.Managers.Abstractions
{
    public interface ILobbyManager
    {
        bool AllPlayersReady { get; }
        bool SomePlayersReady { get; }
        bool NoPlayersReady { get; }
		bool AllPlayersNotWantToPlayNextLevel { get; }
        bool DoesEveryoneOwnBeatmap { get; }
        BeatmapIdentifier? SelectedBeatmap { get; }
        GameplayModifiers SelectedModifiers { get; }
        CountdownState CountDownState { get; }
        long CountdownEndTime { get; }
        GameplayModifiers EmptyModifiers {get; }
        public bool SpectatingPlayersUpdated { get; set; }
        public bool ForceStartSelectedBeatmap { get; set; }

        void Update();
        BeatmapDifficulty[] GetSelectedBeatmapDifficulties();
        CannotStartGameReason GetCannotStartGameReason(IPlayer player, bool DoesEveryoneOwnBeatmap);
    }
}
