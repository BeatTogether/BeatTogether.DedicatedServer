using System;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Types;
using BeatTogether.DedicatedServer.Messaging.Models;
using LiteNetLib;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class Player : IPlayer
    {
        public NetPeer NetPeer { get; }
        public IMatchmakingServer MatchmakingServer { get; }
        public byte ConnectionId { get; }
        public string Secret { get; }
        public string UserId { get; }
        public string UserName { get; }
        public RollingAverage Latency { get; } = new(30);
        public float SyncTime =>
            Math.Min(MatchmakingServer.RunTime - Latency.CurrentAverage - _syncTimeOffset,
                     MatchmakingServer.RunTime);
        public int SortIndex { get; set; }
        public AvatarData? AvatarData { get; set; }
        public bool IsReady { get; set; }

        private const float _syncTimeOffset = 0.06f;

        public Player(NetPeer netPeer, IMatchmakingServer matchmakingServer,
            byte connectionId, string secret, string userId, string userName)
        {
            NetPeer = netPeer;
            MatchmakingServer = matchmakingServer;
            ConnectionId = connectionId;
            Secret = secret;
            UserId = userId;
            UserName = userName;
        }
            
    }
}
