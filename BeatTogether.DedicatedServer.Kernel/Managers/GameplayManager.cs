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
        private const float ResultsScreenTime = 20f; //changing this to 20 sec as on quest i think it is that
        private const float SceneLoadTimeLimit = 10.0f;
        private const float SongLoadTimeLimit = 10.0f;

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
            Console.WriteLine("Now running GameplayManager");
            
            if (State != GameplayManagerState.None)
            {
                _requestReturnToMenuCts!.Cancel();
                return;
            }
            _instance.SetState(MultiplayerGameState.Game);

            //Reset these values Here incase they did not get reset after the prev loop
            ResetValues(beatmap, modifiers);

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
                Console.WriteLine("Player: " + p.UserName + " has a level finish task, ManagerID: " + _instance.Configuration.ManagerId);
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
            sceneReadyCts.CancelAfter((int)(SceneLoadTimeLimit * 1000)); //after 10 sec cancel, should find a way to end gameplay if this is cancled
            await Task.WhenAll(sceneReadyTasks);
            Console.WriteLine("Scene loaded, ManagerID: " + _instance.Configuration.ManagerId);

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
            songReadyCts.CancelAfter((int)(SongLoadTimeLimit * 1000)); //after 10 sec cancel, should find a way to end gameplay if this is cancled
            await Task.WhenAll(songReadyTasks);
            Console.WriteLine("Song ready, ManagerID: " + _instance.Configuration.ManagerId);

            // If no players are actually playing, or not all players are not in the lobby(if at least one player is then true)
            if (loadingPlayers.All(player => !player.InGameplay) || !loadingPlayers.All(player => !player.InLobby))
            {
                _requestReturnToMenuCts.Cancel(); //this will cancel the gameplay if someone is in the lobby
            }
            /*                                    //this will continue the gameplay if someone is in the lobby still
            foreach (var p in loadingPlayers) //stops the instance from soft-locking
            {
                if (p.InLobby)
                {
                    Console.WriteLine(p.UserName + " is in lobby still when game should be starting");
                    HandlePlayerDisconnected(p); //makes sure that the player in the lobby does not cause the instance to hang
                }
            }
            */
            Console.WriteLine("Starting beatmap in lobby: " + _instance.Configuration.SongSelectionMode + ", LobbySize: " + _instance.Configuration.MaxPlayerCount + ", ManagerID: " + _instance.Configuration.ManagerId);
            
            // Start song and wait for finish
            State = GameplayManagerState.Gameplay;
            _songStartTime = _instance.RunTime + SongStartDelay;
            _packetDispatcher.SendToNearbyPlayers(new SetSongStartTimePacket
            {
                StartTime = _songStartTime
            }, DeliveryMethod.ReliableOrdered);

            await Task.WhenAll(levelFinishedTasks);
            Console.WriteLine("Results screen, in lobby: " + _instance.Configuration.SongSelectionMode + ", LobbySize: " + _instance.Configuration.MaxPlayerCount + ", ManagerID: " + _instance.Configuration.ManagerId);
            State = GameplayManagerState.Results;

            // Wait at results screen if anyone cleared
            if (_levelCompletionResults.Values.Any(result => result.LevelEndStateType == LevelEndStateType.Cleared))
            {
                await Task.Delay((int)(ResultsScreenTime * 1000));
            }

            // End gameplay and reset
            ResetValues(null, null);
            State = GameplayManagerState.None;
            Console.WriteLine("Gameplay manager ending, sending players back to lobby: " + _instance.Configuration.SongSelectionMode + ", LobbySize: " + _instance.Configuration.MaxPlayerCount + ", ManagerID: " + _instance.Configuration.ManagerId);
            _packetDispatcher.SendToNearbyPlayers(new ReturnToMenuPacket(), DeliveryMethod.ReliableOrdered); //game seems to ignore this
            _instance.SetState(MultiplayerGameState.Lobby);
        }

        private void ResetValues(BeatmapIdentifier? map, GameplayModifiers? modifiers)
        {
            CurrentBeatmap = map;
            CurrentModifiers = modifiers;
            _levelFinishedTcs.Clear();
            _sceneReadyTcs.Clear();
            _songReadyTcs.Clear();
            _songStartTime = 0;
            _playerSpecificSettings.Clear();
            _levelCompletionResults.Clear();
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
                _packetDispatcher.SendToPlayer(player, new ReturnToMenuPacket(), DeliveryMethod.ReliableOrdered); //doubt this does anything
                HandlePlayerDisconnected(player);//Added this incase this is why server stops
            }
            PlayerSceneReady(player);
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
                _packetDispatcher.SendToPlayer(player, new ReturnToMenuPacket(), DeliveryMethod.ReliableOrdered); //doubt this does anything
                HandlePlayerDisconnected(player);//Added this incase this is why server stops
            }
            PlayerSongReady(player);
        }

        public void HandleLevelFinished(IPlayer player, LevelFinishedPacket packet)
        {
            if (_levelFinishedTcs.TryGetValue(player.UserId, out var tcs) && tcs.Task.IsCompleted)
            {
                return;
            }
            _levelCompletionResults[player.UserId] = packet.Results.LevelCompletionResults;
            PlayerFinishLevel(player);
        }

        public void SignalRequestReturnToMenu()
            => _requestReturnToMenuCts?.Cancel();

        private void HandlePlayerDisconnected(IPlayer player)
        {
            PlayerFinishLevel(player);
            PlayerSceneReady(player);
            PlayerSongReady(player);
        }

        private void PlayerFinishLevel(IPlayer player)
        {
            var levelFinished = _levelFinishedTcs.GetOrAdd(player.UserId, _ => new());
            if (!levelFinished.Task.IsCompleted)
                levelFinished.SetResult();
        }
        private void PlayerSceneReady(IPlayer player)
        {
            var sceneReady = _sceneReadyTcs.GetOrAdd(player.UserId, _ => new());
            if (!sceneReady.Task.IsCompleted)
                sceneReady.SetResult();
        }
        private void PlayerSongReady(IPlayer player)
        {
            var songReady = _songReadyTcs.GetOrAdd(player.UserId, _ => new());
            if (!songReady.Task.IsCompleted)
                songReady.SetResult();
        }



    }
}
