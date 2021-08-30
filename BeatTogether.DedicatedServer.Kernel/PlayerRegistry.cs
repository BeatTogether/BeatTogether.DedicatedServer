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
        public List<IPlayer> Players { get => _playersByRemoteEndPoint.Values.ToList(); }
        private readonly ConcurrentDictionary<EndPoint, IPlayer> _playersByRemoteEndPoint = new();

        public void AddPlayer(IPlayer player) =>
            _playersByRemoteEndPoint.TryAdd(player.NetPeer.EndPoint, player);

        public void RemovePlayer(IPlayer player) =>
            _playersByRemoteEndPoint.TryRemove(player.NetPeer.EndPoint, out _);

        public IPlayer GetPlayer(EndPoint remoteEndPoint) =>
            _playersByRemoteEndPoint[remoteEndPoint];

        public bool TryGetPlayer(EndPoint remoteEndPoint, [MaybeNullWhen(false)] out IPlayer player) =>
            _playersByRemoteEndPoint.TryGetValue(remoteEndPoint, out player);

        public int PlayerCount => _playersByRemoteEndPoint.Count;
    }
}
