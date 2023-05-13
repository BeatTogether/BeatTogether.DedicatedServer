using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using BeatTogether.DedicatedServer.Kernel.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class PlayerRegistry : IPlayerRegistry
    {
        
        public IPlayer[] Players { get => GetPlayers(); }

        private object PlayerDictionaries_Lock = new();
        private readonly Dictionary<EndPoint, IPlayer> _playersByRemoteEndPoint = new();
        private readonly Dictionary<byte, IPlayer> _playersByConnectionId = new();
        private readonly Dictionary<string, IPlayer> _playersByUserId = new();

        private int _PlayerCount = 0;

        public int GetPlayerCount()
        {
            lock(PlayerDictionaries_Lock)
            {
                return _PlayerCount;
            }
        }

        private IPlayer[] GetPlayers()
        {
            lock (PlayerDictionaries_Lock)
            {
                return _playersByUserId.Values.ToArray();
            }
        }


        public bool AddPlayer(IPlayer player)
        {
            lock (PlayerDictionaries_Lock)
            {
                if (_playersByUserId.TryAdd(player.UserId, player))
                {
                    _playersByRemoteEndPoint.TryAdd(player.Endpoint, player);
                    _playersByConnectionId.TryAdd(player.ConnectionId, player);
                    _PlayerCount++;
                    lock (MillisBetweenSyncStatePackets_Lock)
                    {
                        MillisBetweenSyncStatePackets = (int)(0.84 * _PlayerCount + 15.789);
                    }
                    return true;
                }
            }
            return false;
        }

        public void RemovePlayer(IPlayer player)
        {
            lock (PlayerDictionaries_Lock)
            {
                if (_playersByUserId.Remove(player.UserId, out _))
                {
                    _playersByRemoteEndPoint.Remove(player.Endpoint, out _);
                    _playersByConnectionId.Remove(player.ConnectionId, out _);
                    _PlayerCount--;
                    lock (MillisBetweenSyncStatePackets_Lock)
                    {
                        MillisBetweenSyncStatePackets = (int)(0.84 * _PlayerCount + 15.789);
                    }
                }
            }
        }

        public bool TryGetPlayer(EndPoint remoteEndPoint, [MaybeNullWhen(false)] out IPlayer player)
        {
            lock (PlayerDictionaries_Lock)
            {
                return _playersByRemoteEndPoint.TryGetValue(remoteEndPoint, out player);
            }
        }
        public bool TryGetPlayer(byte connectionId, [MaybeNullWhen(false)] out IPlayer player)
        {
            lock (PlayerDictionaries_Lock)
            {
                return _playersByConnectionId.TryGetValue(connectionId, out player);
            }
        }
        public bool TryGetPlayer(string userId, [MaybeNullWhen(false)] out IPlayer player)
        {
            lock (PlayerDictionaries_Lock)
            {
                return _playersByUserId.TryGetValue(userId, out player);
            }
        }

        private readonly object MillisBetweenSyncStatePackets_Lock = new();
        private int MillisBetweenSyncStatePackets = 0;
        public int GetMillisBetweenSyncStatePackets()
        {
            lock (MillisBetweenSyncStatePackets_Lock)
            {
                return MillisBetweenSyncStatePackets;
            }
        }
    }
}
