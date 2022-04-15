using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using BeatTogether.LiteNetLib.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.Managers
{
    public sealed class GameplayManager : IGameplayManager, IDisposable
    {
        public string SessionGameId { get; private set; } = null!;
        public GameplayManagerState State { get; private set; } = GameplayManagerState.None;
        public BeatmapIdentifier? CurrentBeatmap { get; private set; }
        public GameplayModifiers? CurrentModifiers { get; private set; }

        private const float SongStartDelay = 0.5f;
        private const float ResultsScreenTime = 25f;
        private const float SceneLoadTimeLimit = 15.0f;
        private const float SongLoadTimeLimit = 15.0f;

        private float _songStartTime;

        private CancellationTokenSource? _requestReturnToMenuCts;

        private readonly IDedicatedInstance _instance;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IPacketDispatcher _packetDispatcher;

        private readonly ConcurrentDictionary<string, PlayerSpecificSettings> _playerSpecificSettings = new();
        private readonly ConcurrentDictionary<string, LevelCompletionResults> _levelCompletionResults = new();

        private readonly ConcurrentDictionary<string, TaskCompletionSource> _levelFinishedTcs = new();
        private readonly ConcurrentDictionary<string, TaskCompletionSource> _sceneReadyTcs = new();
        private readonly ConcurrentDictionary<string, TaskCompletionSource> _songReadyTcs = new();

        public GameplayManager(
            IDedicatedInstance instance,
            IPlayerRegistry playerRegistry,
            IPacketDispatcher packetDispatcher)
        {
            _instance = instance;
            _playerRegistry = playerRegistry;
            _packetDispatcher = packetDispatcher;

            _instance.PlayerDisconnectedEvent += HandlePlayerDisconnected;
        }

        public void Dispose()
        {
            _instance.PlayerDisconnectedEvent -= HandlePlayerDisconnected;
        }

        public async void StartSong(BeatmapIdentifier beatmap, GameplayModifiers modifiers, CancellationToken cancellationToken)
        {
            if (State != GameplayManagerState.None)
                return;

            _instance.SetState(MultiplayerGameState.Game);
            CurrentBeatmap = beatmap;
            CurrentModifiers = modifiers;
            //Reset these values Here
            _levelFinishedTcs.Clear();
            _sceneReadyTcs.Clear();
            _songReadyTcs.Clear();
            _songStartTime = 0;

            // Reset
            SessionGameId = Guid.NewGuid().ToString();
            _requestReturnToMenuCts = new CancellationTokenSource();

            State = GameplayManagerState.SceneLoad;

            var loadingPlayers = _playerRegistry.Players; // During scene and song, only wait for players that were already connected

            // Create level finished tasks (players may send these at any time during gameplay)
            var levelFinishedCts = new CancellationTokenSource();
            var linkedLevelFinishedCts = CancellationTokenSource.CreateLinkedTokenSource(levelFinishedCts.Token, _requestReturnToMenuCts.Token);
            IEnumerable<Task> levelFinishedTasks = _playerRegistry.Players.Select(p =>
            {
                var task = _levelFinishedTcs.GetOrAdd(p.UserId, _ => new());
                linkedLevelFinishedCts.Token.Register(() => task.TrySetResult());
                return task.Task;
            });

            // Create scene ready tasks
            var sceneReadyCts = new CancellationTokenSource();
            var linkedSceneReadyCts = CancellationTokenSource.CreateLinkedTokenSource(sceneReadyCts.Token, _requestReturnToMenuCts.Token);
            IEnumerable<Task> sceneReadyTasks = loadingPlayers.Select(p => {
                var task = _sceneReadyTcs.GetOrAdd(p.UserId, _ => new());
                linkedSceneReadyCts.Token.Register(() => task.TrySetResult());
                return task.Task;
            });

            // Wait for scene ready
            _packetDispatcher.SendToNearbyPlayers(new GetGameplaySceneReadyPacket(), DeliveryMethod.ReliableOrdered);
            sceneReadyCts.CancelAfter((int)(SceneLoadTimeLimit * 1000));
            await Task.WhenAll(sceneReadyTasks);

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
            IEnumerable<Task> songReadyTasks = loadingPlayers.Select(p => {
                var task = _songReadyTcs.GetOrAdd(p.UserId, _ => new());
                linkedSongReadyCts.Token.Register(() => task.TrySetResult());
                return task.Task;
            });

            // Wait for song ready
            _packetDispatcher.SendToNearbyPlayers(new GetGameplaySongReadyPacket(), DeliveryMethod.ReliableOrdered);
            songReadyCts.CancelAfter((int)(SongLoadTimeLimit * 1000));
            await Task.WhenAll(songReadyTasks);

            // If no players are actually playing
            if (_playerRegistry.Players.All(player => !player.InGameplay))
            {
                _requestReturnToMenuCts.Cancel(); //last time i had this happen(clicked spectate as the game was starting) the lobby broke
            }

            // Start song and wait for finish
            State = GameplayManagerState.Gameplay;
            _songStartTime = _instance.RunTime + SongStartDelay;
            _packetDispatcher.SendToNearbyPlayers(new SetSongStartTimePacket
            {
                StartTime = _songStartTime
            }, DeliveryMethod.ReliableOrdered);
            await Task.WhenAll(levelFinishedTasks);  //TODO the server seems to be setting this to true instantly is the players failed the last beatmap
            //Console.WriteLine("level finished tasks have been achieved");
            State = GameplayManagerState.Results;

            // Wait at results screen if anyone cleared
            if (_levelCompletionResults.Values.Any(result => result.LevelEndStateType == LevelEndStateType.Cleared))
            {
                await Task.Delay((int)(ResultsScreenTime * 1000));
            }

            // End gameplay and reset
            _playerSpecificSettings.Clear();
            _levelCompletionResults.Clear();
            State = GameplayManagerState.None;
            CurrentBeatmap = null;
            CurrentModifiers = null;
            _packetDispatcher.SendToNearbyPlayers(new ReturnToMenuPacket(), DeliveryMethod.ReliableOrdered);
            _instance.SetState(MultiplayerGameState.Lobby);
        }

        public void HandleGameSceneLoaded(IPlayer player, SetGameplaySceneReadyPacket packet)
        {
            if (_sceneReadyTcs.TryGetValue(player.UserId, out var tcs) && tcs.Task.IsCompleted)
            {
                return;
            }
            _playerSpecificSettings[player.UserId] = packet.PlayerSpecificSettings;

            if (_instance.State == MultiplayerGameState.Game && State != GameplayManagerState.SceneLoad)
                _packetDispatcher.SendToNearbyPlayers(new SetPlayerDidConnectLatePacket
                {
                    UserId = player.UserId,
                    PlayersAtStart = new PlayerSpecificSettingsAtStart
                    {
                        ActivePlayerSpecificSettingsAtStart = _playerSpecificSettings.Values.ToList()
                    },
                    SessionGameId = SessionGameId
                }, DeliveryMethod.ReliableOrdered);

            if (_instance.State != MultiplayerGameState.Game)
            {
                _packetDispatcher.SendToPlayer(player, new ReturnToMenuPacket(), DeliveryMethod.ReliableOrdered);
            }
            _sceneReadyTcs.GetOrAdd(player.UserId, _ => new()).SetResult();
        }

        public void HandleGameSongLoaded(IPlayer player)
        {
            if (_songReadyTcs.TryGetValue(player.UserId, out var tcs) && tcs.Task.IsCompleted)
            {
                return;
            }
            if (_instance.State == MultiplayerGameState.Game && State != GameplayManagerState.SongLoad)
            {
                _packetDispatcher.SendToPlayer(player, new SetSongStartTimePacket
                {
                    StartTime = _songStartTime
                }, DeliveryMethod.ReliableOrdered);
            }
            if (_instance.State != MultiplayerGameState.Game)
            {
                _packetDispatcher.SendToPlayer(player, new ReturnToMenuPacket(), DeliveryMethod.ReliableOrdered);
            }
            _songReadyTcs.GetOrAdd(player.UserId, _ => new()).SetResult();
        }

        public void HandleLevelFinished(IPlayer player, LevelFinishedPacket packet)
        {
            //Console.WriteLine("Player finished: " + player.UserName + " Reason: " + packet.Results.PlayerLevelEndReason + " State: " + packet.Results.PlayerLevelEndState);
            if (_levelFinishedTcs.TryGetValue(player.UserId, out var tcs) && tcs.Task.IsCompleted)
            {
                return;
            }
            _levelCompletionResults[player.UserId] = packet.Results.LevelCompletionResults;
            _levelFinishedTcs.GetOrAdd(player.UserId, _ => new()).SetResult();
        }

        public void SignalRequestReturnToMenu()
            => _requestReturnToMenuCts?.Cancel();

        private void HandlePlayerDisconnected(IPlayer player)
        {
            var levelFinished = _levelFinishedTcs.GetOrAdd(player.UserId, _ => new());
            var sceneReady = _sceneReadyTcs.GetOrAdd(player.UserId, _ => new());
            var songReady = _songReadyTcs.GetOrAdd(player.UserId, _ => new());

            if (!levelFinished.Task.IsCompleted)
                levelFinished.SetResult();
            if (!sceneReady.Task.IsCompleted)
                sceneReady.SetResult();
            if (!songReady.Task.IsCompleted)
                songReady.SetResult();
        }
    }
}
