using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using System.Collections.Generic;

namespace BeatTogether.DedicatedServer.Kernel.Managers.Abstractions
{
    public interface ILobbyManager
    {
        bool AllPlayersReady { get; }
        bool AnyPlayersReady { get; }
        bool NoPlayersReady { get; }
		bool AllPlayersNotWantToPlayNextLevel { get; }
        bool CanEveryonePlayBeatmap { get; }
        BeatmapIdentifier? SelectedBeatmap { get; }
        GameplayModifiers SelectedModifiers { get; }
        CountdownState CountDownState { get; }
        long CountdownEndTime { get; }
        GameplayModifiers EmptyModifiers {get; }
        public bool UpdateSpectatingPlayers { get; set; }
        public bool ForceStartSelectedBeatmap { get; set; }

        void Update();
        Dictionary<uint, string[]>? GetSelectedBeatmapDifficultiesRequirements();
        CannotStartGameReason GetCannotStartGameReason(IPlayer player, bool DoesEveryoneOwnBeatmap);
    }
}
