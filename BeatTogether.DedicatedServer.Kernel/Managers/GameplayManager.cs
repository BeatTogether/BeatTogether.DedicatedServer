using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
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
        public GameplayManagerState State { get; private set; }

        private const float SongStartDelay = 2f;

        private ConcurrentDictionary<string, PlayerSpecificSettings> _playerSpecificSettings = new();
        private ConcurrentDictionary<string, LevelCompletionResults> _levelCompletionResults = new();

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

        public async void StartSong(BeatmapIdentifier beatmap, CancellationToken cancellationToken)
        {
            // Reset
            SessionGameId = Guid.NewGuid().ToString();
            _playerSpecificSettings.Clear();
            _levelCompletionResults.Clear();

            IEnumerable<Task<SetGameplaySceneReadyPacket>> sceneReadyTasks = _playerRegistry.Players.Select(player => player.WaitForSceneReady(cancellationToken));

            // Ask for scene ready
            var getGameplaySceneReady = new GetGameplaySceneReadyPacket();
            _packetDispatcher.SendToNearbyPlayers(_playerRegistry.GetPlayer(_server.ManagerId), getGameplaySceneReady, LiteNetLib.DeliveryMethod.ReliableOrdered);

            // Wait for scene ready
            await Task.WhenAll(sceneReadyTasks.ToArray());

            // Set scene sync finished
            var setGameplaySceneSyncFinished = new SetGameplaySceneSyncFinishedPacket
            {
                SessionGameId = SessionGameId,
                PlayersAtStart = new PlayerSpecificSettingsAtStart
                {
                    ActivePlayerSpecificSettingsAtStart = _playerSpecificSettings.Values.ToList()
                }
            };
            _packetDispatcher.SendToNearbyPlayers(_playerRegistry.GetPlayer(_server.ManagerId), setGameplaySceneSyncFinished, LiteNetLib.DeliveryMethod.ReliableOrdered);

            IEnumerable<Task> songReadyTasks = _playerRegistry.Players.Select(player => player.WaitForSongReady(cancellationToken));

            // Ask for song ready
            var getGameplaySongReady = new GetGameplaySongReadyPacket();
            _packetDispatcher.SendToNearbyPlayers(_playerRegistry.GetPlayer(_server.ManagerId), getGameplaySongReady, LiteNetLib.DeliveryMethod.ReliableOrdered);

            // Wait for song ready
            await Task.WhenAll(songReadyTasks.ToArray());

            // Start song
            var setSongStartTime = new SetSongStartTimePacket
            {
                StartTime = _server.RunTime + SongStartDelay
            };
            _packetDispatcher.SendToNearbyPlayers(_playerRegistry.GetPlayer(_server.ManagerId), setSongStartTime, LiteNetLib.DeliveryMethod.ReliableOrdered);

            // TODO: wait for players to finish
        }

        public void HandleGameSceneLoaded(IPlayer player, SetGameplaySceneReadyPacket packet)
        {
            _playerSpecificSettings[player.UserId] = packet.PlayerSpecificSettings;
        }

        public void HandleGameSongLoaded(IPlayer player)
        {
            // This just exists for now
        }
    }
}
