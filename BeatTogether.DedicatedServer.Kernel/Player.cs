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
        public BeatmapIdentifierNetSerializable? BeatmapIdentifier { get; set; }
        public GameplayModifiers Modifiers { get; set; } = new();

        public PlayerStateBloomFilter State { get; set; } = new();

        public bool IsPlayer => State.Contains("player");
        public bool IsSpectating => State.Contains("spectating");
        public bool WantsToPlayNextLevel => State.Contains("wants_to_play_next_level");
        public bool IsBackgrounded => State.Contains("backgrounded");
        public bool InGameplay => State.Contains("in_gameplay");
        public bool WasActiveAtLevelStart => State.Contains("was_active_at_level_start");
        public bool IsActive => State.Contains("is_active");
        public bool FinishedLevel => State.Contains("finished_level");
        public bool InMenu => State.Contains("in_menu");
        public bool IsModded => State.Contains("modded");

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
