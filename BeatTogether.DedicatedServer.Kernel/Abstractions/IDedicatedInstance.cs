using System;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;

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


        BeatmapIdentifier? SelectedBeatmap { get; }
        GameplayModifiers SelectedModifiers { get; }
        CountdownState CountDownState { get; }
        float CountdownEndTime { get; }

        Task Start(CancellationToken cancellationToken = default);
        Task Stop(CancellationToken cancellationToken = default);

        int GetNextSortIndex();
        void ReleaseSortIndex(int sortIndex);
        byte GetNextConnectionId();
        void ReleaseConnectionId(byte connectionId);
        void SetState(MultiplayerGameState state);

        void SetCountdown(CountdownState countdownState, float countdown = 0);
        void CancelCountdown();

        void UpdateBeatmap(BeatmapIdentifier? beatmap, GameplayModifiers modifiers);
        
    }
}
