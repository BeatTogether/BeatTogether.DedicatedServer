using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.Managers
{
    public sealed class GameplayManager : IGameplayManager
    {
        public string SessionGameId { get; private set; } = null!;
        public GameplayManagerState State { get; private set; } = GameplayManagerState.None;
        public BeatmapIdentifier? CurrentBeatmap { get; private set; }
        public GameplayModifiers? CurrentModifiers { get; private set; }

        private const float SongStartDelay = 0.5f;
        private const float ResultsScreenTime = 30f;
        private const float SceneLoadTimeLimit = 15.0f;
        private const float SongLoadTimeLimit = 15.0f;

        private float _songStartTime;

        private ConcurrentDictionary<string, PlayerSpecificSettings> _playerSpecificSettings = new();
        private ConcurrentDictionary<string, LevelCompletionResults> _levelCompletionResults = new();

        private CancellationTokenSource? _requestReturnToMenuCts;

        private IMatchmakingServer _server;
        private IPlayerRegistry _playerRegistry;
        private IPacketDispatcher _packetDispatcher;

        public GameplayManager(
            IMatchmakingServer server,
            IPlayerRegistry playerRegistry,
            IPacketDispatcher packetDispatcher)
        {
            _server = server;
            _playerRegistry = playerRegistry;
            _packetDispatcher = packetDispatcher;
        }

        public async void StartSong(BeatmapIdentifier beatmap, GameplayModifiers modifiers, CancellationToken cancellationToken)
        {
            if (State != GameplayManagerState.None)
                return;

            _server.State = MultiplayerGameState.Game;
            CurrentBeatmap = beatmap;
            CurrentModifiers = modifiers;

            // Reset
            SessionGameId = Guid.NewGuid().ToString();
            _playerSpecificSettings.Clear();
            _levelCompletionResults.Clear();
            _songStartTime = 0;
            _requestReturnToMenuCts = new CancellationTokenSource();

            State = GameplayManagerState.SceneLoad;

            var loadingPlayers = _playerRegistry.Players; // During scene and song, only wait for players that were already connected

            // Create level finished tasks (players may send these at any time during gameplay)
            var levelFinishedCts = new CancellationTokenSource();
            var linkedLevelFinishedCts = CancellationTokenSource.CreateLinkedTokenSource(levelFinishedCts.Token, _requestReturnToMenuCts.Token);
            IEnumerable<Task<LevelFinishedPacket>> levelFinishedTasks = _playerRegistry.Players.Select(player => player.WaitForLevelFinished(linkedLevelFinishedCts.Token));

            // Create scene ready tasks
            var sceneReadyCts = new CancellationTokenSource();
            var linkedSceneReadyCts = CancellationTokenSource.CreateLinkedTokenSource(sceneReadyCts.Token, _requestReturnToMenuCts.Token);
            IEnumerable<Task<SetGameplaySceneReadyPacket>> sceneReadyTasks = loadingPlayers.Select(player => player.WaitForSceneReady(linkedSceneReadyCts.Token));

            // Wait for scene ready
            _packetDispatcher.SendToNearbyPlayers(new GetGameplaySceneReadyPacket(), DeliveryMethod.ReliableOrdered);
            sceneReadyCts.CancelAfter((int)(SceneLoadTimeLimit * 1000));
            await WaitForCompletionOrCancel(sceneReadyTasks);

            // Set scene sync finished
            State = GameplayManagerState.SongLoad;
            _packetDispatcher.SendToNearbyPlayers(new SetGameplaySceneSyncFinishedPacket
            {
                SessionGameId = SessionGameId,
                PlayersAtStart = new PlayerSpecificSettingsAtStart
                {
                    ActivePlayerSpecificSettingsAtStart = _playerSpecificSettings.Values.ToList()
                }
            }, DeliveryMethod.ReliableOrdered);

            // Create song ready tasks
            var songReadyCts = new CancellationTokenSource();
            var linkedSongReadyCts = CancellationTokenSource.CreateLinkedTokenSource(songReadyCts.Token, _requestReturnToMenuCts.Token);
            IEnumerable<Task> songReadyTasks = loadingPlayers.Select(player => player.WaitForSongReady(linkedSongReadyCts.Token));

            // Wait for song ready
            _packetDispatcher.SendToNearbyPlayers(new GetGameplaySongReadyPacket(), DeliveryMethod.ReliableOrdered);
            songReadyCts.CancelAfter((int)(SongLoadTimeLimit * 1000));
            await WaitForCompletionOrCancel(songReadyTasks);

            // If no players are actually playing
            if (_playerRegistry.Players.All(player => !player.InGameplay))
            {
                _requestReturnToMenuCts.Cancel();
            }

            // Start song and wait for finish
            State = GameplayManagerState.Gameplay;
            _songStartTime = _server.RunTime + SongStartDelay;
            _packetDispatcher.SendToNearbyPlayers(new SetSongStartTimePacket
            {
                StartTime = _songStartTime
            }, DeliveryMethod.ReliableOrdered);
            await WaitForCompletionOrCancel(levelFinishedTasks);

            State = GameplayManagerState.Results;

            // Wait at results screen if anyone cleared
            if (_levelCompletionResults.Values.Any(result => result.LevelEndStateType == LevelEndStateType.Cleared))
                await Task.Delay((int)(ResultsScreenTime * 1000));

            // End gameplay
            State = GameplayManagerState.None;
            CurrentBeatmap = null;
            CurrentModifiers = null;
            _packetDispatcher.SendToNearbyPlayers(new ReturnToMenuPacket(), DeliveryMethod.ReliableOrdered);
            _server.State = MultiplayerGameState.Lobby;
        }

        public void HandleGameSceneLoaded(IPlayer player, SetGameplaySceneReadyPacket packet)
        {
            _playerSpecificSettings[player.UserId] = packet.PlayerSpecificSettings;

            if (_server.State == MultiplayerGameState.Game && State != GameplayManagerState.SceneLoad)
                _packetDispatcher.SendToNearbyPlayers(new SetPlayerDidConnectLatePacket
                {
                    UserId = player.UserId,
                    PlayersAtStart = new PlayerSpecificSettingsAtStart
                    {
                        ActivePlayerSpecificSettingsAtStart = _playerSpecificSettings.Values.ToList()
                    },
                    SessionGameId = SessionGameId
                }, DeliveryMethod.ReliableOrdered);

            if (_server.State != MultiplayerGameState.Game)
                _packetDispatcher.SendToPlayer(player, new ReturnToMenuPacket(), DeliveryMethod.ReliableOrdered);
        }

        public void HandleGameSongLoaded(IPlayer player)
        {
            if (_server.State == MultiplayerGameState.Game && State != GameplayManagerState.SongLoad)
                _packetDispatcher.SendToPlayer(player, new SetSongStartTimePacket
                {
                    StartTime = _songStartTime
                }, DeliveryMethod.ReliableOrdered);

            if (_server.State != MultiplayerGameState.Game)
                _packetDispatcher.SendToPlayer(player, new ReturnToMenuPacket(), DeliveryMethod.ReliableOrdered);
        }

        public void HandleLevelFinished(IPlayer player, LevelFinishedPacket packet)
        {
            _levelCompletionResults[player.UserId] = packet.Results.LevelCompletionResults;
        }

        private Task WaitForCompletionOrCancel(IEnumerable<Task> tasks) =>
            Task.WhenAll(tasks.Select(task => task.ContinueWith(t => t.IsCanceled ? Task.CompletedTask : t)));

        public void SignalRequestReturnToMenu()
        {
            if (_requestReturnToMenuCts != null)
            {
                _requestReturnToMenuCts.Cancel();
            }
        }
    }
}
