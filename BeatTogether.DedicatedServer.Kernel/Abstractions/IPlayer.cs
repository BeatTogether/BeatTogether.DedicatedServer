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
    }
}
