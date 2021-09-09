using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Models;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using LiteNetLib;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class MatchmakingServer : IMatchmakingServer, INetEventListener
    {
        public string Secret { get; private set; } = null!;
        public string ManagerId { get; private set; } = null!;
        public GameplayServerConfiguration Configuration { get; private set; } = null!;
        public bool IsRunning => _netManager.IsRunning;
        public float RunTime => (DateTime.UtcNow.Ticks - _startTime) / 10000000.0f;
        public int Port => _netManager.LocalPort;
        public MultiplayerGameState State 
        {
            get => _state;
            set
            {
                _state = value;
                var gameStatePacket = new SetMultiplayerGameStatePacket
                {
                    State = value
                };
                _packetDispatcher.SendToNearbyPlayers(_netManager, gameStatePacket, DeliveryMethod.ReliableOrdered);
            }
        }


        private MultiplayerGameState _state = MultiplayerGameState.Lobby;
        private readonly PacketEncryptionLayer _packetEncryptionLayer;
        private readonly IPortAllocator _portAllocator;
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<MatchmakingServer>();

        private readonly IServiceAccessor<IMatchmakingServer> _matchmakingServerAccessor;
        private readonly IServiceAccessor<IPlayerRegistry> _playerRegistryAccessor;
        private readonly IServiceAccessor<IPacketSource> _packetSourceAccessor;
        private readonly IServiceAccessor<IPermissionsManager> _permissionsManagerAccessor;
        private readonly IServiceAccessor<IEntitlementManager> _entitlementManagerAccessor;
        private readonly IServiceAccessor<ILobbyManager> _lobbyManagerAccessor; 

        private IPacketSource _packetSource = null!;
        private IPlayerRegistry _playerRegistry = null!;
        private IPermissionsManager _permissionsManager = null!;
        private IEntitlementManager _entitlementManager = null!;
        private ILobbyManager _lobbyManager = null!;

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

        public MatchmakingServer(
            IPortAllocator portAllocator,
            PacketEncryptionLayer packetEncryptionLayer,
            IPacketDispatcher packetDispatcher,
            IServiceAccessor<IMatchmakingServer> matchmakingServerAccessor,
            IServiceAccessor<IPlayerRegistry> playerRegistryAccessor,
            IServiceAccessor<IPacketSource> packetSourceAccessor,
            IServiceAccessor<IPermissionsManager> permissionsManagerAccessor,
            IServiceAccessor<IEntitlementManager> entitlementManagerAccessor,
            IServiceAccessor<ILobbyManager> lobbyManagerAccessor)
        {
            _portAllocator = portAllocator;
            _packetEncryptionLayer = packetEncryptionLayer;
            _packetDispatcher = packetDispatcher;

            _matchmakingServerAccessor = matchmakingServerAccessor;
            _playerRegistryAccessor = playerRegistryAccessor;
            _packetSourceAccessor = packetSourceAccessor;
            _permissionsManagerAccessor = permissionsManagerAccessor;
            _entitlementManagerAccessor = entitlementManagerAccessor;
            _lobbyManagerAccessor = lobbyManagerAccessor;

            _netManager = new NetManager(this, packetEncryptionLayer);
        }

        #region Public Methods

        public void Init(string secret, string managerId, GameplayServerConfiguration configuration)
        {
            Secret = secret;
            ManagerId = managerId;
            Configuration = configuration;
        }

        public async Task Start(CancellationToken cancellationToken = default)
        {
            if (_task is null)
                await Stop(cancellationToken);

            var port = _portAllocator.AcquirePort();
            if (!port.HasValue)
                return;

            _ = _matchmakingServerAccessor.Bind(this);
            _playerRegistry = _playerRegistryAccessor.Create();
            _packetSource = _packetSourceAccessor.Create();
            _permissionsManager = _permissionsManagerAccessor.Create();
            _entitlementManager = _entitlementManagerAccessor.Create();
            _lobbyManager = _lobbyManagerAccessor.Create();

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

            var setIsStartButtonEnabledPacket = new SetIsStartButtonEnabledPacket
            {
                Reason = player.UserId == ManagerId ? CannotStartGameReason.NoSongSelected : CannotStartGameReason.None
            };
            _packetDispatcher.SendToPlayer(player, setIsStartButtonEnabledPacket, DeliveryMethod.ReliableOrdered);

            _permissionsManager.UpdatePermissions();
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
                    _playerRegistry.RemovePlayer(player);
                }

                if (_playerRegistry.Players.Count == 0)
                {
                    _ = Stop(CancellationToken.None);
                    _cancellationTokenSource?.Cancel();
                }
            }
        }

        #endregion

        #region Private Methods

        private async Task PollEvents(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _netManager.PollEvents();
                    
                    _lobbyManager.Update();

                    if (_lastSyncTimeUpdate < RunTime - _syncTimeDelay)
                    {
                        foreach (var player in _playerRegistry.Players)
                        {
                            var syncTimePacket = new SyncTimePacket
                            {
                                SyncTime = player.SyncTime
                            };
                            _packetDispatcher.SendToPlayer(player, syncTimePacket, DeliveryMethod.ReliableOrdered);
                        }

                        _lastSyncTimeUpdate = RunTime;
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Exception occurred in PollEvents");
                }

                await Task.Delay(_eventPollDelay, cancellationToken);
            }
        }

        #endregion
    }
}