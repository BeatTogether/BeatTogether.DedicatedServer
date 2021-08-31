using System.Threading;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Models;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IMatchmakingServer
    {
        string Secret { get; }
        string ManagerId { get; }
        GameplayServerConfiguration Configuration { get; }
        PlayersPermissionConfiguration Permissions { get; }
        bool IsRunning { get; }
        float RunTime { get; }
        int Port { get; }
        MultiplayerGameState State { get; }

        Task Start(CancellationToken cancellationToken = default);
        Task Stop(CancellationToken cancellationToken = default);

        int GetNextSortIndex();
        void ReleaseSortIndex(int sortIndex);
        byte GetNextConnectionId();
        void ReleaseConnectionId(byte connectionId);
    }
}
