using System;
using System.Collections.Generic;
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
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MPChatPackets;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;
using BeatTogether.LiteNetLib;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Configuration;
using BeatTogether.LiteNetLib.Enums;
using BeatTogether.LiteNetLib.Util;
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
        public event Action<IPlayer> PlayerConnectedEvent = null!;
        public event Action<IPlayer> PlayerDisconnectedEvent = null!;
        public event Action<string, EndPoint, string[]> PlayerDisconnectBeforeJoining = null!;
        public event Action<string, bool> GameIsInLobby = null!;
        public event Action<IDedicatedInstance> UpdateInstanceEvent = null!;

        private readonly IHandshakeSessionRegistry _handshakeSessionRegistry;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly PacketEncryptionLayer _packetEncryptionLayer;
        public SemaphoreSlim ConnectDisconnectSemaphore { get; } = new(1); //Stops lobby loop from running during player connection/disconnection, and stops disconnects and connects happening simultaneously

        private byte _connectionIdCount = 0;
        private readonly object _ConnectionIDLock = new();
        private int _lastSortIndex = -1;
        private readonly object _SortIndexLock = new();
        private readonly Queue<byte> _releasedConnectionIds = new();
        private readonly object _releasedConnectionIds_Lock = new();
        private readonly Queue<int> _releasedSortIndices = new();
        private readonly object _releasedSortIndices_Lock = new();
        private readonly ILogger _logger = Log.ForContext<DedicatedInstance>();

        private long _startTime;
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
                  (configuration.MaxPlayerCount/20+1),
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

        public int GetNextSortIndex()
        {
            lock (_releasedSortIndices_Lock)
            {
                if (_releasedSortIndices.TryDequeue(out var sortIndex))
                    return sortIndex;
            }
            lock (_SortIndexLock)
            {
                _lastSortIndex++;
                if (_lastSortIndex > _configuration.MaxPlayerCount)
                {
                    return 0;
                }
                return _lastSortIndex;
            }
        }

        public void ReleaseSortIndex(int sortIndex)
        {
            lock (_releasedSortIndices_Lock)
            {
                _releasedSortIndices.Enqueue(sortIndex);
            }
        }
                    
        public byte GetNextConnectionId() //ID 0 is server, ID 127 means send to all players
        {
            lock (_releasedConnectionIds_Lock)
            {
                if (_releasedConnectionIds.TryDequeue(out var connectionId))
                    return connectionId;
            }
            lock (_ConnectionIDLock)
            {
                _connectionIdCount++;
                if (_connectionIdCount == 127)
                    _connectionIdCount++;
                if (_connectionIdCount > (byte.MaxValue - 5)) //Currently not implimented to use ID's over 126 client side
                    return 255; //Give them an unusedID so they dont conflict with anyone
                return _connectionIdCount;
            }
        }

        public void ReleaseConnectionId(byte connectionId)
        {
            lock (_releasedConnectionIds_Lock)
            {
                _releasedConnectionIds.Enqueue(connectionId);
            }
        }

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

        public override async Task<bool> ShouldAcceptConnection(EndPoint endPoint, MemoryBuffer additionalData)
        {
            
            if (await ShouldDenyConnection(endPoint, additionalData))
            {
                string[] Players = _playerRegistry.Players.Select(p => p.UserId).ToArray();
                PlayerDisconnectBeforeJoining?.Invoke(_configuration.Secret, endPoint, Players);
                return false;
            }
            return true;
        }
        public async Task<bool> ShouldDenyConnection(EndPoint endPoint, MemoryBuffer additionalData)
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
                $"UserName='{connectionRequestData.UserName}', "
            );

            if (string.IsNullOrEmpty(connectionRequestData.UserId) ||
                string.IsNullOrEmpty(connectionRequestData.UserName))
            {
                _logger.Warning(
                    "Received a connection request with invalid data " +
                    $"(RemoteEndPoint='{endPoint}', " +
                    $"UserId='{connectionRequestData.UserId}', " +
                    $"UserName='{connectionRequestData.UserName}', " +
                    $"IsConnectionOwner={connectionRequestData.IsConnectionOwner})."
                );
                return true;
            }
            await ConnectDisconnectSemaphore.WaitAsync();
            if (_playerRegistry.GetPlayerCount() >= _configuration.MaxPlayerCount)
            {
                _logger.Warning("Master server sent a player to a full server");
                return true;
            }
            if (connectionRequestData.UserName == "IGGAMES" || connectionRequestData.UserName == "IGGGAMES")
            {
                _logger.Information("an IGG player just tried joining after passing master auth");
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

            if (_configuration.ServerName == string.Empty)
            {
                _logger.Information("About to update servers name" + _configuration.ServerName);
                _configuration.ServerName = player.UserName + "'s server";
                InstanceConfigUpdated();
                _logger.Information("Updated servers name to: " + _configuration.ServerName);
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
            
            // Retrieve encryption params and platform data from handshake process by player session token, if provided
            if (!string.IsNullOrEmpty(connectionRequestData.PlayerSessionId))
            {
                var handshakeSession =
                    _handshakeSessionRegistry.TryGetByPlayerSessionId(connectionRequestData.PlayerSessionId);
                _handshakeSessionRegistry.RemoveExtraPlayerSessionData(connectionRequestData.PlayerSessionId, out var ClientVer, out var Platform, out var PlayerPlatformUserId);
                player.ClientVersion = ClientVer;
                player.Platform = (Platform)Platform;
                player.PlatformUserId = PlayerPlatformUserId;
                if (handshakeSession != null && handshakeSession.EncryptionParameters != null)
                {
                    _packetEncryptionLayer.AddEncryptedEndPoint((IPEndPoint)endPoint, 
                        handshakeSession.EncryptionParameters, true);
                }
            }
            ConnectDisconnectSemaphore.Release();
            return false;
        }
        public override void OnLatencyUpdate(EndPoint endPoint, int latency)
            => _logger.Verbose($"Latency updated (RemoteEndPoint='{endPoint}', Latency={0.001f * latency}).");

        

        public override async void OnConnect(EndPoint endPoint)
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
            await ConnectDisconnectSemaphore.WaitAsync();

            IPlayer[] PlayersAtJoin = _playerRegistry.Players;

            await player.PlayerAccessSemaphore.WaitAsync();

            var Player_ConnectPacket = new INetSerializable[]
               {
                new SyncTimePacket
                    {
                        SyncTime = RunTime
                    },
                new PlayerSortOrderPacket
                    {
                        UserId = player.UserId,
                        SortIndex = player.SortIndex
                    },
                new PlayerConnectedPacket
                    {
                        RemoteConnectionId = 0,
                        UserId = _configuration.ServerId,
                        UserName = _configuration.ServerName,
                        IsConnectionOwner = true
                    },
                new SetIsStartButtonEnabledPacket// Disables start button if they are server owner without selected song
                    {
                        Reason = player.UserId == _configuration.ServerOwnerId ? CannotStartGameReason.NoSongSelected : CannotStartGameReason.None
                    }
               };

            var SendToOtherPlayers = new INetSerializable[]
            {
            new SyncTimePacket
                {
                    SyncTime = RunTime
                },
            new PlayerConnectedPacket
                {
                    RemoteConnectionId = player.ConnectionId,
                    UserId = player.UserId,
                    UserName = player.UserName,
                    IsConnectionOwner = false
                },
            new PlayerSortOrderPacket
                {
                    UserId = player.UserId,
                    SortIndex = player.SortIndex
                },
            };
            player.PlayerAccessSemaphore.Release();
            _packetDispatcher.SendToPlayer(player, Player_ConnectPacket, DeliveryMethod.ReliableOrdered);

            //Sends to all players that they have connected
            await _packetDispatcher.SendExcludingPlayerAndAwait(player, SendToOtherPlayers, DeliveryMethod.ReliableOrdered).WaitAsync(TimeSpan.FromMilliseconds(100));
            INetSerializable[] SendToPlayer;
            INetSerializable[] SendToPlayerFromPlayers;
            Task[] ConnectionTasks = new Task[2];
            foreach (IPlayer p in PlayersAtJoin)
            {
                if (p.ConnectionId != player.ConnectionId)
                {
                    // Send all player connection data packets to new player
                    if (!p.PlayerInitialised.Task.IsCompleted)
                        await p.PlayerInitialised.Task.WaitAsync(TimeSpan.FromMilliseconds(200)); //awaits the player connecting
                    await p.PlayerAccessSemaphore.WaitAsync();
                    SendToPlayer = new INetSerializable[]{
                        new PlayerConnectedPacket
                        {
                            RemoteConnectionId = p.ConnectionId,
                            UserId = p.UserId,
                            UserName = p.UserName,
                            IsConnectionOwner = false
                        },
                        new PlayerSortOrderPacket
                        {
                            UserId = p.UserId,
                            SortIndex = p.SortIndex
                        }
                    };
                    SendToPlayerFromPlayers = new INetSerializable[]
                    {
                        new PlayerAvatarPacket
                        {
                            PlayerAvatar = p.Avatar
                        },
                        new PlayerStatePacket
                        {
                            PlayerState = p.State
                        },
                        new MpPlayerData
                        {
                            PlatformID = p.PlatformUserId!,
                            Platform = (byte)p.Platform,
                            ClientVersion = p.ClientVersion!
                        }
                    };
                    p.PlayerAccessSemaphore.Release();
                    ConnectionTasks[0] = _packetDispatcher.SendToPlayerAndAwait(player, SendToPlayer, DeliveryMethod.ReliableOrdered);

                    // Send all player avatars and states to just joined player
                    ConnectionTasks[1] = _packetDispatcher.SendFromPlayerToPlayerAndAwait(p, player, SendToPlayerFromPlayers, DeliveryMethod.ReliableOrdered);
                    await Task.WhenAll(ConnectionTasks).WaitAsync(TimeSpan.FromMilliseconds(100));
                }
            }

            // Update permissions - constant manager possibly does not work
            if ((_configuration.SetConstantManagerFromUserId == player.UserId || _playerRegistry.GetPlayerCount() == 1) && _configuration.GameplayServerMode == GameplayServerMode.Managed)
            {
                _configuration.ServerOwnerId = player.UserId;
            }

            //Send remaining join packets
            ConnectionTasks = new Task[4];
            foreach (IPlayer p in PlayersAtJoin)
            {
                ConnectionTasks[0] = _packetDispatcher.SendToPlayerAndAwait(p,new SetPlayersPermissionConfigurationPacket
                {
                    PermissionConfiguration = new PlayersPermissionConfiguration
                    {
                        PlayersPermission = PlayersAtJoin.Select(x => new PlayerPermissionConfiguration
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
                if (p.ConnectionId != player.ConnectionId)
                {
                    ConnectionTasks[1] = _packetDispatcher.SendFromPlayerToPlayerAndAwait(player, p, new MpPlayerData
                    {
                        PlatformID = player.PlatformUserId!,
                        Platform = (byte)player.Platform,
                        ClientVersion = player.ClientVersion!
                    }, DeliveryMethod.ReliableOrdered);
                }
                else
                {
                    ConnectionTasks[1] = Task.CompletedTask;
                }
                ConnectionTasks[2] = _packetDispatcher.SendToPlayerAndAwait(p, new MpNodePoseSyncStatePacket
                {
                    fullStateUpdateFrequency = 0.1f,
                    deltaUpdateFrequency = _playerRegistry.GetMillisBetweenSyncStatePackets() * 0.001f
                }, DeliveryMethod.ReliableOrdered);
                ConnectionTasks[3] = _packetDispatcher.SendToPlayerAndAwait(p, new MpcTextChatPacket()
                {
                    Text = "Player joined: " + player.UserName + " Platform: " + player.Platform.ToString() + " Game version: " + player.ClientVersion
                }, DeliveryMethod.ReliableOrdered);
                await Task.WhenAll(ConnectionTasks).WaitAsync(TimeSpan.FromMilliseconds(100));
            }
            ConnectDisconnectSemaphore.Release();
            PlayerConnectedEvent?.Invoke(player);
        }

        public void DisconnectPlayer(string UserId) //Used my master server kick player event
        {

            if(_playerRegistry.TryGetPlayer(UserId, out var player))
                _packetDispatcher.SendToPlayer(player, new KickPlayerPacket
                {
                    DisconnectedReason = DisconnectedReason.Kicked
                }, DeliveryMethod.ReliableOrdered);
        }

        public override async void OnDisconnect(EndPoint endPoint, DisconnectReason reason)
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
            await ConnectDisconnectSemaphore.WaitAsync();
            if (_playerRegistry.TryGetPlayer(endPoint, out var player))
            {
                //Sends to all players that they have disconnected
                _packetDispatcher.SendFromPlayer(player, new PlayerDisconnectedPacket
                {
                    DisconnectedReason = DisconnectedReason.ClientConnectionClosed
                }, DeliveryMethod.ReliableOrdered);
                
                if (_configuration.ServerOwnerId == player.UserId)
                    _configuration.ServerOwnerId = "";

                _playerRegistry.RemovePlayer(player);
                ReleaseSortIndex(player.SortIndex);
                ReleaseConnectionId(player.ConnectionId);

                PlayerDisconnectedEvent?.Invoke(player);
            }
            ConnectDisconnectSemaphore.Release();

            if (_playerRegistry.GetPlayerCount() != 0 && string.IsNullOrEmpty(_configuration.ServerOwnerId) && _configuration.GameplayServerMode == GameplayServerMode.Managed)
            {
                var serverOwner = _playerRegistry.Players[0];
                _configuration.ServerOwnerId = serverOwner.UserId;

                // Update permissions
                _packetDispatcher.SendToNearbyPlayers(new SetPlayersPermissionConfigurationPacket
                {
                    PermissionConfiguration = new PlayersPermissionConfiguration
                    {
                        PlayersPermission = new PlayerPermissionConfiguration[]
                        {
                            new PlayerPermissionConfiguration()
                            {
                                UserId = serverOwner!.UserId,
                                IsServerOwner = serverOwner.IsServerOwner,
                                HasRecommendBeatmapsPermission = serverOwner.CanRecommendBeatmaps,
                                HasRecommendGameplayModifiersPermission = serverOwner.CanRecommendModifiers,
                                HasKickVotePermission = serverOwner.CanKickVote,
                                HasInvitePermission = serverOwner.CanInvite
                            }
                        }
                    }
                }, DeliveryMethod.ReliableOrdered);

                // Disable start button if they are server owner without selected song
                if (serverOwner.BeatmapIdentifier == null)
                    _packetDispatcher.SendToPlayer(serverOwner, new SetIsStartButtonEnabledPacket
                    {
                        Reason = CannotStartGameReason.NoSongSelected
                    }, DeliveryMethod.ReliableOrdered);
            }


            _packetDispatcher.SendToNearbyPlayers(new MpNodePoseSyncStatePacket
            {
                fullStateUpdateFrequency = 0.1f,
                deltaUpdateFrequency = _playerRegistry.GetMillisBetweenSyncStatePackets() * 0.001f
            }, DeliveryMethod.ReliableOrdered);

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
        }
        #endregion

        #region Private Methods

        private async void SendSyncTime(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (IPlayer player in _playerRegistry.Players)
                {
                    _packetDispatcher.SendToPlayer(player, new SyncTimePacket()
                    {
                        SyncTime = player.SyncTime
                    }, DeliveryMethod.ReliableOrdered);
                }
                try
                {
                    await Task.Delay(SyncTimeDelay, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }

        public void InstanceConfigUpdated()
        {
            UpdateInstanceEvent?.Invoke(this);
        }

        #endregion
    }
}
