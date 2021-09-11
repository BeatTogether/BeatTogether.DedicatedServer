using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Types;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using LiteNetLib;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class Player : IPlayer
    {
        public NetPeer NetPeer { get; }
        public IMatchmakingServer MatchmakingServer { get; }
        public byte ConnectionId { get; }
        public byte RemoteConnectionId { get; set; }
        public string Secret { get; }
        public string UserId { get; }
        public string UserName { get; }
        public RollingAverage Latency { get; } = new(30);
        public float SyncTime =>
            Math.Min(MatchmakingServer.RunTime - Latency.CurrentAverage - _syncTimeOffset,
                     MatchmakingServer.RunTime);
        public int SortIndex { get; set; }

        public AvatarData Avatar { get; set; } = new();
        public bool IsReady { get; set; }
        public bool InLobby { get; set; }
        public BeatmapIdentifier? BeatmapIdentifier { get; set; }
        public GameplayModifiers Modifiers { get; set; } = new();

        public PlayerStateHash State { get; set; } = new();

        public bool IsPlayer => State.Contains("player");
        public bool IsSpectating => State.Contains("spectating");
        public bool WantsToPlayNextLevel => State.Contains("wants_to_play_next_level");
        public bool IsBackgrounded => State.Contains("backgrounded");
        public bool InGameplay => State.Contains("in_gameplay");
        public bool WasActiveAtLevelStart => State.Contains("was_active_at_level_start");
        public bool IsActive => State.Contains("is_active");
        public bool FinishedLevel => State.Contains("finished_level");
        public bool InMenu => State.Contains("in_menu");
        public bool IsModded => State.Contains("modded");

        private const float _syncTimeOffset = 0.06f;

        public Player(NetPeer netPeer, IMatchmakingServer matchmakingServer,
            byte connectionId, string secret, string userId, string userName)
        {
            NetPeer = netPeer;
            MatchmakingServer = matchmakingServer;
            ConnectionId = connectionId;
            Secret = secret;
            UserId = userId;
            UserName = userName;
        }

        private ConcurrentBag<TaskCompletionSource<SetGameplaySceneReadyPacket>> _gameplaySceneReadyTcs = new();
        private ConcurrentBag<TaskCompletionSource> _gameplaySongReadyTcs = new();
        private ConcurrentBag<TaskCompletionSource<LevelFinishedPacket>> _gameplayLevelFinishedTcs = new();

        public Task<SetGameplaySceneReadyPacket> WaitForSceneReady(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<SetGameplaySceneReadyPacket>();
            cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            _gameplaySceneReadyTcs.Add(tcs);
            return tcs.Task;
        }

        public void SignalSceneReady(SetGameplaySceneReadyPacket packet)
        {
            foreach (var tcs in _gameplaySceneReadyTcs)
            {
                tcs.TrySetResult(packet);
            }
            _gameplaySceneReadyTcs.Clear();
        }

        public Task WaitForSongReady(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource();
            cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            _gameplaySongReadyTcs.Add(tcs);
            return tcs.Task;
        }

        public void SignalSongReady()
        {
            foreach (var tcs in _gameplaySongReadyTcs)
            {
                tcs.TrySetResult();
            }
            _gameplaySongReadyTcs.Clear();
        }

        public Task<LevelFinishedPacket> WaitForLevelFinished(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<LevelFinishedPacket>();
            cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            _gameplayLevelFinishedTcs.Add(tcs);
            return tcs.Task;
        }

        public void SignalLevelFinished(LevelFinishedPacket packet)
        {
            foreach (var tcs in _gameplayLevelFinishedTcs)
            {
                tcs.TrySetResult(packet);
            }
            _gameplayLevelFinishedTcs.Clear();
        }

        public void Dispose()
        {
            foreach (var tcs in _gameplaySceneReadyTcs)
            {
                tcs.TrySetCanceled();
            }

            foreach (var tcs in _gameplaySongReadyTcs)
            {
                tcs.TrySetCanceled();
            }

            foreach (var tcs in _gameplayLevelFinishedTcs)
            {
                tcs.TrySetCanceled();
            }
        }
    }
}
