using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class PlayerRegistry : IPlayerRegistry
    {
        public List<IPlayer> Players { get => _playersByUserId.Values.ToList(); }

        private readonly ConcurrentDictionary<EndPoint, IPlayer> _playersByRemoteEndPoint = new();
        private readonly ConcurrentDictionary<byte, IPlayer> _playersByConnectionId = new();    //Max of 128 players in a lobby (if using connectionID's) I dislike connection IDs
        private readonly ConcurrentDictionary<string, IPlayer> _playersByUserId = new();

        public void AddPlayer(IPlayer player)
        {
            _playersByRemoteEndPoint.TryAdd(player.Endpoint, player);
            _playersByUserId[player.UserId] = player;
            _playersByConnectionId[player.ConnectionId] = player;
        }

        public void RemovePlayer(IPlayer player)
        {
            _playersByRemoteEndPoint.TryRemove(player.Endpoint, out _);
            _playersByUserId.TryRemove(player.UserId, out _);
            _playersByConnectionId.TryRemove(player.ConnectionId, out _);
        }

        public IPlayer GetPlayer(EndPoint remoteEndPoint) =>
            _playersByRemoteEndPoint[remoteEndPoint];

        public IPlayer GetPlayer(byte connectionId) =>
            _playersByConnectionId[connectionId];

        public IPlayer GetPlayer(string userId) =>
            _playersByUserId[userId];

        public bool TryGetPlayer(EndPoint remoteEndPoint, [MaybeNullWhen(false)] out IPlayer player) =>
            _playersByRemoteEndPoint.TryGetValue(remoteEndPoint, out player);

        public bool TryGetPlayer(byte connectionId, [MaybeNullWhen(false)] out IPlayer player) =>
            _playersByConnectionId.TryGetValue(connectionId, out player);

        public bool TryGetPlayer(string userId, [MaybeNullWhen(false)] out IPlayer player) =>
            _playersByUserId.TryGetValue(userId, out player);
    }
}
