using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using LiteNetLib.Utils;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.Managers
{
    public sealed class GameplayManager : IGameplayManager
    {
        private ConcurrentBag<IPlayer> _inactivePlayers = new();
        private ConcurrentBag<IPlayer> _activePlayers = new();

        private ConcurrentDictionary<string, PlayerSpecificSettings> _playerSpecificSettings = new();
        private ConcurrentDictionary<string, LevelCompletionResults> _levelCompletionResults = new();

        private float _songStartTime;
        private bool _gameInit = true;
        private bool _sceneLoaded;
        private bool _songLoaded;

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

        public void Update()
        {
            if (_server.State != MultiplayerGameState.Game)
            {
                if (!_gameInit)
                {
                    _inactivePlayers.Clear();
                    _activePlayers.Clear();
                    _playerSpecificSettings.Clear();
                    _levelCompletionResults.Clear();
                    _gameInit = true;
                    _sceneLoaded = false;
                    _songLoaded = false;

                    _packetDispatcher.SendToNearbyPlayers(_playerRegistry.GetPlayer(_server.ManagerId), new ReturnToMenuPacket(), LiteNetLib.DeliveryMethod.ReliableOrdered);
                }
            }

            if (_gameInit)
            {
                foreach(IPlayer player in _playerRegistry.Players)
                {
                    _inactivePlayers.Add(player);
                }
                _gameInit = false;
            }
        }

        public void HandleGameSceneLoaded(IPlayer player, SetGameplaySceneReadyPacket packet)
        {

        }

        public void HandleGameSongLoaded(IPlayer player, SetGameplaySongReadyPacket packet)
        {

        }
    }
}
