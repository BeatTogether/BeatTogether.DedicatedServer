using BeatTogether.DedicatedServer.Kernel.Types;
using BeatTogether.DedicatedServer.Messaging.Models;
using LiteNetLib;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IPlayer
    {
        NetPeer NetPeer { get; }
        IMatchmakingServer MatchmakingServer { get; }
        byte ConnectionId { get; }
        string Secret { get; }
        string UserId { get; }
        string UserName { get; }
        RollingAverage Latency { get; }
        float SyncTime { get; }
        int SortIndex { get; set; }
        AvatarData? AvatarData { get; set; }
        bool IsReady { get; set; }
        BeatmapIdentifierNetSerializable? BeatmapIdentifier { get; set; }
        GameplayModifiers? Modifiers { get; set; }
        PlayerStateBloomFilter State { get; set; }
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
    }
}
