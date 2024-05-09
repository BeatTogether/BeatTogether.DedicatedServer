using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using BeatTogether.Core.Enums;
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
        public string HashedUserId { get; set; }
        public string UserName { get; }
        public string PlayerSessionId { get; set; }

        public uint ENetPeerId { get; set; }

        public RollingAverage Latency { get; } = new(30);
        public long SyncTime =>
            Math.Min(Instance.RunTime - Latency.CurrentAverage,
                     Instance.RunTime);
        public int SortIndex { get; set; }
        public byte[]? Random { get; set; }
        public byte[]? PublicEncryptionKey { get; set; }
        public Version PlayerClientVersion { get; set; } = new();
        public Platform PlayerPlatform { get; set; } = Platform.Test; //Unknown
        public string PlatformUserId { get; set; } = "";
        public MultiplayerAvatarsData Avatar { get; set; } = new();
        public bool IsReady { get; set; }
        public bool InLobby { get; set; }

        public BeatmapIdentifier? BeatmapIdentifier { get; set; } = null;
        public GameplayModifiers Modifiers { get; set; } = new();
        public PlayerStateHash State { get; set; } = new();
        public bool IsServerOwner => HashedUserId == Instance._configuration.ServerOwnerId;
        public bool CanRecommendBeatmaps => true;// This check is wrong as GameplayServerControlSettings is None in Quickplay to disable Modifier selection  //Instance._configuration.GameplayServerControlSettings is not GameplayServerControlSettings.None;
        public bool CanRecommendModifiers =>
            Instance._configuration.GameplayServerConfiguration.GameplayServerControlSettings is GameplayServerControlSettings.AllowModifierSelection or GameplayServerControlSettings.All;
        public bool CanKickVote => HashedUserId == Instance._configuration.ServerOwnerId;
        public bool CanInvite =>
            Instance._configuration.GameplayServerConfiguration.DiscoveryPolicy is DiscoveryPolicy.WithCode or DiscoveryPolicy.Public;
        public bool ForceLateJoin { get; set; } = false; //Used to force trouble players to late join a mp game/tell them to spectate

        public bool IsPlayer => State.Contains("player"); //If the user is a player
        public bool IsSpectating => State.Contains("spectating"); //True if player is spectating (special case that is not applicable to normal game clients)
        public bool WantsToPlayNextLevel => State.Contains("wants_to_play_next_level"); //True if player wants to spectate
        public bool IsBackgrounded => State.Contains("backgrounded"); //If user has gone AFK
        public bool InGameplay => State.Contains("in_gameplay"); //True while user in gameplay
        public bool WasActiveAtLevelStart => State.Contains("was_active_at_level_start"); //True if the player was active at the beatmap level start - playing the level
        public bool IsActive => State.Contains("is_active"); //If player is activly playing the beatmap
        public bool FinishedLevel => State.Contains("finished_level"); //If the player has finished the level
        public bool InMenu => State.Contains("in_menu"); //Should be true while in lobby

        private readonly ConcurrentDictionary<string, EntitlementStatus> _entitlements = new(); //Set a max amount of like 50 or something.

        public Player(EndPoint endPoint, IDedicatedInstance instance,
            byte connectionId, string userId, string userName, string playerSessionId, AccessLevel accessLevel = AccessLevel.Player)
        {
            Endpoint = endPoint;
            Instance = instance;
            ConnectionId = connectionId;
            HashedUserId = userId;
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

        public EntitlementStatus GetEntitlement(string levelId)
            => _entitlements.TryGetValue(levelId, out var value) ? value : EntitlementStatus.Unknown;

        public void SetEntitlement(string levelId, EntitlementStatus entitlement)
            => _entitlements[levelId] = entitlement;

        public bool UpdateEntitlement { get; set; } = false;

        public string MapHash { get; set; } = string.Empty;
        public Dictionary<uint, string[]> BeatmapDifficultiesRequirements { get; set; } = new();

        public long TicksAtLastSyncStateDelta { get; set; } = 0; //33ms gaps for 30/sec, 66ms gap for 15/sec
        public long TicksAtLastSyncState { get; set; } = 0;

    }
}
