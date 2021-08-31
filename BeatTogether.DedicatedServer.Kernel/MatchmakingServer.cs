using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Models;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets;
using LiteNetLib;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class MatchmakingServer : IMatchmakingServer, INetEventListener
    {
        public string Secret { get => _serverContext?.Secret ?? _tempSecret; }
        public string ManagerId { get => _serverContext?.ManagerId ?? _tempSecret; private set => _serverContext.ManagerId = value; }
        public GameplayServerConfiguration Configuration { get => _serverContext?.Configuration ?? _tempConfiguration; private set => _serverContext.Configuration = value; }
        public PlayersPermissionConfiguration Permissions { get; } = new();
        public bool IsRunning => _netManager.IsRunning;
        public float RunTime => (DateTime.UtcNow.Ticks - _startTime) / 10000000.0f;
        public int Port => _netManager.LocalPort;
        public MultiplayerGameState State { get => _serverContext.State; private set => _serverContext.State = value; }
        public List<IPlayer> Players { get => _serverContext.Players; }

        private readonly PacketEncryptionLayer _packetEncryptionLayer;
        private readonly IPortAllocator _portAllocator;
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<MatchmakingServer>();

        private readonly IServiceAccessor<IServerContext> _serverContextAccessor;
        private readonly IServiceAccessor<IPlayerRegistry> _playerRegistryAccessor;
        private readonly IServiceAccessor<IPacketSource> _packetSourceAccessor;

        private IServerContext _serverContext = null!;
        private IPacketSource _packetSource = null!;
        private IPlayerRegistry _playerRegistry = null!;

        private long _startTime;
        private readonly NetManager _netManager;
        private readonly ConcurrentQueue<byte> _releasedConnectionIds = new();
        private int _connectionIdCount = 0;
        private readonly ConcurrentQueue<int> _releasedSortIndices = new();
        private int _lastSortIndex = -1;
        private float _lastSyncTimeUpdate;

        private Task? _task;
        private CancellationTokenSource? _cancellationTokenSource;

        private const int _eventPollDelay = 10;
        private const float _syncTimeDelay = 5f;

        private string _tempSecret;
        private string _tempManagerId;
        private GameplayServerConfiguration _tempConfiguration;

        public MatchmakingServer(
            IPortAllocator portAllocator,
            PacketEncryptionLayer packetEncryptionLayer,
            IPacketDispatcher packetDispatcher,
            IServiceAccessor<IServerContext> serverContextAccessor,
            IServiceAccessor<IPlayerRegistry> playerRegistryAccessor,
            IServiceAccessor<IPacketSource> packetSourceAccessor,
            string secret,
            string managerId,
            GameplayServerConfiguration configuration)
        {
            _tempSecret = secret;
            _tempManagerId = managerId;
            _tempConfiguration = configuration;

            _portAllocator = portAllocator;
            _packetEncryptionLayer = packetEncryptionLayer;
            _packetDispatcher = packetDispatcher;

            _serverContextAccessor = serverContextAccessor;
            _playerRegistryAccessor = playerRegistryAccessor;
            _packetSourceAccessor = packetSourceAccessor;

            _netManager = new NetManager(this, packetEncryptionLayer);
        }

        #region Public Methods

        public async Task Start(CancellationToken cancellationToken = default)
        {
            if (_task is null)
                await Stop(cancellationToken);

            var port = _portAllocator.AcquirePort();
            if (!port.HasValue)
                return;

            _serverContext = _serverContextAccessor.Create<ServerContext>();
            _playerRegistry = _playerRegistryAccessor.Create<PlayerRegistry>();
            _packetSource = _packetSourceAccessor.Create<PacketSource>();

            _serverContext.Secret = _tempSecret;
            _serverContext.ManagerId = _tempManagerId;
            _serverContext.Configuration = _tempConfiguration;

            _logger.Information(
                "Starting matchmaking server " +
                $"(Port={port}," +
                $"Secret='{Secret}', " +
                $"ManagerId='{ManagerId}', " +
                $"MaxPlayerCount={Configuration.MaxPlayerCount}, " +
                $"DiscoveryPolicy={Configuration.DiscoveryPolicy}, " +
                $"InvitePolicy={Configuration.InvitePolicy}, " +
                $"GameplayServerMode={Configuration.GameplayServerMode}, " +
                $"SongSelectionMode={Configuration.SongSelectionMode}, " +
                $"GameplayServerControlSettings={Configuration.GameplayServerControlSettings})."
            );
            _cancellationTokenSource = new CancellationTokenSource();
            _task = Task.Run(() => PollEvents(_cancellationTokenSource.Token));
            _netManager.Start(port.Value);
            _startTime = DateTime.UtcNow.Ticks;
        }

        public async Task Stop(CancellationToken cancellationToken = default)
        {
            if (_task is null)
                return;

            _logger.Information(
                "Stopping matchmaking server " +
                $"(Port={Port}," +
                $"Secret='{Secret}', " +
                $"ManagerId='{ManagerId}', " +
                $"MaxPlayerCount={Configuration.MaxPlayerCount}, " +
                $"DiscoveryPolicy={Configuration.DiscoveryPolicy}, " +
                $"InvitePolicy={Configuration.InvitePolicy}, " +
                $"GameplayServerMode={Configuration.GameplayServerMode}, " +
                $"SongSelectionMode={Configuration.SongSelectionMode}, " +
                $"GameplayServerControlSettings={Configuration.GameplayServerControlSettings})."
            );
            try
            {
                _cancellationTokenSource?.Cancel();
            }
            finally
            {
                await Task.WhenAny(_task, Task.Delay(Timeout.Infinite, cancellationToken));
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                _task = null;
                _netManager.Stop();
                _portAllocator.ReleasePort(Port);
            }
        }

        public int GetNextSortIndex()
        {
            if (_releasedSortIndices.TryDequeue(out var sortIndex))
                return sortIndex;
            return Interlocked.Increment(ref _lastSortIndex);
        }

        public void ReleaseSortIndex(int sortIndex) =>
            _releasedSortIndices.Enqueue(sortIndex);

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

        #endregion

        #region INetEventListener

        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
        {
            var connectionRequestData = new ConnectionRequestData();
            try
            {
                connectionRequestData.Deserialize(request.Data);
            }
            catch (Exception e)
            {
                _logger.Warning(e,
                    "Failed to deserialize connection request data " +
                    $"(RemoteEndPoint='{request.RemoteEndPoint}')."
                );
                request.Reject();
                return;
            }

            _logger.Debug(
                "Handling connection request " +
                $"(RemoteEndPoint='{request.RemoteEndPoint}', " +
                $"Secret='{connectionRequestData.Secret}', " +
                $"UserId='{connectionRequestData.UserId}', " +
                $"UserName='{connectionRequestData.UserName}', " +
                $"IsConnectionOwner={connectionRequestData.IsConnectionOwner})."
            );

            if (string.IsNullOrEmpty(connectionRequestData.Secret) ||
                string.IsNullOrEmpty(connectionRequestData.UserId) ||
                string.IsNullOrEmpty(connectionRequestData.UserName))
            {
                _logger.Warning(
                    "Received a connection request with invalid data " +
                    $"(RemoteEndPoint='{request.RemoteEndPoint}', " +
                    $"Secret='{connectionRequestData.Secret}', " +
                    $"UserId='{connectionRequestData.UserId}', " +
                    $"UserName='{connectionRequestData.UserName}', " +
                    $"IsConnectionOwner={connectionRequestData.IsConnectionOwner})."
                );
                request.Reject();
                return;
            }

            var netPeer = request.Accept();
            var connectionId = GetNextConnectionId();
            var sortIndex = GetNextSortIndex();
            var player = new Player(
                netPeer,
                this,
                connectionId,
                connectionRequestData.Secret,
                connectionRequestData.UserId,
                connectionRequestData.UserName
            );
            player.SortIndex = sortIndex;
            _serverContext.AddPlayer(player);
            _playerRegistry.AddPlayer(player);
            _logger.Information(
                "Player joined matchmaking server " +
                $"(RemoteEndPoint='{player.NetPeer.EndPoint}', " +
                $"ConnectionId={player.ConnectionId}, " +
                $"Secret='{player.Secret}', " +
                $"UserId='{player.UserId}', " +
                $"UserName='{player.UserName}', " +
                $"SortIndex={player.SortIndex})."
            );
        }

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError) =>
            _logger.Error($"Socket error occurred (SocketError={socketError}).");

        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency) =>
            _logger.Verbose($"Latency updated (RemoteEndPoint='{peer.EndPoint}', Latency={0.001f * latency}).");

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod) =>
            _packetSource.Signal(peer, reader, deliveryMethod);

        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) =>
            _logger.Verbose(
                "Received unconnected packet " +
                $"(RemoteEndPoint='{remoteEndPoint}', Length={reader.AvailableBytes}, " +
                $"MessageType={messageType})."
            );

        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            _logger.Debug($"Peer connected (RemoteEndPoint='{peer.EndPoint}').");

            if (!_playerRegistry.TryGetPlayer(peer.EndPoint, out var player))
            {
                _logger.Warning(
                    "Failed to retrieve player " +
                    $"(RemoteEndPoint='{peer.EndPoint}')."
                );
                peer.Disconnect();
                return;
            }
            var syncTimePacket = new SyncTimePacket
            {
                SyncTime = player.SyncTime
            };
            _packetDispatcher.SendToPlayer(player, syncTimePacket, DeliveryMethod.ReliableOrdered);

            var playerSortOrderPacket = new PlayerSortOrderPacket
            {
                UserId = player.UserId,
                SortIndex = player.SortIndex
            };
            _packetDispatcher.SendToNearbyPlayers(player, playerSortOrderPacket, DeliveryMethod.ReliableOrdered);

            UpdatePermissions();
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (disconnectInfo.Reason != DisconnectReason.Reconnect &&
                disconnectInfo.Reason != DisconnectReason.PeerToPeerConnection)
            {
                _logger.Debug(
                    "Peer disconnected " +
                    $"(RemoteEndPoint='{peer.EndPoint}', DisconnectInfo={disconnectInfo})."
                );
                _packetEncryptionLayer.RemoveEncryptedEndPoint(peer.EndPoint);

                if (_playerRegistry.TryGetPlayer(peer.EndPoint, out var player))
                {
                    _serverContext!.RemovePlayer(player);
                    _playerRegistry.RemovePlayer(player);
                }

                if (Players.Count == 0)
                {
                    _ = Stop(CancellationToken.None);
                    _cancellationTokenSource?.Cancel();
                }
            }
        }

        #endregion

        #region Private Methods

        private void UpdatePermissions()
        {
            foreach (IPlayer player in _serverContext.Players)
            {
                var playerPermission = new PlayerPermissionConfiguration
                {
                    UserId = player.UserId,
                    IsServerOwner = player.UserId == _serverContext.ManagerId,
                    HasRecommendBeatmapsPermission = true,
                    HasRecommendGameplayModifiersPermission = _serverContext.Configuration.GameplayServerControlSettings == Enums.GameplayServerControlSettings.AllowModifierSelection || _serverContext.Configuration.GameplayServerControlSettings == Enums.GameplayServerControlSettings.All,
                    HasKickVotePermission = true
                };
                Permissions.PlayersPermission.Add(playerPermission);
            }
        }

        private async Task PollEvents(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _netManager.PollEvents();

                if (_lastSyncTimeUpdate < RunTime - _syncTimeDelay)
                {
                    foreach (var player in Players) {
                        var syncTimePacket = new SyncTimePacket
                        {
                            SyncTime = player.SyncTime
                        };
                        _packetDispatcher.SendToPlayer(player, syncTimePacket, DeliveryMethod.ReliableOrdered);
                    }
                    _lastSyncTimeUpdate = RunTime;
                }

                await Task.Delay(_eventPollDelay, cancellationToken);
            }
        }

        #endregion
    }
}
