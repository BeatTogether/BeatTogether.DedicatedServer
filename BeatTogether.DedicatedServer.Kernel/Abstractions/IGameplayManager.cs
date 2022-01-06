using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using System.Threading;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IGameplayManager
    {
        string SessionGameId { get; }
        GameplayManagerState State { get; }
		BeatmapIdentifier? CurrentBeatmap { get; }
		GameplayModifiers? CurrentModifiers { get; }

		void HandleGameSceneLoaded(IPlayer player, SetGameplaySceneReadyPacket packet);
        void HandleGameSongLoaded(IPlayer player);
        void HandleLevelFinished(IPlayer player, LevelFinishedPacket packet);
		void StartSong(BeatmapIdentifier beatmap, GameplayModifiers modifiers, CancellationToken cancellationToken);

        void SignalRequestReturnToMenu();
    }
}
