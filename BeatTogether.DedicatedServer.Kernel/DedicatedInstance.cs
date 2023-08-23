using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Kernel.Encryption;
using BeatTogether.DedicatedServer.Kernel.ENet;
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
using BeatTogether.LiteNetLib.Sources;
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
        public int LiteNetPort => _configuration.LiteNetPort;
        public int ENetPort => _configuration.ENetPort;
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
        
        public IPacketDispatcher PacketDispatcher { get; private set; }
        public ConnectedMessageSource ConnectedMessageSource { get; private set; }

        public readonly ENetServer ENetServer;

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
                  new IPEndPoint(IPAddress.Any, configuration.LiteNetPort),
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

            ENetServer = new(this, configuration.ENetPort);
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

        public async Task Start(CancellationToken cancellationToken = default)
        {
            if (IsRunning)
                return;

            PacketDispatcher = _serviceProvider.GetRequiredService<IPacketDispatcher>();
            ConnectedMessageSource = _serviceProvider.GetRequiredService<ConnectedMessageSource>();
            
            _startTime = DateTime.UtcNow.Ticks;

            _logger.Information(
                "Starting dedicated server " +
                "(Endpoint={Endpoint}," +
                $"ServerName='{_configuration.ServerName}', " +
                $"Secret='{_configuration.Secret}', " +
                $"ManagerId='{_configuration.ServerOwnerId}', " +
                $"MaxPlayerCount={_configuration.MaxPlayerCount}, " +
                $"DiscoveryPolicy={_configuration.DiscoveryPolicy}, " +
                $"InvitePolicy={_configuration.InvitePolicy}, " +
                $"GameplayServerMode={_configuration.GameplayServerMode}, " +
                $"SongSelectionMode={_configuration.SongSelectionMode}, " +
                $"GameplayServerControlSettings={_configuration.GameplayServerControlSettings})",
                Endpoint
            );
            _stopServerCts = new CancellationTokenSource();
            
            if (_configuration.DestroyInstanceTimeout >= 0)
            {
                _waitForPlayerCts = new CancellationTokenSource();
                
                var waitTimeLimit = (WaitForPlayerTimeLimit + (int)(_configuration.DestroyInstanceTimeout * 1000));
                
                _ = Task.Delay(waitTimeLimit, _waitForPlayerCts.Token)
                    .ContinueWith(t =>
                {
                    if (!t.IsCanceled)
                    {
                        _logger.Warning("Stopping instance (no players joined timeout): {Instance}",
                            _configuration.ServerName);
                        _ = Stop(CancellationToken.None);
                    }
                    else
                    {
                        _waitForPlayerCts = null;
                    }
                }, cancellationToken);
            }

            base.Start();
            await ENetServer.Start();
            
            _ = Task.Run(() => SendSyncTime(_stopServerCts.Token), cancellationToken);
        }

        public async Task Stop(CancellationToken cancellationToken = default)
        {
            if (!IsRunning)
                return;

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
            
            PacketDispatcher.SendToNearbyPlayers(new KickPlayerPacket
            {
                DisconnectedReason = DisconnectedReason.ServerTerminated
            }, DeliveryMethod.ReliableOrdered);

            ENetServer.KickAllPeers();

            _stopServerCts!.Cancel();
            _waitForPlayerCts!.Cancel();
            
            StopEvent?.Invoke(this);

            base.Stop();
            await ENetServer.Stop();
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
            PacketDispatcher.SendToNearbyPlayers(new SetMultiplayerGameStatePacket()
            {
                State = state
            }, DeliveryMethod.ReliableOrdered);
            GameIsInLobby?.Invoke(_configuration.Secret, state == MultiplayerGameState.Lobby);
        }

        #endregion

        #region EnetCompat

        public IPlayer? TryAcceptConnection(IPEndPoint endPoint, ref SpanBuffer Data)
        {
            bool PlayerNoJoin = false;

            ConnectionRequestData connectionRequestData = new();
            try
            {
                connectionRequestData.ReadFrom(ref Data);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex +
                    "Failed to deserialize connection request data" +
                    $"(RemoteEndPoint='{endPoint}')."
                );
                PlayerNoJoin = true;
                goto EndOfTryAccept;
            }

            _logger.Debug(
                "Handling connection request though Enet" +
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
                PlayerNoJoin = true;
                goto EndOfTryAccept;
            }
            ConnectDisconnectSemaphore.Wait(); //Will fix joining issues, and although it will block any Enet recieving, hopefully its not that bad
            if (_playerRegistry.GetPlayerCount() >= _configuration.MaxPlayerCount)
            {
                ConnectDisconnectSemaphore.Release();
                _logger.Warning("Master server sent a player to a full server");
                PlayerNoJoin = true;
                goto EndOfTryAccept;
            }
            if (connectionRequestData.UserName == "IGGAMES" || connectionRequestData.UserName == "IGGGAMES")
            {
                ConnectDisconnectSemaphore.Release();
                _logger.Information("an IGG player just tried joining after passing master auth");
                PlayerNoJoin = true;
                goto EndOfTryAccept;
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
                ConnectDisconnectSemaphore.Release();
                PlayerNoJoin = true;
                goto EndOfTryAccept;
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
            Task.Run(() => SendPlayerDataInBackgroundAsync(player));
            return player;
            EndOfTryAccept:
            if (PlayerNoJoin)
            {
                string[] Players = _playerRegistry.Players.Select(p => p.UserId).ToArray();
                PlayerDisconnectBeforeJoining?.Invoke(_configuration.Secret, endPoint, Players);
                return null;
            }
            return null;
        }

        private async void SendPlayerDataInBackgroundAsync(IPlayer player)
        {
            await player.PlayerAccessSemaphore.WaitAsync();
            var SendToOtherPlayers = new INetSerializable[]
            {
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
            new MpNodePoseSyncStatePacket
                {
                    fullStateUpdateFrequency = 0.1f,
                    deltaUpdateFrequency = _playerRegistry.GetMillisBetweenSyncStatePackets() * 0.001f
                },
            };
            player.PlayerAccessSemaphore.Release();

            PacketDispatcher.SendExcludingPlayer(player, SendToOtherPlayers, DeliveryMethod.ReliableOrdered);
        }

        #endregion


        #region LiteNetServer


        public override async Task<bool> ShouldAcceptConnection(EndPoint endPoint, byte[] additionalData)
        {
            
            if (await ShouldDenyConnection(endPoint, additionalData))
            {
                string[] Players = _playerRegistry.Players.Select(p => p.UserId).ToArray();
                PlayerDisconnectBeforeJoining?.Invoke(_configuration.Secret, endPoint, Players);
                return false;
            }

            return true;
        }

        private bool GetConnectionData(byte[] additionalData, out ConnectionRequestData connectionRequestData)
        {
            connectionRequestData = new();
            SpanBuffer spanBuffer = new(additionalData);
            try
            {
                connectionRequestData.ReadFrom(ref spanBuffer);
            }
            catch(Exception ex)
            {
                _logger.Warning(ex +
                    "Failed to deserialize connection request data"
                );
                return false;
            }
            return true;
        }


        public async Task<bool> ShouldDenyConnection(EndPoint endPoint, byte[] additionalData)
        {
            //var connectionRequestData = new ConnectionRequestData();
            if(!GetConnectionData(additionalData, out var connectionRequestData))
            {
                _logger.Warning(
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
                ConnectDisconnectSemaphore.Release();
                _logger.Warning("Master server sent a player to a full server");
                return true;
            }
            if (connectionRequestData.UserName == "IGGAMES" || connectionRequestData.UserName == "IGGGAMES")
            {
                ConnectDisconnectSemaphore.Release();
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
                ConnectDisconnectSemaphore.Release();
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
            //Send to all other players that they have connected, so that they can recieve the routed avatar packet
            await player.PlayerAccessSemaphore.WaitAsync();
            var SendToOtherPlayers = new INetSerializable[]
            {
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
            new MpNodePoseSyncStatePacket
                {
                    fullStateUpdateFrequency = 0.1f,
                    deltaUpdateFrequency = _playerRegistry.GetMillisBetweenSyncStatePackets() * 0.001f
                },
            };
            player.PlayerAccessSemaphore.Release();

            PacketDispatcher.SendExcludingPlayer(player, SendToOtherPlayers, DeliveryMethod.ReliableOrdered);

            
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
            PlayerConnectedEvent?.Invoke(player);
            await Task.Delay(50);
            //The player has already been sent to every other player in the should connect function.

            //Start of sending other players to player
            //Now send all players already in the game to the new player
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
                    },
                new MpNodePoseSyncStatePacket
                    {
                        fullStateUpdateFrequency = 0.1f,
                        deltaUpdateFrequency = _playerRegistry.GetMillisBetweenSyncStatePackets() * 0.001f
                    },
               };
            player.PlayerAccessSemaphore.Release();

            await PacketDispatcher.SendToPlayerAndAwait(player, Player_ConnectPacket, DeliveryMethod.ReliableOrdered);

            List<INetSerializable> MakeBigPacketToSendToPlayer = new();
            foreach (IPlayer p in PlayersAtJoin)
            {
                if(p.ConnectionId != player.ConnectionId)
                {
                    await p.PlayerAccessSemaphore.WaitAsync();
                    MakeBigPacketToSendToPlayer.Add(new PlayerConnectedPacket
                    {
                        RemoteConnectionId = p.ConnectionId,
                        UserId = p.UserId,
                        UserName = p.UserName,
                        IsConnectionOwner = false
                    });
                    MakeBigPacketToSendToPlayer.Add(new PlayerSortOrderPacket
                    {
                        UserId = p.UserId,
                        SortIndex = p.SortIndex
                    });
                    p.PlayerAccessSemaphore.Release();
                }
            }
            foreach (var SubPacket in MakeBigPacketToSendToPlayer.Chunk(20))
            {
                await PacketDispatcher.SendToPlayerAndAwait(player, SubPacket.ToArray(), DeliveryMethod.ReliableOrdered);
            }
            //End of sending other players to player


            // Update permissions - constant manager possibly does not work
            if ((_configuration.SetConstantManagerFromUserId == player.UserId || _playerRegistry.GetPlayerCount() == 1) && _configuration.GameplayServerMode == GameplayServerMode.Managed)
            {
                _configuration.ServerOwnerId = player.UserId;
                PacketDispatcher.SendToNearbyPlayers(new SetPlayersPermissionConfigurationPacket //Sends updated value to all players.//TODO Make a function for updating players permissions as there will be commands for it soon
                {
                    PermissionConfiguration = new PlayersPermissionConfiguration
                    {
                        PlayersPermission = new PlayerPermissionConfiguration[]
                        {
                            new PlayerPermissionConfiguration()
                            {
                                UserId = player.UserId,
                                IsServerOwner = player.IsServerOwner,
                                HasRecommendBeatmapsPermission = player.CanRecommendBeatmaps,
                                HasRecommendGameplayModifiersPermission = player.CanRecommendModifiers,
                                HasKickVotePermission = player.CanKickVote,
                                HasInvitePermission = player.CanInvite
                            }
                        }
                    }
                }, DeliveryMethod.ReliableOrdered);
            }

            if (player.Platform == Platform.Test) //If the player is a bot, send permissions. Normal players request this in a packet when they join
            {
                bool HasManager = (_playerRegistry.TryGetPlayer(_configuration.ServerOwnerId, out var ServerOwner) && !player.IsServerOwner);
                PlayerPermissionConfiguration[] playerPermissionConfigurations = new PlayerPermissionConfiguration[HasManager ? 2 : 1];
                playerPermissionConfigurations[0] = new PlayerPermissionConfiguration
                {
                    UserId = player.UserId,
                    IsServerOwner = player.IsServerOwner,
                    HasRecommendBeatmapsPermission = player.CanRecommendBeatmaps,
                    HasRecommendGameplayModifiersPermission = player.CanRecommendModifiers,
                    HasKickVotePermission = player.CanKickVote,
                    HasInvitePermission = player.CanInvite
                };
                if (HasManager)
                    playerPermissionConfigurations[1] = new PlayerPermissionConfiguration
                    {
                        UserId = ServerOwner!.UserId,
                        IsServerOwner = ServerOwner!.IsServerOwner,
                        HasRecommendBeatmapsPermission = ServerOwner!.CanRecommendBeatmaps,
                        HasRecommendGameplayModifiersPermission = ServerOwner!.CanRecommendModifiers,
                        HasKickVotePermission = ServerOwner!.CanKickVote,
                        HasInvitePermission = ServerOwner!.CanInvite
                    };
                PacketDispatcher.SendToPlayer(player, new SetPlayersPermissionConfigurationPacket
                {
                    PermissionConfiguration = new PlayersPermissionConfiguration
                    {
                        PlayersPermission = playerPermissionConfigurations
                    }
                }, DeliveryMethod.ReliableOrdered);
            }

            //Start of sending player avatars and states of other players to new player
            INetSerializable[] SendToPlayerFromPlayers = new INetSerializable[2];
            SendToPlayerFromPlayers[0] = new PlayerIdentityPacket();
            SendToPlayerFromPlayers[1] = new MpPlayerData();

            foreach (IPlayer p in PlayersAtJoin)
            {
                if (p.ConnectionId != player.ConnectionId)
                {
                    // Send player to player data to new player
                    await p.PlayerAccessSemaphore.WaitAsync();

                    ((PlayerIdentityPacket)SendToPlayerFromPlayers[0]).PlayerState = p.State;
                    ((PlayerIdentityPacket)SendToPlayerFromPlayers[0]).PlayerAvatar = p.Avatar;
                    ((PlayerIdentityPacket)SendToPlayerFromPlayers[0]).Random = new ByteArray { Data = p.Random };
                    ((PlayerIdentityPacket)SendToPlayerFromPlayers[0]).PublicEncryptionKey = new ByteArray { Data = p.PublicEncryptionKey };
                    ((MpPlayerData)SendToPlayerFromPlayers[1]).PlatformID = p.PlatformUserId;
                    ((MpPlayerData)SendToPlayerFromPlayers[1]).Platform = (byte)p.Platform;
                    ((MpPlayerData)SendToPlayerFromPlayers[1]).ClientVersion = p.ClientVersion;
                    
                    p.PlayerAccessSemaphore.Release();
                    
                    // Send all player avatars and states to just joined player
                    await Task.WhenAny(PacketDispatcher.SendFromPlayerToPlayerAndAwait(p, player, SendToPlayerFromPlayers, DeliveryMethod.ReliableOrdered), Task.Delay(50));
                    //Sends them one by one to avoid server lag
                }
            }
            //End of sending avatars and states to new player

            foreach (IPlayer p in PlayersAtJoin)
            {
                if (p.ConnectionId != player.ConnectionId)
                {
                    await Task.WhenAny(
                    PacketDispatcher.SendFromPlayerToPlayerAndAwait(player, p, new MpPlayerData
                    {
                        PlatformID = player.PlatformUserId!,
                        Platform = (byte)player.Platform,
                        ClientVersion = player.ClientVersion!
                    }, DeliveryMethod.ReliableOrdered),
                    Task.Delay(50));
                }
            }

            await player.PlayerAccessSemaphore.WaitAsync();
            var packet = new MpcTextChatPacket { Text = player.UserName + " Joined, Platform: " + player.Platform.ToString() + " Version: " + player.ClientVersion };
            player.PlayerAccessSemaphore.Release();
            foreach (var p in PlayersAtJoin)
            {
                if (p.CanTextChat)
                {
                    await Task.WhenAny(PacketDispatcher.SendToPlayerAndAwait(p, packet, DeliveryMethod.ReliableOrdered), Task.Delay(50));
                }
            }
        }

        public void DisconnectPlayer(string UserId) //Used my master servers kick player event
        {

            if(_playerRegistry.TryGetPlayer(UserId, out var player))
                PacketDispatcher.SendToPlayer(player, new KickPlayerPacket
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

                PacketDispatcher.SendFromPlayer(player, new PlayerDisconnectedPacket
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
                PacketDispatcher.SendToNearbyPlayers(new SetPlayersPermissionConfigurationPacket
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
                    PacketDispatcher.SendToPlayer(serverOwner, new SetIsStartButtonEnabledPacket
                    {
                        Reason = CannotStartGameReason.NoSongSelected
                    }, DeliveryMethod.ReliableOrdered);
            }


            PacketDispatcher.SendToNearbyPlayers(new MpNodePoseSyncStatePacket
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
                    PacketDispatcher.SendToPlayer(player, new SyncTimePacket()
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
