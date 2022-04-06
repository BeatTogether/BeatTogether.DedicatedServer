using BeatTogether.DedicatedServer.Messaging.Models;

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
        float CountdownEndTime { get; }

        void Update();
    }
}
