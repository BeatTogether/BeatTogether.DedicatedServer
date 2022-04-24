using System;
using System.Collections.Concurrent;
using System.Net;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Types;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class Player : IPlayer
    {
        public EndPoint Endpoint { get; }
        public IDedicatedInstance Instance { get; }
        public byte ConnectionId { get; }
        public byte RemoteConnectionId => 0;
        public string Secret { get; }
        public string UserId { get; }
        public string UserName { get; }
        public RollingAverage Latency { get; } = new(30);
        public float SyncTime =>
            Math.Min(Instance.RunTime - Latency.CurrentAverage - _syncTimeOffset,
                     Instance.RunTime);
        public int SortIndex { get; set; }
        public byte[] Random { get; set; }
        public byte[] PublicEncryptionKey { get; set; }

        public AvatarData Avatar { get; set; } = new();
        public bool IsReady { get; set; }
        public bool InLobby { get; set; }
        public BeatmapIdentifier? BeatmapIdentifier { get; set; }
        public GameplayModifiers Modifiers { get; set; } = new();

        public PlayerStateHash State { get; set; } = new();

        public bool IsManager => UserId == Instance.Configuration.ManagerId;
        public bool CanRecommendBeatmaps => true;
        public bool CanRecommendModifiers =>
            Instance.Configuration.GameplayServerControlSettings is Enums.GameplayServerControlSettings.AllowModifierSelection or Enums.GameplayServerControlSettings.All;
        public bool CanKickVote => UserId == Instance.Configuration.ManagerId;
        public bool CanInvite =>
            Instance.Configuration.DiscoveryPolicy is Enums.DiscoveryPolicy.WithCode or Enums.DiscoveryPolicy.Public;

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
        private ConcurrentDictionary<string, EntitlementStatus> _entitlements = new();

        public Player(EndPoint endPoint, IDedicatedInstance instance,
            byte connectionId, string secret, string userId, string userName)
        {
            Endpoint = endPoint;
            Instance = instance;
            ConnectionId = connectionId;
            Secret = secret;
            UserId = userId;
            UserName = userName;
        }

        public EntitlementStatus GetEntitlement(string levelId)
            => _entitlements.TryGetValue(levelId, out var value) ? value : EntitlementStatus.Unknown;

        public void SetEntitlement(string levelId, EntitlementStatus entitlement)
            => _entitlements[levelId] = entitlement;
    }
}
