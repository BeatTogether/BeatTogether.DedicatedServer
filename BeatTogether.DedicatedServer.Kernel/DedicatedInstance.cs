using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using BeatTogether.LiteNetLib;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Configuration;
using BeatTogether.LiteNetLib.Enums;
using Krypton.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;


namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class DedicatedInstance : LiteNetServer, IDedicatedInstance
    {
        // Milliseconds instance will wait for a player to connect.
        public const int WaitForPlayerTimeLimit = 10000;

        // Milliseconds between sync time updates
        public const int SyncTimeDelay = 5000;

        public InstanceConfiguration Configuration { get; private set; }
        public bool IsRunning => IsStarted;
        public float RunTime => (DateTime.UtcNow.Ticks - _startTime) / 10000000.0f;
        //public int Port => Endpoint.Port;
        public MultiplayerGameState State { get; private set; } = MultiplayerGameState.Lobby;

        public float NoPlayersTime { get; private set; } = -1; //tracks the instance time once there are 0 players in the lobby

        public event Action StartEvent = null!;
        public event Action StopEvent = null!;
        public event Action<IPlayer> PlayerConnectedEvent = null!;
        public event Action<IPlayer> PlayerDisconnectedEvent = null!;

        private readonly IPlayerRegistry _playerRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentQueue<byte> _releasedConnectionIds = new();
        private readonly ConcurrentQueue<int> _releasedSortIndices = new();
        private readonly ILogger _logger = Log.ForContext<DedicatedInstance>();

        private long _startTime;
        private int _connectionIdCount = 0;
        private int _lastSortIndex = -1;
        private CancellationTokenSource? _waitForPlayerCts = null;
        private CancellationTokenSource? _stopServerCts;
        private IPacketDispatcher _packetDispatcher = null!;

        public DedicatedInstance(
            InstanceConfiguration configuration,
            IPlayerRegistry playerRegistry,
            LiteNetConfiguration liteNetConfiguration,
            LiteNetPacketRegistry registry,
            IServiceProvider serviceProvider,
            IPacketLayer packetLayer)
            : base (
                  new IPEndPoint(IPAddress.Any, configuration.Port),
                  liteNetConfiguration,
                  registry,
                  serviceProvider,
                  packetLayer)
        {
            Configuration = configuration;
            _playerRegistry = playerRegistry;
            _serviceProvider = serviceProvider;

        }

        #region Public Methods

        public IPlayerRegistry GetPlayerRegistry()
        {
            return _playerRegistry;
        }
        public IServiceProvider GetServiceProvider()
        {
            return _serviceProvider;
        }

        public Task Start(CancellationToken cancellationToken = default)
        {
            if (IsRunning)
                return Task.CompletedTask;

            _packetDispatcher = _serviceProvider.GetRequiredService<IPacketDispatcher>();
            _startTime = DateTime.UtcNow.Ticks;

            _logger.Information(
                "Starting dedicated server " +
                $"(Port={Port}," +
                $"Secret='{Configuration.Secret}', " +
                $"ManagerId='{Configuration.ManagerId}', " +
                $"MaxPlayerCount={Configuration.MaxPlayerCount}, " +
                $"DiscoveryPolicy={Configuration.DiscoveryPolicy}, " +
                $"InvitePolicy={Configuration.InvitePolicy}, " +
                $"GameplayServerMode={Configuration.GameplayServerMode}, " +
                $"SongSelectionMode={Configuration.SongSelectionMode}, " +
                $"GameplayServerControlSettings={Configuration.GameplayServerControlSettings})."
            );
            _stopServerCts = new CancellationTokenSource();

            Task.Run(() => SendSyncTime(_stopServerCts.Token), cancellationToken);

            if (Configuration.DestroyInstanceTimeout != -1)
            {
                _waitForPlayerCts = new CancellationTokenSource();
                Task.Delay((WaitForPlayerTimeLimit + (int)(Configuration.DestroyInstanceTimeout * 1000)), _waitForPlayerCts.Token).ContinueWith(t =>
                {
                    if (!t.IsCanceled)
                    {
                        _logger.Warning("Timed out waiting for player to join, Server will close now");
                        _ = Stop(CancellationToken.None);
                    }
                    else
                    {
                        _waitForPlayerCts = null;
                    }
                }, cancellationToken);
            }

            StartEvent?.Invoke();

            base.Start();
            return Task.CompletedTask;
        }

        public Task Stop(CancellationToken cancellationToken = default)
        {
            if (!IsRunning)
                return Task.CompletedTask;

            _logger.Information(
                "Stopping dedicated server " +
                $"(Port={Port}," +
                $"Secret='{Configuration.Secret}', " +
                $"ManagerId='{Configuration.ManagerId}', " +
                $"MaxPlayerCount={Configuration.MaxPlayerCount}, " +
                $"DiscoveryPolicy={Configuration.DiscoveryPolicy}, " +
                $"InvitePolicy={Configuration.InvitePolicy}, " +
                $"GameplayServerMode={Configuration.GameplayServerMode}, " +
                $"SongSelectionMode={Configuration.SongSelectionMode}, " +
                $"GameplayServerControlSettings={Configuration.GameplayServerControlSettings})."
            );

            _stopServerCts!.Cancel();
            StopEvent?.Invoke();

            base.Stop();
            return Task.CompletedTask;
        }


        public int GetNextSortIndex()
        {
            if (_releasedSortIndices.TryDequeue(out var sortIndex))
                return sortIndex;
            var SortIndex = Interlocked.Increment(ref _lastSortIndex);
            if (SortIndex == 127)
                SortIndex = Interlocked.Increment(ref _lastSortIndex);
            return SortIndex;
        }

        public void ReleaseSortIndex(int sortIndex) =>
            _releasedSortIndices.Enqueue(sortIndex);

        public byte GetNextConnectionId() //ID 0 is server, ID 127 means send to all players
        {
            if (_releasedConnectionIds.TryDequeue(out var connectionId))
                return (byte)(connectionId % 256);
            var connectionIdCount = Interlocked.Increment(ref _connectionIdCount);
            if (connectionIdCount == 127)
                connectionIdCount = Interlocked.Increment(ref _connectionIdCount);
            if (connectionIdCount > byte.MaxValue)
                return 0;
            return (byte)connectionIdCount;
        }

        public void ReleaseConnectionId(byte connectionId) =>
            _releasedConnectionIds.Enqueue(connectionId);

        public void SetState(MultiplayerGameState state)
        {
            State = state;
            _packetDispatcher.SendToNearbyPlayers(new SetMultiplayerGameStatePacket
            {
                State = state
            }, DeliveryMethod.ReliableOrdered);
        }

        #endregion

        #region LiteNetServer

        public override bool ShouldAcceptConnection(EndPoint endPoint, ref SpanBufferReader additionalData)
        {
            var connectionRequestData = new ConnectionRequestData();
            try
            {
                connectionRequestData.ReadFrom(ref additionalData);
            }
            catch (Exception e)
            {
                _logger.Warning(e,
                    "Failed to deserialize connection request data " +
                    $"(RemoteEndPoint='{endPoint}')."
                );
                Console.WriteLine("Failed 1");
                return false;
            }

            _logger.Debug(
                "Handling connection request " +
                $"(RemoteEndPoint='{endPoint}', " +
                $"Secret='{connectionRequestData.Secret}', " +
                $"UserId='{connectionRequestData.UserId}', " +
                $"UserName='{connectionRequestData.UserName}', " +
                $"IsConnectionOwner={connectionRequestData.IsConnectionOwner})."
            );

            if (string.IsNullOrEmpty(connectionRequestData.UserId) ||
                string.IsNullOrEmpty(connectionRequestData.UserName) ||
                string.IsNullOrEmpty(connectionRequestData.Secret))
            {
                _logger.Warning(
                    "Received a connection request with invalid data " +
                    $"(RemoteEndPoint='{endPoint}', " +
                    $"Secret='{connectionRequestData.Secret}', " +
                    $"UserId='{connectionRequestData.UserId}', " +
                    $"UserName='{connectionRequestData.UserName}', " +
                    $"IsConnectionOwner={connectionRequestData.IsConnectionOwner})."
                );
                Console.WriteLine("Failed 2");
                return false;
            }

            if (_playerRegistry.Players.Count >= Configuration.MaxPlayerCount)
                return false;

            int sortIndex = GetNextSortIndex();
            byte connectionId = GetNextConnectionId();
            
            var player = new Player(
                endPoint,
                this,
                connectionId,
                connectionRequestData.Secret,
                connectionRequestData.UserId,
                connectionRequestData.UserName
            )
            {
                SortIndex = sortIndex
            };

            _playerRegistry.AddPlayer(player);
            _logger.Information(
                "Player joined dedicated server " +
                $"(RemoteEndPoint='{player.Endpoint}', " +
                $"ConnectionId={player.ConnectionId}, " +
                $"Secret='{player.Secret}', " +
                $"UserId='{player.UserId}', " +
                $"UserName='{player.UserName}', " +
                $"SortIndex={player.SortIndex})."
            );

            if (_waitForPlayerCts != null)
                _waitForPlayerCts.Cancel();
            return true;
        }

        public override void OnLatencyUpdate(EndPoint endPoint, int latency)
            => _logger.Verbose($"Latency updated (RemoteEndPoint='{endPoint}', Latency={0.001f * latency}).");

        public override void OnConnect(EndPoint endPoint)
        {
            _logger.Information($"Endpoint connected (RemoteEndPoint='{endPoint}'), connecting player");

            if (!_playerRegistry.TryGetPlayer(endPoint, out var player))
            {
                _logger.Warning(
                    "Failed to retrieve player " +
                    $"(RemoteEndPoint='{endPoint}')."
                );
                Disconnect(endPoint);
                return;
            }

            // Update SyncTime
            _packetDispatcher.SendToNearbyPlayers(new SyncTimePacket
            {
                SyncTime = RunTime
            }, DeliveryMethod.ReliableOrdered);

            // Send new player's connection data
            _packetDispatcher.SendExcludingPlayer(player, new PlayerConnectedPacket
            {
                RemoteConnectionId = player.ConnectionId,
                UserId = player.UserId,
                UserName = player.UserName,
                IsConnectionOwner = false
            }, DeliveryMethod.ReliableOrdered);

            // Send new player's sort order
            _packetDispatcher.SendToNearbyPlayers(new PlayerSortOrderPacket
            {
                UserId = player.UserId,
                SortIndex = player.SortIndex
            }, DeliveryMethod.ReliableOrdered);

            // Send host player to new player
            _packetDispatcher.SendToPlayer(player, new PlayerConnectedPacket
            {
                RemoteConnectionId = 0,
                UserId = Configuration.ServerId,
                UserName = Configuration.ServerName,
                IsConnectionOwner = true
            }, DeliveryMethod.ReliableOrdered);
            /*
            //Not needed
            // Send host player sort order to new player
            _packetDispatcher.SendToPlayer(player, new PlayerSortOrderPacket
            {
                UserId = Configuration.Secret,
                SortIndex = 0
            }, DeliveryMethod.ReliableOrdered);
            */

            foreach (IPlayer p in _playerRegistry.Players.Where(p => p.ConnectionId != player.ConnectionId))
            {

                // Send all player connection data packets to new player
                _packetDispatcher.SendToPlayer(player, new PlayerConnectedPacket
                {
                    RemoteConnectionId = p.ConnectionId,
                    UserId = p.UserId,
                    UserName = p.UserName,
                    IsConnectionOwner = false
                }, DeliveryMethod.ReliableOrdered);

                // Send all player sort index packets to new player
                //if (p.SortIndex != -1) there are no players with -1 at all
                _packetDispatcher.SendToPlayer(player, new PlayerSortOrderPacket
                {
                    UserId = p.UserId,
                    SortIndex = p.SortIndex
                }, DeliveryMethod.ReliableOrdered);

                // Send all player identity packets to new player
                _packetDispatcher.SendFromPlayerToPlayer(p, player, new PlayerIdentityPacket
                    {
                        PlayerState = p.State,
                        PlayerAvatar = p.Avatar,
                        Random = new ByteArray { Data = p.Random },
                        PublicEncryptionKey = new ByteArray { Data = p.PublicEncryptionKey }
                    }, DeliveryMethod.ReliableOrdered);
            }

            // Disable start button if they are manager without selected song
            _packetDispatcher.SendToPlayer(player, new SetIsStartButtonEnabledPacket
                {
                    Reason = player.UserId == Configuration.ManagerId ? CannotStartGameReason.NoSongSelected : CannotStartGameReason.None
                }, DeliveryMethod.ReliableOrdered);

            // Update permissions
            if ((Configuration.SetManagerFromUserId == player.UserId || _playerRegistry.Players.Count == 1) && Configuration.GameplayServerMode == Enums.GameplayServerMode.Managed)
                Configuration.ManagerId = player.UserId;

            _packetDispatcher.SendToNearbyPlayers(new SetPlayersPermissionConfigurationPacket
            {
                PermissionConfiguration = new PlayersPermissionConfiguration
                {
                    PlayersPermission = _playerRegistry.Players.Select(x => new PlayerPermissionConfiguration
                    {
                        UserId = x.UserId,
                        IsServerOwner = x.IsManager,
                        HasRecommendBeatmapsPermission = x.CanRecommendBeatmaps,
                        HasRecommendGameplayModifiersPermission = x.CanRecommendModifiers,
                        HasKickVotePermission = x.CanKickVote,
                        HasInvitePermission = x.CanInvite
                    }).ToList()
                }
            }, DeliveryMethod.ReliableOrdered);
            PlayerConnectedEvent?.Invoke(player);
        }

        public override void OnDisconnect(EndPoint endPoint, DisconnectReason reason)
        {
            _logger.Information(
                "Endpoint disconnected " +
                $"(RemoteEndPoint='{endPoint}', DisconnectReason={reason})."
            );

            if (reason == DisconnectReason.Reconnect || reason == DisconnectReason.PeerToPeerConnection)
            {
                _logger.Information(
                    "Endpoint reconnecting or is peer to peer."
                );
                return;
            }

            // Disconnect player
            if (_playerRegistry.TryGetPlayer(endPoint, out var player))
            {
                _packetDispatcher.SendFromPlayer(player, new PlayerDisconnectedPacket
                {
                    DisconnectedReason = DisconnectedReason.ClientConnectionClosed
                }, DeliveryMethod.ReliableOrdered);

                if (Configuration.ManagerId == player.UserId)
                    Configuration.ManagerId = "";

                _playerRegistry.RemovePlayer(player);
                ReleaseSortIndex(player.SortIndex);
                ReleaseConnectionId(player.ConnectionId);

                PlayerDisconnectedEvent?.Invoke(player);
            }

            if (_playerRegistry.Players.Count == 0)
            {
                NoPlayersTime = RunTime;
                if (Configuration.DestroyInstanceTimeout != -1)
                {
                    _waitForPlayerCts = new CancellationTokenSource();
                    _ = Task.Delay((int)(Configuration.DestroyInstanceTimeout * 1000), _waitForPlayerCts.Token).ContinueWith(t =>
                    {
                        if (!t.IsCanceled)
                        {
                            _logger.Information("No players joined within the closing timeout, stopping lobby now");
                            _ = Stop(CancellationToken.None);
                        }
                        else
                        {
                            _waitForPlayerCts = null;
                        }
                    });
                }
            }
            else
            {
                // Set new manager if manager left
                if (Configuration.ManagerId == "" && Configuration.GameplayServerMode == Enums.GameplayServerMode.Managed)
                {
                    Configuration.ManagerId = _playerRegistry.Players.Last().UserId;
                    var manager = _playerRegistry.GetPlayer(Configuration.ManagerId);

                    // Disable start button if they are manager without selected song
                    if (manager.BeatmapIdentifier == null)
                        _packetDispatcher.SendToPlayer(manager, new SetIsStartButtonEnabledPacket
                        {
                            Reason = CannotStartGameReason.NoSongSelected
                        }, DeliveryMethod.ReliableOrdered);

                    // Update permissions
                    _packetDispatcher.SendToNearbyPlayers(new SetPlayersPermissionConfigurationPacket
                    {
                        PermissionConfiguration = new PlayersPermissionConfiguration
                        {
                            PlayersPermission = _playerRegistry.Players.Select(x => new PlayerPermissionConfiguration
                            {
                                UserId = x.UserId,
                                IsServerOwner = x.IsManager,
                                HasRecommendBeatmapsPermission = x.CanRecommendBeatmaps,
                                HasRecommendGameplayModifiersPermission = x.CanRecommendModifiers,
                                HasKickVotePermission = x.CanKickVote,
                                HasInvitePermission = x.CanInvite
                            }).ToList()
                        }
                    }, DeliveryMethod.ReliableOrdered);
                }
            }
        }
        #endregion

        #region Private Methods

        private async void SendSyncTime(CancellationToken cancellationToken)
        {
            foreach (IPlayer player in _playerRegistry.Players)
                _packetDispatcher.SendToPlayer(player, new SyncTimePacket
                {
                    SyncTime = player.SyncTime
                }, DeliveryMethod.ReliableOrdered);
            try
            {
                await Task.Delay(SyncTimeDelay, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                return;
            }
            SendSyncTime(cancellationToken);
        }

        #endregion
    }
}
