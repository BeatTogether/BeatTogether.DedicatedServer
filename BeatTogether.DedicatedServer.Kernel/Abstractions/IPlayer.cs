using BeatTogether.DedicatedServer.Kernel.Types;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using System.Net;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IPlayer
    {
        EndPoint Endpoint { get; }
        IDedicatedServer Server { get; }
        byte ConnectionId { get; }
        byte RemoteConnectionId { get; }
        string Secret { get; }
        string UserId { get; }
        string UserName { get; }

        RollingAverage Latency { get; }
        float SyncTime { get; }
        int SortIndex { get; set; }
        AvatarData Avatar { get; set; }
        bool IsReady { get; set; }

        BeatmapIdentifier? BeatmapIdentifier { get; set; }
        GameplayModifiers Modifiers { get; set; }
        PlayerStateHash State { get; set; }

        public bool IsManager { get; }
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
        bool InLobby { get; set; }

        EntitlementStatus GetEntitlement(string levelId);
        void SetEntitlement(string levelId, EntitlementStatus entitlement);
    }
}
