using System;
using System.Collections.Concurrent;
using System.Net;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Enums;
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
        public string? PlayerSessionId { get; }
        public object LatencyLock { get; set; } = new();
        public RollingAverage Latency { get; } = new(30);
        public float SyncTime =>
            Math.Min(Instance.RunTime - Latency.CurrentAverage - _syncTimeOffset,
                     Instance.RunTime);
        public object SortLock { get; set; } = new();
        public int SortIndex { get; set; }
        public byte[]? Random { get; set; }
        public byte[]? PublicEncryptionKey { get; set; }
        public string ClientVersion { get; set; } = "Pre-1.29";
        public Platform Platform { get; set; } = Platform.Test; //Unknown
        public string PlatformUserId { get; set; } = "";
        public object PlayerIdentityLock { get; set; } = new();
        public AvatarData Avatar { get; set; } = new();
        public object ReadyLock { get; set; } = new();
        public bool IsReady { get; set; }
        public object InLobbyLock { get; set; } = new();
        public bool InLobby { get; set; }

        public object BeatmapLock { get; set; } = new();
        public BeatmapIdentifier? BeatmapIdentifier { get; set; } = null;
        public object ModifiersLock { get; set; } = new();
        public GameplayModifiers Modifiers { get; set; } = new();
        public object StateLock { get; set; } = new();
        public PlayerStateHash State { get; set; } = new();
        public bool IsServerOwner => UserId == Instance._configuration.ServerOwnerId;
        public bool CanRecommendBeatmaps => true;
        public bool CanRecommendModifiers =>
            Instance._configuration.GameplayServerControlSettings is Enums.GameplayServerControlSettings.AllowModifierSelection or Enums.GameplayServerControlSettings.All;
        public bool CanKickVote => UserId == Instance._configuration.ServerOwnerId;
        public bool CanInvite =>
            Instance._configuration.DiscoveryPolicy is Enums.DiscoveryPolicy.WithCode or Enums.DiscoveryPolicy.Public;

        public bool IsPlayer => State.Contains("player");
        public bool IsSpectating => State.Contains("spectating");
        public bool WantsToPlayNextLevel => State.Contains("wants_to_play_next_level");
        public bool IsBackgrounded => State.Contains("backgrounded");
        public bool InGameplay => State.Contains("in_gameplay");
        public bool WasActiveAtLevelStart => State.Contains("was_active_at_level_start");
        public bool IsActive => State.Contains("is_active");
        public bool FinishedLevel => State.Contains("finished_level");
        public bool InMenu => State.Contains("in_menu");

        private const float _syncTimeOffset = 0.06f;
        private ConcurrentDictionary<string, EntitlementStatus> _entitlements = new();

        public Player(EndPoint endPoint, IDedicatedInstance instance,
            byte connectionId, string secret, string userId, string userName, string? playerSessionId, AccessLevel accessLevel = AccessLevel.Player)
        {
            Endpoint = endPoint;
            Instance = instance;
            ConnectionId = connectionId;
            Secret = secret;
            UserId = userId;
            UserName = userName;
            PlayerSessionId = playerSessionId;
            _AccessLevel = accessLevel;
        }
        public bool IsPatreon { get; set; } = false;
        public object MPChatLock { get; set; } = new();
        public bool CanTextChat { get; set; } = false;
        public bool CanReceiveVoiceChat { get; set; } = false;
        public bool CanTransmitVoiceChat { get; set; } = false;

        private AccessLevel _AccessLevel;
        public AccessLevel GetAccessLevel()
        {
            AccessLevel accessLevel = _AccessLevel;
            if (IsServerOwner)
                accessLevel = AccessLevel.Manager;
            if (IsPatreon)
                accessLevel++;
            return accessLevel;
        }

        public void SetAccessLevel(AccessLevel newLevel)
        {
            _AccessLevel = newLevel;
        }

        public object EntitlementLock { get; set; } = new();
        public EntitlementStatus GetEntitlement(string levelId)
            => _entitlements.TryGetValue(levelId, out var value) ? value : EntitlementStatus.Unknown;

        public void SetEntitlement(string levelId, EntitlementStatus entitlement)
            => _entitlements[levelId] = entitlement;

        public bool UpdateEntitlement { get; set; } = false;

        public string MapHash { get; set; } = string.Empty;
        public bool Chroma { get; set; } = false;
        public bool NoodleExtensions { get; set; } = false;
        public bool MappingExtensions { get; set; } = false;
        public BeatmapDifficulty[] BeatmapDifficulties { get; set; } = Array.Empty<BeatmapDifficulty>();

        public void ResetRecommendedMapRequirements()
        {
            MapHash= string.Empty;
            Chroma  = false;
            NoodleExtensions  = false;
            MappingExtensions = false;
            BeatmapDifficulties = Array.Empty<BeatmapDifficulty>();
        }
    }
}
