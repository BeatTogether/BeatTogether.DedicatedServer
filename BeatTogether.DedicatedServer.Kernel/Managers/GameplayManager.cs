using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
//using BeatTogether.LiteNetLib.Enums;
using Serilog;
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

        private const long SongStartDelay = 500L;
        private const float SceneLoadTimeLimit = 15.0f;
        private const float SongLoadTimeLimit = 15.0f;

        public long _songStartTime { get; private set; }
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


        private CancellationTokenSource? levelFinishedCts = null;
        private CancellationTokenSource? linkedLevelFinishedCts = null;
        private CancellationTokenSource? sceneReadyCts = null;
        private CancellationTokenSource? linkedSceneReadyCts = null;
        private CancellationTokenSource? songReadyCts = null;
        private CancellationTokenSource? linkedSongReadyCts = null;

        private readonly ILogger _logger = Log.ForContext<GameplayManager>();

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
            _requestReturnToMenuCts = new CancellationTokenSource();
            if (State != GameplayManagerState.None || CurrentBeatmap == null)
            {
                _requestReturnToMenuCts!.Cancel();
                return;
            }
            _instance.SetState(MultiplayerGameState.Game);

            //Reset
            ResetValues();
            //_instance.InstanceStateChanged(CountdownState.NotCountingDown, State);
            SessionGameId = Guid.NewGuid().ToString();

            State = GameplayManagerState.SceneLoad;
            foreach (var player in _playerRegistry.Players)//Array of players that are playing at the start
            {
                if (!player.IsSpectating && !player.ForceLateJoin)
                    PlayersAtStart.Add(player.UserId);
            }

            // Create level finished tasks (players may send these at any time during gameplay)
            levelFinishedCts = new();
            linkedLevelFinishedCts = CancellationTokenSource.CreateLinkedTokenSource(levelFinishedCts.Token, _requestReturnToMenuCts.Token);
            PlayersAtStart.ForEach(p =>
            {
                var task = _levelFinishedTcs.GetOrAdd(p, _ => new());
                linkedLevelFinishedCts.Token.Register(() => { if (!task.Task.IsCompleted) PlayerFinishLevel(p); });
            });

            // Create scene ready tasks
            sceneReadyCts = new();
            linkedSceneReadyCts = CancellationTokenSource.CreateLinkedTokenSource(sceneReadyCts.Token, _requestReturnToMenuCts.Token);
            PlayersAtStart.ForEach(p =>
            {
                var task = _sceneReadyTcs.GetOrAdd(p, _ => new());
                linkedSceneReadyCts.Token.Register(() => { if (!task.Task.IsCompleted) LeaveGameplay(p); });
            });

            // Create song ready tasks
            songReadyCts = new();
            linkedSongReadyCts = CancellationTokenSource.CreateLinkedTokenSource(songReadyCts.Token, _requestReturnToMenuCts.Token);
            PlayersAtStart.ForEach(p =>
            {
                var task = _songReadyTcs.GetOrAdd(p, _ => new());
                linkedSongReadyCts.Token.Register(() => { if (!task.Task.IsCompleted) LeaveGameplay(p); });
            });

            // Wait for scene ready
            _packetDispatcher.SendToNearbyPlayers(new GetGameplaySceneReadyPacket(), IgnoranceChannelTypes.Reliable);
            sceneReadyCts.CancelAfter((int)((SceneLoadTimeLimit + (PlayersAtStart.Count * 0.3f)) * 1000));
            await Task.WhenAll(_sceneReadyTcs.Values.Select(p => p.Task));

            _packetDispatcher.SendToNearbyPlayers(new SetGameplaySceneSyncFinishedPacket
            {
                SessionGameId = SessionGameId,
                PlayersAtStart = new PlayerSpecificSettingsAtStart
                {
                    ActivePlayerSpecificSettingsAtStart = _playerSpecificSettings.Values.ToArray()
                }
            }, IgnoranceChannelTypes.Reliable);

            // Set scene sync finished
            State = GameplayManagerState.SongLoad;

            //Wait for players to have the song ready
            _packetDispatcher.SendToNearbyPlayers(new GetGameplaySongReadyPacket(), IgnoranceChannelTypes.Reliable);
            songReadyCts.CancelAfter((int)((SongLoadTimeLimit + (PlayersAtStart.Count*0.3f)) * 1000));
            await Task.WhenAll(_songReadyTcs.Values.Select(p => p.Task));

            long StartDelay = 0;
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
            _songStartTime = (_instance.RunTime + SongStartDelay + (StartDelay * 2));
            _logger.Verbose($"SongStartTime: {_songStartTime} RunTime: {_instance.RunTime}");

            State = GameplayManagerState.Gameplay;

            _packetDispatcher.SendToNearbyPlayers(new SetSongStartTimePacket
            {
                StartTime = _songStartTime
            }, IgnoranceChannelTypes.Reliable);

            //Initiates the song start process for forced late joiners
            foreach(IPlayer p in _playerRegistry.Players)
            {
                if (p.ForceLateJoin)
                {
                    _packetDispatcher.SendToPlayer(p, new Messaging.Packets.MultiplayerSession.MenuRpc.StartLevelPacket
                    {
                        Beatmap = CurrentBeatmap,
                        Modifiers = CurrentModifiers,
                        StartTime = _songStartTime
                    }, IgnoranceChannelTypes.Reliable);
                }
            }

            await Task.WhenAll(_levelFinishedTcs.Values.Select(p => p.Task));

            State = GameplayManagerState.Results;

            if (_levelCompletionResults.Values.Any(result => result.LevelEndStateType == LevelEndStateType.Cleared) && _instance._configuration.CountdownConfig.ResultsScreenTime > 0)
                await Task.Delay((int)(_instance._configuration.CountdownConfig.ResultsScreenTime * 1000), cancellationToken);

            // End gameplay and reset
            SetBeatmap(null, new());
            ResetValues();
            _instance.SetState(MultiplayerGameState.Lobby);
            _packetDispatcher.SendToNearbyPlayers( new ReturnToMenuPacket(), IgnoranceChannelTypes.Reliable);
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
            if (State == GameplayManagerState.SceneLoad && _sceneReadyTcs.TryGetValue(player.UserId, out var tcs) && !tcs.Task.IsCompleted)
            {
                _playerSpecificSettings[player.UserId] = packet.PlayerSpecificSettings;
                PlayerSceneReady(player.UserId);
                return;
            }

            if (_instance.State != MultiplayerGameState.Game || State == GameplayManagerState.Results || State == GameplayManagerState.None) //Returns player to lobby
            {
                _packetDispatcher.SendToPlayer(player, new ReturnToMenuPacket(), IgnoranceChannelTypes.Reliable);
                LeaveGameplay(player.UserId);
                return;
            }

            if (State != GameplayManagerState.SceneLoad)//Late joiners are sent players at start
            {
                _packetDispatcher.SendToNearbyPlayers(new SetPlayerDidConnectLatePacket
                {
                    UserId = player.UserId,
                    PlayersAtStart = new PlayerSpecificSettingsAtStart
                    {
                        ActivePlayerSpecificSettingsAtStart = _playerSpecificSettings.Values.ToArray()
                    },
                    SessionGameId = SessionGameId
                }, IgnoranceChannelTypes.Reliable);
                _packetDispatcher.SendToPlayer(player, new GetGameplaySongReadyPacket(), IgnoranceChannelTypes.Reliable);
            } 
        }

        public void HandleGameSongLoaded(IPlayer player)
        {
            if (State == GameplayManagerState.SongLoad && _songReadyTcs.TryGetValue(player.UserId, out var tcs) && !tcs.Task.IsCompleted)
            {
                PlayerSongReady(player.UserId);
                return;
            }
            if (_instance.State != MultiplayerGameState.Game || State == GameplayManagerState.Results || State == GameplayManagerState.None) //Player is sent back to lobby
            {
                _packetDispatcher.SendToPlayer(player, new ReturnToMenuPacket(), IgnoranceChannelTypes.Reliable);
                HandlePlayerLeaveGameplay(player);
                return;
            }
            if (State != GameplayManagerState.SceneLoad) //Late joiners get sent start time
                if(_songStartTime != 0)
                    _packetDispatcher.SendToPlayer(player, new SetSongStartTimePacket
                    {
                        StartTime = _songStartTime
                    }, IgnoranceChannelTypes.Reliable);
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
        public void HandlePlayerLeaveGameplay(IPlayer player)
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
