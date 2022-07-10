﻿using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using System.Threading;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.Managers.Abstractions
{
    public interface IGameplayManager
    {
        string SessionGameId { get; }
        GameplayManagerState State { get; }
		BeatmapIdentifier CurrentBeatmap { get; }
		GameplayModifiers CurrentModifiers { get; }
        public float _songStartTime { get; }

        void HandlePlayerLeaveGameplay(IPlayer player, int Unused = 0);
        void HandleGameSceneLoaded(IPlayer player, SetGameplaySceneReadyPacket packet);
        void HandleGameSongLoaded(IPlayer player);
        void HandleLevelFinished(IPlayer player, LevelFinishedPacket packet);
        void SetBeatmap(BeatmapIdentifier beatmap, GameplayModifiers modifiers);
        void StartSong(CancellationToken cancellationToken);

        void SignalRequestReturnToMenu();
    }
}
