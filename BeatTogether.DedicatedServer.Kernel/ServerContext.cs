using BeatTogether.DedicatedServer.Kernel.Models;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class ServerContext : IServerContext
    {
        public List<IPlayer> Players { get => _playersByUserId.Values.ToList(); }
        public string Secret { get; set; }
        public string ManagerId { get; set; }
        public GameplayServerConfiguration Configuration { get; set; }
        public PlayersPermissionConfiguration Permissions { get; set; } = new();
        public MultiplayerGameState State { get; set; } = MultiplayerGameState.Lobby;

        private readonly ConcurrentDictionary<byte, IPlayer> _playersByConnectionId = new();
        private readonly ConcurrentDictionary<string, IPlayer> _playersByUserId = new();

        public void AddPlayer(IPlayer player)
        {
            _playersByUserId[player.UserId] = player;
            _playersByConnectionId[player.ConnectionId] = player;
        }

        public void RemovePlayer(IPlayer player)
        {
            _playersByUserId.TryRemove(player.UserId, out _);
            _playersByConnectionId.TryRemove(player.ConnectionId, out _);
        }

        public IPlayer GetPlayer(byte connectionId) =>
            _playersByConnectionId[connectionId];

        public IPlayer GetPlayer(string userId) =>
            _playersByUserId[userId];

        public bool TryGetPlayer(byte connectionId, [MaybeNullWhen(false)] out IPlayer player) =>
            _playersByConnectionId.TryGetValue(connectionId, out player);

        public bool TryGetPlayer(string userId, [MaybeNullWhen(false)] out IPlayer player) =>
            _playersByUserId.TryGetValue(userId, out player);
    }
}
