using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.Core.Enums;
using BeatTogether.DedicatedServer.Kernel.Configuration;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IDedicatedInstance
    {
        //event Action<IDedicatedInstance> StartEvent;
        event Action<IDedicatedInstance> StopEvent;
        event Action<IPlayer> PlayerConnectedEvent;
        event Action<IPlayer> PlayerDisconnectedEvent;
        event Action<string, EndPoint, string[]> PlayerDisconnectBeforeJoining;
        event Action<string, bool> GameIsInLobby;
        event Action<IDedicatedInstance> UpdateInstanceEvent;

        void InstanceConfigUpdated();
        InstanceConfiguration _configuration { get; }
        bool IsRunning { get; }
        long RunTime { get; }
        public int Port { get; }
        MultiplayerGameState State { get; }

        long NoPlayersTime { get; }

        IPlayerRegistry GetPlayerRegistry();
        IServiceProvider GetServiceProvider();

        Task Start(CancellationToken cancellationToken = default);
        Task Stop(CancellationToken cancellationToken = default);


        void DisconnectPlayer(IPlayer player);
        int GetNextSortIndex();
        void ReleaseSortIndex(int sortIndex);
        byte GetNextConnectionId();
        void ReleaseConnectionId(byte connectionId);
        void SetState(MultiplayerGameState state);
    }
}
