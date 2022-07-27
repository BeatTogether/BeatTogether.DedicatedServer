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

/*Gameplay manager code
 * Waits for players to have loaded the beatmap and be ready
 * Tells clients when to start beatmap
 * Waits for clients to finish before setting the dedicated server back to Lobby state
 * Handles when a client leaves gameplay mode
 */

namespace BeatTogether.DedicatedServer.Kernel.Managers
{
    public sealed class GameplayManager : IGameplayManager, IDisposable
    {
        public string SessionGameId { get; private set; } = null!;
        public GameplayManagerState State { get; private set; } = GameplayManagerState.None;
        public BeatmapIdentifier? CurrentBeatmap { get; private set; } = null;
        public GameplayModifiers CurrentModifiers { get; private set; } = new();

        private const float SongStartDelay = 0.5f;
        private const float SceneLoadTimeLimit = 15.0f;
        private const float SongLoadTimeLimit = 15.0f;

        public float _songStartTime { get; private set; }
        List<string> PlayersAtStart = new();

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

            _instance.PlayerDisconnectedEvent += HandlePlayerLeaveGameplay;
        }

        public void Dispose()
        {
            _instance.PlayerDisconnectedEvent -= HandlePlayerLeaveGameplay;
        }

        public void SetBeatmap(BeatmapIdentifier? beatmap, GameplayModifiers modifiers)
        {
            CurrentBeatmap = beatmap;
            CurrentModifiers = modifiers;
        }

        public async void StartSong(CancellationToken cancellationToken)
        {
            if (State != GameplayManagerState.None || CurrentBeatmap == null)
            {
                _requestReturnToMenuCts!.Cancel();
                return;
            }
            _instance.SetState(MultiplayerGameState.Game);

            //Reset
            ResetValues();
            _instance.InstanceStateChanged(CountdownState.NotCountingDown, State);
            SessionGameId = Guid.NewGuid().ToString();

            CancellationTokenSource levelFinishedCts;
            CancellationTokenSource linkedLevelFinishedCts;
            //IEnumerable<Task> levelFinishedTasks;
            CancellationTokenSource sceneReadyCts;
            CancellationTokenSource linkedSceneReadyCts;
            //IEnumerable<Task> sceneReadyTasks;
            CancellationTokenSource songReadyCts;
            CancellationTokenSource linkedSongReadyCts;
            //IEnumerable<Task> songReadyTasks;

            lock (RequestReturnLock)
            {
                _requestReturnToMenuCts = new CancellationTokenSource();
                State = GameplayManagerState.SceneLoad;

                foreach (var player in _playerRegistry.Players)//Array of players that are playing at the start
                {
                    if (!player.IsSpectating)
                    {
                        PlayersAtStart.Add(player.UserId);
                    }
                }

                // Create level finished tasks (players may send these at any time during gameplay)
                levelFinishedCts = new();
                linkedLevelFinishedCts = CancellationTokenSource.CreateLinkedTokenSource(levelFinishedCts.Token, _requestReturnToMenuCts.Token);
                /*
                levelFinishedTasks = PlayersAtStart.Select(p =>
                {
                    var task = _levelFinishedTcs.GetOrAdd(p, _ => new());
                    linkedLevelFinishedCts.Token.Register(() => LeaveGameplay(p));
                    return task.Task;
                });
                */
                PlayersAtStart.ForEach(p =>
                {
                    var task = _levelFinishedTcs.GetOrAdd(p, _ => new());
                    linkedLevelFinishedCts.Token.Register(() => LeaveGameplay(p));
                });
                // Create scene ready tasks
                sceneReadyCts = new();
                linkedSceneReadyCts = CancellationTokenSource.CreateLinkedTokenSource(sceneReadyCts.Token, _requestReturnToMenuCts.Token);
                /*
                sceneReadyTasks = PlayersAtStart.Select(p =>
                {
                    var task = _sceneReadyTcs.GetOrAdd(p, _ => new());
                    linkedSceneReadyCts.Token.Register(() => LeaveGameplay(p));
                    return task.Task;
                });
                */
                PlayersAtStart.ForEach(p =>
                {
                    var task = _sceneReadyTcs.GetOrAdd(p, _ => new());
                    linkedSceneReadyCts.Token.Register(() => LeaveGameplay(p));
                });

                // Create song ready tasks
                songReadyCts = new();
                linkedSongReadyCts = CancellationTokenSource.CreateLinkedTokenSource(songReadyCts.Token, _requestReturnToMenuCts.Token);
                /*
                songReadyTasks = PlayersAtStart.Select(p =>
                {
                    var task = _songReadyTcs.GetOrAdd(p, _ => new());
                    linkedSongReadyCts.Token.Register(() => LeaveGameplay(p));
                    return task.Task;
                });
                */
                PlayersAtStart.ForEach(p =>
                {
                    var task = _songReadyTcs.GetOrAdd(p, _ => new());
                    linkedSongReadyCts.Token.Register(() => LeaveGameplay(p));
                });
            }
            // Wait for scene ready
            _packetDispatcher.SendToNearbyPlayers(new GetGameplaySceneReadyPacket(), DeliveryMethod.ReliableOrdered);
            sceneReadyCts.CancelAfter((int)((SceneLoadTimeLimit + (PlayersAtStart.Count * 0.2f)) * 1000));
            await Task.WhenAll(/*sceneReadyTasks*/_sceneReadyTcs.Values.Select(p => p.Task));
            //if (sceneReadyCts.IsCancellationRequested)
            //    SignalRequestReturnToMenu();

            _packetDispatcher.SendToNearbyPlayers(new SetGameplaySceneSyncFinishedPacket
            {
                SessionGameId = SessionGameId,
                PlayersAtStart = new PlayerSpecificSettingsAtStart
                {
                    ActivePlayerSpecificSettingsAtStart = _playerSpecificSettings.Values.ToArray()
                }
            }, DeliveryMethod.ReliableOrdered);

            // Set scene sync finished
            State = GameplayManagerState.SongLoad;
            _instance.InstanceStateChanged(CountdownState.NotCountingDown, State);


            //Wait for players to have the song ready
            _packetDispatcher.SendToNearbyPlayers(new GetGameplaySongReadyPacket(), DeliveryMethod.ReliableOrdered);
            songReadyCts.CancelAfter((int)((SongLoadTimeLimit + (PlayersAtStart.Count*0.2f)) * 1000));
            await Task.WhenAll(/*songReadyTasks*/ _songReadyTcs.Values.Select(p => p.Task));
            //if (songReadyCts.IsCancellationRequested)
            //    SignalRequestReturnToMenu();

            float StartDelay = 0;
            foreach (var UserId in PlayersAtStart)
            {
                if (_playerRegistry.TryGetPlayer(UserId, out var p))
                {
                    if (!p.InGameplay || p.InLobby)
                        HandlePlayerLeaveGameplay(p);
                    StartDelay = Math.Max(StartDelay, p.Latency.CurrentAverage);
                }

            }

            // Start song and wait for finish
            _songStartTime = _instance.RunTime + SongStartDelay + (StartDelay * 2f);
            State = GameplayManagerState.Gameplay;
            _instance.InstanceStateChanged(CountdownState.NotCountingDown, State);
            _packetDispatcher.SendToNearbyPlayers(new SetSongStartTimePacket
            {
                StartTime = _songStartTime
            }, DeliveryMethod.ReliableOrdered);
            _instance.BeatmapChanged(CurrentBeatmap, CurrentModifiers, true, DateTime.Now.AddSeconds(_songStartTime - _instance.RunTime));
            //_levelFinishedTcs.Values.Select(p => p.Task);
            //levelFinishedTasks
            await Task.WhenAll(_levelFinishedTcs.Values.Select(p => p.Task));
            State = GameplayManagerState.Results;
            _instance.InstanceStateChanged(CountdownState.NotCountingDown, State);
            // Wait at results screen if anyone cleared or skip if the countdown is set to 0.
            if (_levelCompletionResults.Values.Any(result => result.LevelEndStateType == LevelEndStateType.Cleared) && _instance._configuration.CountdownConfig.ResultsScreenTime > 0)
                await Task.Delay((int)(_instance._configuration.CountdownConfig.ResultsScreenTime * 1000), cancellationToken);

            // End gameplay and reset
            SetBeatmap(null, new());
            ResetValues();
            _instance.SetState(MultiplayerGameState.Lobby);
            _packetDispatcher.SendToNearbyPlayers( new ReturnToMenuPacket(), DeliveryMethod.ReliableOrdered);
            _instance.InstanceStateChanged(CountdownState.NotCountingDown, State);
        }

        private void ResetValues()
        {
            State = GameplayManagerState.None;
            _levelFinishedTcs.Clear();
            _sceneReadyTcs.Clear();
            _songReadyTcs.Clear();
            _songStartTime = 0;
            _playerSpecificSettings.Clear();
            _levelCompletionResults.Clear();
            PlayersAtStart.Clear();
        }


        public void HandleGameSceneLoaded(IPlayer player, SetGameplaySceneReadyPacket packet)
        {
            if (_sceneReadyTcs.TryGetValue(player.UserId, out var tcs) && !tcs.Task.IsCompleted)
            {
                _playerSpecificSettings[player.UserId] = packet.PlayerSpecificSettings;
                PlayerSceneReady(player.UserId);
            }

            if (_instance.State != MultiplayerGameState.Game || State == GameplayManagerState.Results || State == GameplayManagerState.None)
            {
                _packetDispatcher.SendToPlayer(player, new ReturnToMenuPacket(), DeliveryMethod.ReliableOrdered);
                HandlePlayerLeaveGameplay(player);
                return;
            }

            if (_instance.State == MultiplayerGameState.Game && State != GameplayManagerState.SceneLoad)
                _packetDispatcher.SendToNearbyPlayers(new SetPlayerDidConnectLatePacket
                {
                    UserId = player.UserId,
                    PlayersAtStart = new PlayerSpecificSettingsAtStart
                    {
                        ActivePlayerSpecificSettingsAtStart = _playerSpecificSettings.Values.ToArray()
                    },
                    SessionGameId = SessionGameId
                }, DeliveryMethod.ReliableOrdered);
        }

        public void HandleGameSongLoaded(IPlayer player)
        {
            if (_songReadyTcs.TryGetValue(player.UserId, out var tcs) && !tcs.Task.IsCompleted)
                PlayerSongReady(player.UserId);
            if (_instance.State != MultiplayerGameState.Game || State == GameplayManagerState.Results || State == GameplayManagerState.None)
            {
                _packetDispatcher.SendToPlayer(player, new ReturnToMenuPacket(), DeliveryMethod.ReliableOrdered);
                HandlePlayerLeaveGameplay(player);
                return;
            }
            if (_instance.State == MultiplayerGameState.Game && State != GameplayManagerState.SongLoad && State != GameplayManagerState.SceneLoad)
                if(_songStartTime != 0)
                    _packetDispatcher.SendToPlayer(player, new SetSongStartTimePacket
                    {
                        StartTime = _songStartTime
                    }, DeliveryMethod.ReliableOrdered);
        }

        public void HandleLevelFinished(IPlayer player, LevelFinishedPacket packet)
        {
            if (_levelFinishedTcs.TryGetValue(player.UserId, out var tcs) && tcs.Task.IsCompleted)
                return;
            _levelCompletionResults[player.UserId] = packet.Results.LevelCompletionResults;
            PlayerFinishLevel(player.UserId);
        }

        object RequestReturnLock = new();
        public void SignalRequestReturnToMenu()
        {
            lock (RequestReturnLock)
            {
                if (_requestReturnToMenuCts != null && !_requestReturnToMenuCts.IsCancellationRequested)
                    _requestReturnToMenuCts.Cancel();
            }
        }

        //will set players tasks as done if they leave gameplay due to disconnect or returning to the menu
        public void HandlePlayerLeaveGameplay(IPlayer player, int Unused = 0)
        {
            LeaveGameplay(player.UserId);
        }

        private void LeaveGameplay(string UserId)
        {
            PlayerFinishLevel(UserId);
            PlayerSceneReady(UserId);
            PlayerSongReady(UserId);
        }

        private void PlayerFinishLevel(string UserId)
        {
            if (_levelFinishedTcs.TryGetValue(UserId, out var tcs) && !tcs.Task.IsCompleted)
                tcs.SetResult();
        }
        private void PlayerSceneReady(string UserId)
        {
            if (_sceneReadyTcs.TryGetValue(UserId, out var tcs) && !tcs.Task.IsCompleted)
                tcs.SetResult();
        }
        private void PlayerSongReady(string UserId)
        {
            if (_songReadyTcs.TryGetValue(UserId, out var tcs) && !tcs.Task.IsCompleted)
                tcs.SetResult();
        }
    }
}
