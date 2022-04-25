using System;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Enums;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IDedicatedInstance
    {
        event Action StartEvent;
        event Action StopEvent;
        event Action<IPlayer> PlayerConnectedEvent;
        event Action<IPlayer> PlayerDisconnectedEvent;

        InstanceConfiguration Configuration { get; }
        bool IsRunning { get; }
        float RunTime { get; }
        int Port { get; }
		string UserId { get; }
		string UserName { get; }
        MultiplayerGameState State { get; }

		Task Start(CancellationToken cancellationToken = default);
        Task Stop(CancellationToken cancellationToken = default);

        int GetNextSortIndex();
        void ReleaseSortIndex(int sortIndex);
        byte GetNextConnectionId();
        void ReleaseConnectionId(byte connectionId);
        void SetState(MultiplayerGameState state);
    }
}
