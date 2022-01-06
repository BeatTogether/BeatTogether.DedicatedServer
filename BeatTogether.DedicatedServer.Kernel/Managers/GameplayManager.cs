using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using BeatTogether.LiteNetLib.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        private CancellationTokenSource? _requestReturnToMenuCts;

        private readonly IDedicatedServer _server;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IPacketDispatcher _packetDispatcher;

        private readonly ConcurrentDictionary<string, PlayerSpecificSettings> _playerSpecificSettings = new();
        private readonly ConcurrentDictionary<string, LevelCompletionResults> _levelCompletionResults = new();

        private readonly ConcurrentDictionary<string, TaskCompletionSource> _levelFinishedTcs = new();
        private readonly ConcurrentDictionary<string, TaskCompletionSource> _sceneReadyTcs = new();
        private readonly ConcurrentDictionary<string, TaskCompletionSource> _songReadyTcs = new();

        public GameplayManager(
            IDedicatedServer server,
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

            _server.SetState(MultiplayerGameState.Game);
            CurrentBeatmap = beatmap;
            CurrentModifiers = modifiers;

            // Reset
            SessionGameId = Guid.NewGuid().ToString();
            _playerSpecificSettings.Clear();
            _levelCompletionResults.Clear();
            _levelFinishedTcs.Clear();
            _sceneReadyTcs.Clear();
            _songReadyTcs.Clear();
            _songStartTime = 0;
            _requestReturnToMenuCts = new CancellationTokenSource();

            State = GameplayManagerState.SceneLoad;

            var loadingPlayers = _playerRegistry.Players; // During scene and song, only wait for players that were already connected

            // Create level finished tasks (players may send these at any time during gameplay)
            var levelFinishedCts = new CancellationTokenSource();
            var linkedLevelFinishedCts = CancellationTokenSource.CreateLinkedTokenSource(levelFinishedCts.Token, _requestReturnToMenuCts.Token);
            IEnumerable<Task> levelFinishedTasks = _playerRegistry.Players.Select(p => _levelFinishedTcs.GetOrAdd(p.UserId, _ => new()).Task);

            // Create scene ready tasks
            var sceneReadyCts = new CancellationTokenSource();
            var linkedSceneReadyCts = CancellationTokenSource.CreateLinkedTokenSource(sceneReadyCts.Token, _requestReturnToMenuCts.Token);
            IEnumerable<Task> sceneReadyTasks = loadingPlayers.Select(p => _sceneReadyTcs.GetOrAdd(p.UserId, _ => new()).Task);

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
            IEnumerable<Task> songReadyTasks = loadingPlayers.Select(p => _songReadyTcs.GetOrAdd(p.UserId, _ => new()).Task);

            // Wait for song ready
            _packetDispatcher.SendToNearbyPlayers(new GetGameplaySongReadyPacket(), DeliveryMethod.ReliableOrdered);
            songReadyCts.CancelAfter((int)(SongLoadTimeLimit * 1000));
            await Task.WhenAll(sceneReadyTasks);

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
            await Task.WhenAll(levelFinishedTasks);

            State = GameplayManagerState.Results;

            // Wait at results screen if anyone cleared
            if (_levelCompletionResults.Values.Any(result => result.LevelEndStateType == LevelEndStateType.Cleared))
                await Task.Delay((int)(ResultsScreenTime * 1000));

            // End gameplay
            State = GameplayManagerState.None;
            CurrentBeatmap = null;
            CurrentModifiers = null;
            _packetDispatcher.SendToNearbyPlayers(new ReturnToMenuPacket(), DeliveryMethod.ReliableOrdered);
            _server.SetState(MultiplayerGameState.Lobby);
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

            _sceneReadyTcs.GetOrAdd(player.UserId, _ => new()).SetResult();
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

            _songReadyTcs.GetOrAdd(player.UserId, _ => new()).SetResult();

        }

        public void HandleLevelFinished(IPlayer player, LevelFinishedPacket packet)
        {
            _levelCompletionResults[player.UserId] = packet.Results.LevelCompletionResults;
            _levelFinishedTcs.GetOrAdd(player.UserId, _ => new()).SetResult();
        }

        public void SignalRequestReturnToMenu()
            => _requestReturnToMenuCts?.Cancel();

        private void HandleClientDisconnect(EndPoint endPoint, DisconnectReason reason)
        {
            var p = _playerRegistry.GetPlayer(endPoint);
            _levelFinishedTcs.GetOrAdd(p.UserId, _ => new()).SetResult();
            _sceneReadyTcs.GetOrAdd(p.UserId, _ => new()).SetResult();
            _songReadyTcs.GetOrAdd(p.UserId, _ => new()).SetResult();
        }
    }
}
