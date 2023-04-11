using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Kernel.Encryption;
using BeatTogether.DedicatedServer.Kernel.Enums;
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

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class DedicatedInstance : LiteNetServer, IDedicatedInstance
    {
        // Milliseconds instance will wait for a player to connect.
        public const int WaitForPlayerTimeLimit = 10000;

        // Milliseconds between sync time updates
        public const int SyncTimeDelay = 5000;

        public InstanceConfiguration _configuration { get; private set; }
        public bool IsRunning => IsStarted;
        public float RunTime => (DateTime.UtcNow.Ticks - _startTime) / 10000000.0f;
        public float NoPlayersTime { get; private set; } = -1; //tracks the instance time once there are 0 players in the lobby
        public MultiplayerGameState State { get; private set; } = MultiplayerGameState.Lobby;

        public event Action<IDedicatedInstance> StopEvent = null!;
        public event Action<IPlayer, int> PlayerConnectedEvent = null!;
        public event Action<IPlayer, int> PlayerDisconnectedEvent = null!;
        public event Action<string, EndPoint, int> PlayerDisconnectBeforeJoining = null!;
        public event Action<string, bool> GameIsInLobby = null!;

        private readonly IHandshakeSessionRegistry _handshakeSessionRegistry;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly PacketEncryptionLayer _packetEncryptionLayer;
        
        private readonly ConcurrentQueue<byte> _releasedConnectionIds = new();
        private readonly ConcurrentQueue<int> _releasedSortIndices = new();
        private readonly ILogger _logger = Log.ForContext<DedicatedInstance>();

        private long _startTime;
        private byte _connectionIdCount = 0;
        private int _lastSortIndex = -1;
        private CancellationTokenSource? _waitForPlayerCts = null;
        private CancellationTokenSource? _stopServerCts;
        private IPacketDispatcher _packetDispatcher = null!;

        public DedicatedInstance(
            InstanceConfiguration configuration,
            IHandshakeSessionRegistry handshakeSessionRegistry,
            IPlayerRegistry playerRegistry,
            LiteNetConfiguration liteNetConfiguration,
            LiteNetPacketRegistry registry,
            IServiceProvider serviceProvider,
            IPacketLayer packetLayer,
            PacketEncryptionLayer packetEncryptionLayer)
            : base (
                  new IPEndPoint(IPAddress.Any, configuration.Port),
                  liteNetConfiguration,
                  registry,
                  serviceProvider,
                  packetLayer)
        {
            _configuration = configuration;

            _handshakeSessionRegistry = handshakeSessionRegistry;
            _playerRegistry = playerRegistry;
            _serviceProvider = serviceProvider;
            _packetEncryptionLayer = packetEncryptionLayer;
        }

        #region Public Methods
        
        public IHandshakeSessionRegistry GetHandshakeSessionRegistry()
        {
            return _handshakeSessionRegistry;
        }

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
                $"ServerName='{_configuration.ServerName}', " +
                $"Secret='{_configuration.Secret}', " +
                $"ManagerId='{_configuration.ServerOwnerId}', " +
                $"MaxPlayerCount={_configuration.MaxPlayerCount}, " +
                $"DiscoveryPolicy={_configuration.DiscoveryPolicy}, " +
                $"InvitePolicy={_configuration.InvitePolicy}, " +
                $"GameplayServerMode={_configuration.GameplayServerMode}, " +
                $"SongSelectionMode={_configuration.SongSelectionMode}, " +
                $"GameplayServerControlSettings={_configuration.GameplayServerControlSettings})."
            );
            _stopServerCts = new CancellationTokenSource();



            if (_configuration.DestroyInstanceTimeout != -1)
            {
                _waitForPlayerCts = new CancellationTokenSource();
                Task.Delay((WaitForPlayerTimeLimit + (int)(_configuration.DestroyInstanceTimeout * 1000)), _waitForPlayerCts.Token).ContinueWith(t =>
                {
                    if (!t.IsCanceled)
                    {
                        _logger.Warning("Stopping instance: " + _configuration.ServerName);
                        _ = Stop(CancellationToken.None);
                    }
                    else
                    {
                        _waitForPlayerCts = null;
                    }
                }, cancellationToken);
            }

            //StartEvent?.Invoke(this);

            base.Start();
            Task.Run(() => SendSyncTime(_stopServerCts.Token), cancellationToken);
            return Task.CompletedTask;
        }

        public Task Stop(CancellationToken cancellationToken = default)
        {
            if (!IsRunning)
                return Task.CompletedTask;

            _logger.Information(
                "Stopping dedicated server " +
                $"(Port={Port}," +
                $"ServerName='{_configuration.ServerName}', " +
                $"Secret='{_configuration.Secret}', " +
                $"ManagerId='{_configuration.ServerOwnerId}', " +
                $"MaxPlayerCount={_configuration.MaxPlayerCount}, " +
                $"DiscoveryPolicy={_configuration.DiscoveryPolicy}, " +
                $"InvitePolicy={_configuration.InvitePolicy}, " +
                $"GameplayServerMode={_configuration.GameplayServerMode}, " +
                $"SongSelectionMode={_configuration.SongSelectionMode}, " +
                $"GameplayServerControlSettings={_configuration.GameplayServerControlSettings})."
            );
            _packetDispatcher.SendToNearbyPlayers(new KickPlayerPacket
            {
                DisconnectedReason = DisconnectedReason.ServerTerminated
            }, DeliveryMethod.ReliableOrdered);

            _stopServerCts!.Cancel();
            StopEvent?.Invoke(this);

            base.Stop();
            return Task.CompletedTask;
        }

        object SortIndexLock = new();
        public int GetNextSortIndex()
        {
            if (_releasedSortIndices.TryDequeue(out var sortIndex))
                return sortIndex;
            lock (SortIndexLock)
            {
                _lastSortIndex++;
                if (_lastSortIndex > _configuration.MaxPlayerCount)
                {
                    return 0;
                }
                return _lastSortIndex;
            }
        }

        public void ReleaseSortIndex(int sortIndex) =>
            _releasedSortIndices.Enqueue(sortIndex);

        object ConnectionIDLock = new();
        public byte GetNextConnectionId() //ID 0 is server, ID 127 also means send to all players, 255 will mean perm spectator when/if implimented. Starts at 1 because funny server logic
        {
            if (_releasedConnectionIds.TryDequeue(out var connectionId))
                return connectionId;
            lock (ConnectionIDLock)
            {
                _connectionIdCount++;
                if (_connectionIdCount == 127)
                    _connectionIdCount++;
                if (_connectionIdCount > (byte.MaxValue - 5))
                    return 255; //Give them an unusedID so they dont conflict with anyone
                return _connectionIdCount;
            }
        }

        public void ReleaseConnectionId(byte connectionId) =>
            _releasedConnectionIds.Enqueue(connectionId);

        public void SetState(MultiplayerGameState state)
        {
            State = state;
            _packetDispatcher.SendToNearbyPlayers(new SetMultiplayerGameStatePacket()
            {
                State = state
            }, DeliveryMethod.ReliableOrdered);
            GameIsInLobby?.Invoke(_configuration.Secret, state == MultiplayerGameState.Lobby);
        }

        #endregion

        #region LiteNetServer

        object AcceptConnectionLock = new();

        public override bool ShouldAcceptConnection(EndPoint endPoint, ref SpanBufferReader additionalData)
        {

            if (ShouldDenyConnection(endPoint, ref additionalData))
            {
                PlayerDisconnectBeforeJoining?.Invoke(_configuration.Secret, endPoint, _playerRegistry.GetPlayerCount());
                return false;
            }
            return true;
        }
        public bool ShouldDenyConnection(EndPoint endPoint, ref SpanBufferReader additionalData)
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
                return true;
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
                string.IsNullOrEmpty(connectionRequestData.UserName))
                //string.IsNullOrEmpty(connectionRequestData.Secret))
            {
                _logger.Warning(
                    "Received a connection request with invalid data " +
                    $"(RemoteEndPoint='{endPoint}', " +
                    //$"Secret='{connectionRequestData.Secret}', " +
                    $"UserId='{connectionRequestData.UserId}', " +
                    $"UserName='{connectionRequestData.UserName}', " +
                    $"IsConnectionOwner={connectionRequestData.IsConnectionOwner})."
                );
                return true;
            }
            
            lock (AcceptConnectionLock)
            {
                if (_playerRegistry.GetPlayerCount() >= _configuration.MaxPlayerCount)
                {
                    return true;
                }
                int sortIndex = GetNextSortIndex();
                byte connectionId = GetNextConnectionId();

                var player = new Player(
                    endPoint,
                    this,
                    connectionId,
                    _configuration.Secret,
                    connectionRequestData.UserId,
                    connectionRequestData.UserName,
                    connectionRequestData.PlayerSessionId
                )
                {
                    SortIndex = sortIndex
                };

                if (!_playerRegistry.AddPlayer(player))
                {
                    ReleaseSortIndex(player.SortIndex);
                    ReleaseConnectionId(player.ConnectionId);
                    return true;
                }
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
            }
            
            // Retrieve encryption params from handshake process by player session token, if provided
            if (!string.IsNullOrEmpty(connectionRequestData.PlayerSessionId))
            {
                var handshakeSession =
                    _handshakeSessionRegistry.TryGetByPlayerSessionId(connectionRequestData.PlayerSessionId);

                if (handshakeSession != null && handshakeSession.EncryptionParameters != null)
                {
                    _packetEncryptionLayer.AddEncryptedEndPoint((IPEndPoint)endPoint, 
                        handshakeSession.EncryptionParameters, true);
                }
            }
            
            return false;
        }

        public override void OnLatencyUpdate(EndPoint endPoint, int latency)
            => _logger.Verbose($"Latency updated (RemoteEndPoint='{endPoint}', Latency={0.001f * latency}).");

        object ConnectionLock = new();
        private readonly SyncTimePacket _SyncTimePacket = new();
        public override void OnConnect(EndPoint endPoint)
        {
            lock (ConnectionLock)
            {
                _logger.Information($"Endpoint connected (RemoteEndPoint='{endPoint}')");

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
                _SyncTimePacket.SyncTime = RunTime;
                _packetDispatcher.SendToNearbyPlayers(_SyncTimePacket, DeliveryMethod.ReliableOrdered);

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
                    RemoteConnectionId = 0,//TODO testing this
                    UserId = _configuration.ServerId,
                    UserName = _configuration.ServerName,
                    IsConnectionOwner = true
                }, DeliveryMethod.ReliableOrdered);

                foreach (IPlayer p in _playerRegistry.Players)
                {
                    if(p.ConnectionId != player.ConnectionId)
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

                // Disable start button if they are server owner without selected song
                _packetDispatcher.SendToPlayer(player, new SetIsStartButtonEnabledPacket
                {
                    Reason = player.UserId == _configuration.ServerOwnerId ? CannotStartGameReason.NoSongSelected : CannotStartGameReason.None
                }, DeliveryMethod.ReliableOrdered);

                // Update permissions
                if ((_configuration.SetConstantManagerFromUserId == player.UserId || _playerRegistry.GetPlayerCount() == 1) && _configuration.GameplayServerMode == Enums.GameplayServerMode.Managed)
                {
                    _configuration.ServerOwnerId = player.UserId;
                }

                _packetDispatcher.SendToNearbyPlayers(new SetPlayersPermissionConfigurationPacket
                {
                    PermissionConfiguration = new PlayersPermissionConfiguration
                    {
                        PlayersPermission = _playerRegistry.Players.Select(x => new PlayerPermissionConfiguration
                        {
                            UserId = x.UserId,
                            IsServerOwner = x.IsServerOwner,
                            HasRecommendBeatmapsPermission = x.CanRecommendBeatmaps,
                            HasRecommendGameplayModifiersPermission = x.CanRecommendModifiers,
                            HasKickVotePermission = x.CanKickVote,
                            HasInvitePermission = x.CanInvite
                        }).ToArray()
                    }
                }, DeliveryMethod.ReliableOrdered);
                PlayerConnectedEvent?.Invoke(player, _playerRegistry.GetPlayerCount());
            }
            
        }

        object DisconnectplayerLock = new();
        public void DisconnectPlayer(string UserId)
        {
            lock (DisconnectplayerLock)
            {
                if(_playerRegistry.TryGetPlayer(UserId, out var player))
                    _packetDispatcher.SendToPlayer(player, new KickPlayerPacket
                    {
                        DisconnectedReason = DisconnectedReason.Kicked
                    }, DeliveryMethod.ReliableOrdered);
            }
        }

        object DisconnectLock = new();
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
            lock (DisconnectLock)
            {
                if (_playerRegistry.TryGetPlayer(endPoint, out var player))
                {
                    _packetDispatcher.SendFromPlayer(player, new PlayerDisconnectedPacket
                    {
                        DisconnectedReason = DisconnectedReason.ClientConnectionClosed
                    }, DeliveryMethod.ReliableOrdered);

                    if (_configuration.ServerOwnerId == player.UserId)
                        _configuration.ServerOwnerId = "";

                    _playerRegistry.RemovePlayer(player);
                    ReleaseSortIndex(player.SortIndex);
                    ReleaseConnectionId(player.ConnectionId);

                    PlayerDisconnectedEvent?.Invoke(player, _playerRegistry.GetPlayerCount());
                }


                if (_playerRegistry.GetPlayerCount() == 0)
                {
                    NoPlayersTime = RunTime;
                    if (_configuration.DestroyInstanceTimeout != -1)
                    {
                        _waitForPlayerCts = new CancellationTokenSource();
                        _ = Task.Delay((int)(_configuration.DestroyInstanceTimeout * 1000), _waitForPlayerCts.Token).ContinueWith(t =>
                        {
                            if (!t.IsCanceled && _playerRegistry.GetPlayerCount() == 0)
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
                    // Set new server owner if server owner left
                    if (_configuration.ServerOwnerId == "" && _configuration.GameplayServerMode == GameplayServerMode.Managed)
                    {
                        _configuration.ServerOwnerId = _playerRegistry.Players[0].UserId;
                        var serverOwner = _playerRegistry.GetPlayer(_configuration.ServerOwnerId);

                        // Disable start button if they are server owner without selected song
                        if (serverOwner.BeatmapIdentifier == null)
                            _packetDispatcher.SendToPlayer(serverOwner, new SetIsStartButtonEnabledPacket
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
                                    IsServerOwner = x.IsServerOwner,
                                    HasRecommendBeatmapsPermission = x.CanRecommendBeatmaps,
                                    HasRecommendGameplayModifiersPermission = x.CanRecommendModifiers,
                                    HasKickVotePermission = x.CanKickVote,
                                    HasInvitePermission = x.CanInvite
                                }).ToArray()
                            }
                        }, DeliveryMethod.ReliableOrdered);
                    }
                }
            }
        }
        #endregion

        #region Private Methods

        private async void SendSyncTime(CancellationToken cancellationToken)
        {
            foreach (IPlayer player in _playerRegistry.Players)
            {
                _SyncTimePacket.SyncTime = player.SyncTime;
                _packetDispatcher.SendToPlayer(player, _SyncTimePacket, DeliveryMethod.ReliableOrdered);
            }
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
