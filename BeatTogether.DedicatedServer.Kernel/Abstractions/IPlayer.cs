using BeatTogether.DedicatedServer.Kernel.Types;
using BeatTogether.DedicatedServer.Messaging.Models;
using LiteNetLib;
using System;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IPlayer : IGameplaySceneSignalSource, IGameplaySongSignalSource, IGameplayFinishedSignalSource, IDisposable
    {
        NetPeer NetPeer { get; }
        IMatchmakingServer MatchmakingServer { get; }
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
    }
}
