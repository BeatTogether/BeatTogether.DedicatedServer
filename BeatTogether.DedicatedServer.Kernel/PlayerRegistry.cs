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

        private readonly object PlayerDictionaries_Lock = new();
        private readonly Dictionary<EndPoint, IPlayer> _playersByRemoteEndPoint = new();
        private readonly Dictionary<byte, IPlayer> _playersByConnectionId = new();
        private readonly Dictionary<string, IPlayer> _playersByUserId = new();

        private readonly Dictionary<string, Core.Abstractions.IPlayer> _pendingPlayerSessionData = new();



        public void AddExtraPlayerSessionData(Core.Abstractions.IPlayer playerSessionData)
        {
            _pendingPlayerSessionData[playerSessionData.PlayerSessionId] = playerSessionData;
        }

        public bool RemoveExtraPlayerSessionDataAndApply(Core.Abstractions.IPlayer playerSessionData)
        {

            if (_pendingPlayerSessionData.Remove(playerSessionData.PlayerSessionId, out var Value))
            {
                playerSessionData.PlayerClientVersion = Value.PlayerClientVersion;
                playerSessionData.PlayerPlatform = Value.PlayerPlatform;
                playerSessionData.PlatformUserId = Value.PlatformUserId;
                return true;
            }
            playerSessionData.PlayerClientVersion = new System.Version("1.0.0");
            playerSessionData.PlayerPlatform = Core.Enums.Platform.Test;
            playerSessionData.PlatformUserId = "ERROR";
            return false;
        }


        public bool RemoveExtraPlayerSessionData(string playerSessionId)
        {
            return _pendingPlayerSessionData.Remove(playerSessionId, out _);
        }


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
                if (_playersByUserId.TryAdd(player.HashedUserId, player))
                {
                    _playersByRemoteEndPoint.TryAdd(player.Endpoint, player);
                    _playersByConnectionId.TryAdd(player.ConnectionId, player);
                    _PlayerCount++;
                    MillisBetweenPoseSyncStatePackets = _PlayerCount == 1 ? 1000 : (long)(0.94 * _PlayerCount + 15);
                    MillisBetweenScoreSyncStatePackets = _PlayerCount == 1 ? 1000 : (long)(1.5 * _PlayerCount + 20);
                    return true;
                }
            }
            return false;
        }

        public void RemovePlayer(IPlayer player)
        {
            lock (PlayerDictionaries_Lock)
            {
                if (_playersByUserId.Remove(player.HashedUserId, out _))
                {
                    _playersByRemoteEndPoint.Remove(player.Endpoint, out _);
                    _playersByConnectionId.Remove(player.ConnectionId, out _);
                    _PlayerCount--;
                    MillisBetweenPoseSyncStatePackets = _PlayerCount == 1 ? 1000 : (long)(0.94 * _PlayerCount + 15);
                    MillisBetweenScoreSyncStatePackets = _PlayerCount == 1 ? 1000 : (long)(1.5 * _PlayerCount + 20);
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



        private long MillisBetweenPoseSyncStatePackets = 0;
        public long GetMillisBetweenPoseSyncStateDeltaPackets()
        {
            return MillisBetweenPoseSyncStatePackets;
        }

        private long MillisBetweenScoreSyncStatePackets = 0;
        public long GetMillisBetweenScoreSyncStateDeltaPackets()
        {
            return MillisBetweenScoreSyncStatePackets;
        }
    }
}
