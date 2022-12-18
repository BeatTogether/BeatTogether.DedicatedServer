using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using System.Collections.Generic;

namespace BeatTogether.DedicatedServer.Kernel.Managers.Abstractions
{
    public interface ILobbyManager
    {
        bool AllPlayersReady { get; }
        bool SomePlayersReady { get; }
        bool NoPlayersReady { get; }
		bool AllPlayersSpectating { get; }

        BeatmapIdentifier? SelectedBeatmap { get; }
        GameplayModifiers SelectedModifiers { get; }
        CountdownState CountDownState { get; }
        float CountdownEndTime { get; }
        GameplayModifiers EmptyModifiers {get; }

        void Update();
        void UpdateBeatmap(BeatmapIdentifier? beatmap, GameplayModifiers modifiers);
        List<BeatmapDifficulty> GetSelectedBeatmapDifficulties();
        void SetCountdown(CountdownState countdownState, float countdown = 0);
    }
}
