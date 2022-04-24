using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
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
using WinFormsLibrary;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class DedicatedInstance : LiteNetServer, IDedicatedInstance
    {
        // Milliseconds instance will wait for a player to connect.
        public const int WaitForPlayerTimeLimit = 10000;

        // Milliseconds between sync time updates
        public const int SyncTimeDelay = 5000;

        public string UserId => "ziuMSceapEuNN7wRGQXrZg";
        public string UserName => "";
        public InstanceConfiguration Configuration { get; private set; }
        public bool IsRunning => IsStarted;
        public float RunTime => (DateTime.UtcNow.Ticks - _startTime) / 10000000.0f;
        public int Port => Endpoint.Port;
        public MultiplayerGameState State { get; private set; } = MultiplayerGameState.Lobby;

        public event Action StartEvent = null!;
        public event Action StopEvent = null!;
        public event Action<IPlayer> PlayerConnectedEvent = null!;
        public event Action<IPlayer> PlayerDisconnectedEvent = null!;

        private readonly IPlayerRegistry _playerRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILobbyManager _lobbyManager;
        private readonly ConcurrentQueue<byte> _releasedConnectionIds = new();
        private readonly ConcurrentQueue<int> _releasedSortIndices = new();
        private readonly ILogger _logger = Log.ForContext<DedicatedInstance>();

        private long _startTime;
        private int _connectionIdCount = 0;
        private int _lastSortIndex = -1;
        private CancellationTokenSource? _waitForPlayerCts;
        private CancellationTokenSource? _stopServerCts;
        private IPacketDispatcher _packetDispatcher = null!;

        public DedicatedInstance(
            InstanceConfiguration configuration,
            IPlayerRegistry playerRegistry,
            LiteNetConfiguration liteNetConfiguration,
            LiteNetPacketRegistry registry,
            IServiceProvider serviceProvider,
            IPacketLayer packetLayer,
            ILobbyManager lobbyManager)
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
            _lobbyManager = lobbyManager;
        }

        #region Public Methods

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

            _waitForPlayerCts = new CancellationTokenSource();
            _stopServerCts = new CancellationTokenSource();
            SendSyncTime(_stopServerCts.Token);
            _ = Task.Delay(WaitForPlayerTimeLimit, _waitForPlayerCts.Token).ContinueWith(t =>
            {
                if (!t.IsCanceled && !Configuration.Secret.Contains("SpecialServer"))
                {
                    _logger.Warning("Timed out waiting for player to join, stopping server.");
                    _ = Stop(CancellationToken.None);
                }
                else
                {
                    _waitForPlayerCts = null;
                }
            });

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
            return Interlocked.Increment(ref _lastSortIndex);
        }

        public void ReleaseSortIndex(int sortIndex) =>
            _releasedSortIndices.Enqueue(sortIndex);

        public int GetConnectionIDcount()
        {
            return _connectionIdCount;
        }

        //TODO should probably code a hard limit of 128 players somewhere (unless anyone would like to change connectionID to an int)
        public byte GetNextConnectionId()
        {
            if (_releasedConnectionIds.TryDequeue(out var connectionId))
                return (byte)(connectionId % 127);
            var connectionIdCount = Interlocked.Increment(ref _connectionIdCount);
            if (connectionIdCount >= 127)
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
        public PlayerRegistry GetPlayerRegistry()
        {
            return (PlayerRegistry)_playerRegistry;
        }

        public void KickPlayer(string UserId)
        {
            if (_playerRegistry.TryGetPlayer(UserId, out var player))
            {
                _packetDispatcher.SendFromPlayer(player!, new PlayerDisconnectedPacket
                {
                    DisconnectedReason = DisconnectedReason.Kicked
                }, DeliveryMethod.ReliableOrdered);

                if (Configuration.ManagerId == player.UserId)
                    Configuration.ManagerId = "";

                _playerRegistry.RemovePlayer(player);
                ReleaseSortIndex(player.SortIndex);
                ReleaseConnectionId(player.ConnectionId);

                PlayerDisconnectedEvent?.Invoke(player);
                MessageForm.Updt();
                OnDisconnect(player.Endpoint, DisconnectReason.ConnectionRejected);
            }
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
                return false;
            }

            if (_playerRegistry.Players.Count >= Configuration.MaxPlayerCount)
                return false;

            var connectionId = GetNextConnectionId();
            var sortIndex = GetNextSortIndex();
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
            _logger.Debug($"Endpoint connected (RemoteEndPoint='{endPoint}').");

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

            // Send host player to new player                                    //TODO test without this
            _packetDispatcher.SendToPlayer(player, new PlayerConnectedPacket
            {
                RemoteConnectionId = 0,
                UserId = UserId,
                UserName = UserName,
                IsConnectionOwner = true
            }, DeliveryMethod.ReliableOrdered);

            // Send host player sort order to new player                        //TODO test without this
            _packetDispatcher.SendToPlayer(player, new PlayerSortOrderPacket
            {
                UserId = Configuration.Secret,
                SortIndex = 0
            }, DeliveryMethod.ReliableOrdered);



            foreach (IPlayer p in _playerRegistry.Players)
            {
                if (p.Endpoint != player.Endpoint)
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
                    if (p.SortIndex != -1)
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
            }

            // Disable start button if they are manager without selected song
            _packetDispatcher.SendToPlayer(player, new SetIsStartButtonEnabledPacket
            {
                Reason = player.UserId == Configuration.ManagerId ? CannotStartGameReason.NoSongSelected : CannotStartGameReason.None
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

            //handles sending players that join the countdown time, if the lobby is waiting for everyone to download the map then they get sent the map to download
            if (_lobbyManager.CountdownEndTime < 0)
            {
                _packetDispatcher.SendToNearbyPlayers(new StartLevelPacket
                {
                    Beatmap = _lobbyManager.SelectedBeatmap!,
                    Modifiers = _lobbyManager.SelectedModifiers,
                    StartTime = _lobbyManager.CountdownEndTime
                }, DeliveryMethod.ReliableOrdered);
            }
            else if (_lobbyManager.CountdownEndTime != 0)
            {
                _packetDispatcher.SendToNearbyPlayers(new SetCountdownEndTimePacket
                {
                    CountdownTime = _lobbyManager.CountdownEndTime
                }, DeliveryMethod.ReliableOrdered);
            }
            PlayerConnectedEvent?.Invoke(player);
            MessageForm.Updt();
        }

        public void StopDedicatedInstance()
        {
            _ = Stop(CancellationToken.None);
        }



        public override void OnDisconnect(EndPoint endPoint, DisconnectReason reason)
        {
            if (reason == DisconnectReason.Reconnect || reason == DisconnectReason.PeerToPeerConnection)
                return;

            _logger.Debug(
                "Endpoint disconnected " +
                $"(RemoteEndPoint='{endPoint}', DisconnectReason={reason})."
            );

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
                MessageForm.Updt();
            }

            if (_playerRegistry.Players.Count == 0 && !Configuration.Secret.Contains("SpecialServer"))
                _ = Stop(CancellationToken.None);
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
