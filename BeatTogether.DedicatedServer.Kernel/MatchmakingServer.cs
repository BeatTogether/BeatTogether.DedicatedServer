using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Models;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets;
using LiteNetLib;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class MatchmakingServer : IMatchmakingServer, INetEventListener
    {
        public string Secret { get; }
        public string ManagerId { get; }
        public GameplayServerConfiguration Configuration { get; }
        public bool IsRunning => _netManager.IsRunning;
        public float RunTime => (DateTime.UtcNow.Ticks - _startTime) / 10000000.0f;
        public int Port => _netManager.LocalPort;
        public MultiplayerGameState State { get; private set; } = MultiplayerGameState.Lobby;

        private readonly PacketEncryptionLayer _packetEncryptionLayer;
        private readonly IPortAllocator _portAllocator;
        private readonly IPacketSource _packetSource;
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly ILogger _logger = Log.ForContext<MatchmakingServer>();

        private long _startTime;
        private readonly NetManager _netManager;
        private readonly ConcurrentDictionary<byte, IPlayer> _playersByConnectionId = new();
        private readonly ConcurrentDictionary<string, IPlayer> _playersByUserId = new();
        private readonly ConcurrentQueue<byte> _releasedConnectionIds = new();
        private int _connectionIdCount = 0;
        private readonly ConcurrentQueue<int> _releasedSortIndices = new();
        private int _lastSortIndex = -1;

        private Task? _task;
        private CancellationTokenSource? _cancellationTokenSource;

        private const int _eventPollDelay = 10;

        public MatchmakingServer(
            IPortAllocator portAllocator,
            PacketEncryptionLayer packetEncryptionLayer,
            IPacketSource packetSource,
            IPacketDispatcher packetDispatcher,
            IPlayerRegistry playerRegistry,
            string secret,
            string managerId,
            GameplayServerConfiguration configuration)
        {
            Secret = secret;
            ManagerId = managerId;
            Configuration = configuration;

            _portAllocator = portAllocator;
            _packetEncryptionLayer = packetEncryptionLayer;
            _packetSource = packetSource;
            _packetDispatcher = packetDispatcher;
            _playerRegistry = playerRegistry;

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

        public IPlayer GetPlayer(byte connectionId) =>
            _playersByConnectionId[connectionId];

        public IPlayer GetPlayer(string userId) =>
            _playersByUserId[userId];

        public bool TryGetPlayer(byte connectionId, [MaybeNullWhen(false)] out IPlayer player) =>
            _playersByConnectionId.TryGetValue(connectionId, out player);

        public bool TryGetPlayer(string userId, [MaybeNullWhen(false)] out IPlayer player) =>
            _playersByUserId.TryGetValue(userId, out player);

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
            if (!_playersByUserId.TryAdd(player.UserId, player))
            {
                _logger.Warning(
                    "Player failed to join matchmaking server " +
                    $"(RemoteEndPoint='{player.NetPeer.EndPoint}', " +
                    $"ConnectionId={player.ConnectionId}, " +
                    $"Secret='{player.Secret}', " +
                    $"UserId='{player.UserId}', " +
                    $"UserName='{player.UserName}', " +
                    $"SortIndex={player.SortIndex})."
                );
                // TODO: Kick player
                return;
            }
            _playersByConnectionId[connectionId] = player;
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
            _packetDispatcher.SendToNearbyPlayers(player, syncTimePacket, DeliveryMethod.ReliableOrdered);

            var playerSortOrderPacket = new PlayerSortOrderPacket
            {
                UserId = player.UserId,
                SortIndex = player.SortIndex
            };
            _packetDispatcher.SendToNearbyPlayers(player, playerSortOrderPacket, DeliveryMethod.ReliableOrdered);
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
            }
        }

        #endregion

        #region Private Methods

        private async Task PollEvents(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _netManager.PollEvents();
                await Task.Delay(_eventPollDelay, cancellationToken);
            }
        }

        #endregion
    }
}
