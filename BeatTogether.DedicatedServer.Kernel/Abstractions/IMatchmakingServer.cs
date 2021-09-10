using System.Threading;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Models;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using LiteNetLib;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IMatchmakingServer
    {
        string Secret { get; }
        string ManagerId { get; }
        GameplayServerConfiguration Configuration { get; }
        bool IsRunning { get; }
        float RunTime { get; }
        int Port { get; }
        MultiplayerGameState State { get; set; }

        Task Start(CancellationToken cancellationToken = default);
        Task Stop(CancellationToken cancellationToken = default);

        int GetNextSortIndex();
        void ReleaseSortIndex(int sortIndex);
        byte GetNextConnectionId();
        void ReleaseConnectionId(byte connectionId);
        void SendToAll(NetDataWriter writer, DeliveryMethod deliveryMethod);
    }
}
