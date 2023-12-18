using BeatTogether.DedicatedServer.Kernel.Enums;
using System.Net;
using BeatTogether.DedicatedServer.Kernel.Types;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using System.Threading;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IPlayer
    {
        SemaphoreSlim PlayerAccessSemaphore { get; set; }
        EndPoint Endpoint { get; }
        IDedicatedInstance Instance { get; }
        byte ConnectionId { get; }
        byte RemoteConnectionId { get; }
        string Secret { get; }
        string UserId { get; }
        string UserName { get; }
        string? PlayerSessionId { get; }
        
        byte[]? Random { get; set; }
        byte[]? PublicEncryptionKey { get; set; }
        string ClientVersion { get; set; }
        Platform Platform { get; set; }
        string PlatformUserId { get; set; }
        
        uint? ENetPeerId { get; set; }
        bool IsENetConnection => ENetPeerId.HasValue;

        RollingAverage Latency { get; }
        long SyncTime { get; }
        int SortIndex { get; set; }
        MultiplayerAvatarsData Avatar { get; set; }
        bool IsReady { get; set; }

        BeatmapIdentifier? BeatmapIdentifier { get; set; }
        GameplayModifiers Modifiers { get; set; }
        PlayerStateHash State { get; set; }
        public bool ForceLateJoin { get; set; }
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
        bool InLobby { get; set; }
        bool IsPatreon { get; set; }
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
        long TicksAtLastSyncStateDelta { get; set; }
        long TicksAtLastSyncState { get; set; }
    }
}
