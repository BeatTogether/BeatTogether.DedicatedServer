﻿using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Kernel.Types;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using System.Net;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IPlayer
    {
        EndPoint Endpoint { get; }
        IDedicatedInstance Instance { get; }
        byte ConnectionId { get; }
        byte RemoteConnectionId { get; }
        string Secret { get; }
        string UserId { get; }
        string UserName { get; }
        byte[]? Random { get; set; }
        byte[]? PublicEncryptionKey { get; set; }

        object LatencyLock { get; set; }
        RollingAverage Latency { get; }
        float SyncTime { get; }
        object SortLock { get; set; }
        int SortIndex { get; set; }
        object PlayerIdentityLock { get; set; }
        AvatarData Avatar { get; set; }
        object ReadyLock { get; set; }
        bool IsReady { get; set; }

        object BeatmapLock { get; set; }
        BeatmapIdentifier? BeatmapIdentifier { get; set; }
        object ModifiersLock { get; set; }
        GameplayModifiers Modifiers { get; set; }
        object StateLock { get; set; }
        PlayerStateHash State { get; set; }

        public bool IsServerOwner { get; }
        public bool CanRecommendBeatmaps { get; }
        public bool CanRecommendModifiers { get; }
        public bool CanKickVote { get; }
        public bool CanInvite { get; }

        bool IsPlayer { get; }
        bool IsSpectating { get; }
        bool WantsToPlayNextLevel { get; }
        bool IsBackgrounded { get; }
        bool InGameplay { get; }
        bool WasActiveAtLevelStart { get; }
        bool IsActive { get; }
        bool FinishedLevel { get; }
        bool InMenu { get; }
        bool IsModded { get; }
        object InLobbyLock { get; set; }
        bool InLobby { get; set; }
        object EntitlementLock { get; set; }
        bool IsPatreon { get; set; }
        object MPChatLock { get; set; }
        bool CanTextChat { get; set; }
        public bool CanReceiveVoiceChat { get; set; }
        public bool CanTransmitVoiceChat { get; set; }
        public AccessLevel GetAccessLevel();
        public void SetAccessLevel(AccessLevel newAccessLevel);
        EntitlementStatus GetEntitlement(string levelId);
        void SetEntitlement(string levelId, EntitlementStatus entitlement);
        bool UpdateEntitlement { get; set; }
        public string MapHash { get; set; }
        public bool Chroma { get; set; }
        public bool NoodleExtensions { get; set; }
        public bool MappingExtensions { get; set; }
        public BeatmapDifficulty[] BeatmapDifficulties { get; set; }
        void ResetRecommendedMapRequirements();
    }
}
