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
        event Action<IDedicatedInstance> StartEvent;
        event Action<IDedicatedInstance> StopEvent;
        event Action<IPlayer> PlayerConnectedEvent;
        event Action<IPlayer, int> PlayerDisconnectedEvent;
        event Action<string, int> PlayerCountChangeEvent;
        event Action<string, Enums.CountdownState, MultiplayerGameState, Enums.GameplayManagerState> StateChangedEvent;
        event Action<IDedicatedInstance> UpdateInstanceEvent;
        event Action<string, BeatmapIdentifier?, GameplayModifiers, bool, DateTime> UpdateBeatmapEvent;

        void PlayerUpdated(IPlayer player);
        void InstanceStateChanged(CountdownState countdown, GameplayManagerState gameplay);
        void BeatmapChanged(BeatmapIdentifier? map, GameplayModifiers modifiers, bool IsGameplay, DateTime CountdownEnd);
        void InstanceChanged();
        InstanceConfiguration _configuration { get; }
        bool IsRunning { get; }
        float RunTime { get; }
        int Port { get; }
        MultiplayerGameState State { get; }

        float NoPlayersTime { get; }

        IPlayerRegistry GetPlayerRegistry();
        IServiceProvider GetServiceProvider();

        Task Start(CancellationToken cancellationToken = default);
        Task Stop(CancellationToken cancellationToken = default);


        void DisconnectPlayer(string UserId);
        int GetNextSortIndex();
        void ReleaseSortIndex(int sortIndex);
        byte GetNextConnectionId();
        void ReleaseConnectionId(byte connectionId);
        void SetState(MultiplayerGameState state);
    }
}
