using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Kernel.ENet;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Kernel.Extensions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MPChatPackets;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Util;
using Serilog;
using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class DedicatedInstance : ENetServer, IDedicatedInstance
    {
        // Milliseconds instance will wait for a player to connect.
        public const int WaitForPlayerTimeLimit = 10000;

        // Milliseconds between sync time updates
        public const int SyncTimeDelay = 5000;

        public InstanceConfiguration _configuration { get; private set; }
        public int Port => _configuration.Port;
        public bool IsRunning => IsAlive;
        public long RunTime => (DateTime.UtcNow.Ticks - _startTime) / 10000L;
        public float NoPlayersTime { get; private set; } = -1; //tracks the instance time once there are 0 players in the lobby
        public MultiplayerGameState State { get; private set; } = MultiplayerGameState.Lobby;

        public event Action<IDedicatedInstance> StopEvent = null!;
        public event Action<IPlayer> PlayerConnectedEvent = null!;
        public event Action<IPlayer> PlayerDisconnectedEvent = null!;
        public event Action<string, EndPoint, string[]> PlayerDisconnectBeforeJoining = null!;
        public event Action<string, bool> GameIsInLobby = null!;
        public event Action<IDedicatedInstance> UpdateInstanceEvent = null!;

        private readonly IPlayerRegistry _playerRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPacketDispatcher PacketDispatcher;
        private readonly PacketSource ConnectedMessageSource;


        private byte _connectionIdCount = 0;
        private int _lastSortIndex = -1;
        private readonly Queue<byte> _releasedConnectionIds = new();
        private readonly Queue<int> _releasedSortIndices = new();
        private readonly ILogger _logger = Log.ForContext<DedicatedInstance>();

        private long _startTime;
        private CancellationTokenSource? _waitForPlayerCts = null;
        private CancellationTokenSource? _stopServerCts;


        public DedicatedInstance(
            InstanceConfiguration configuration,
            IPlayerRegistry playerRegistry,
            IPacketDispatcher packetDispatcher,
            PacketSource connectedMessageSource,
            IServiceProvider serviceProvider)
            : base (configuration.Port)
        {
            _configuration = configuration;
            _playerRegistry = playerRegistry;
            _serviceProvider = serviceProvider;
            PacketDispatcher = packetDispatcher;
            ConnectedMessageSource = connectedMessageSource;
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

        public async Task Start(CancellationToken cancellationToken = default)
        {
            if (IsRunning)
                return;

            //PacketDispatcher = _serviceProvider.GetRequiredService<IPacketDispatcher>();
            //ConnectedMessageSource = _serviceProvider.GetRequiredService<PacketSource>();
            
            _startTime = DateTime.UtcNow.Ticks;

            _logger.Information(
                "Starting dedicated server " +
                $"Endpoint={EndPoint}," +
                $"ServerName='{_configuration.ServerName}', " +
                $"Secret='{_configuration.Secret}', " +
                $"ManagerId='{_configuration.ServerOwnerId}', " +
                $"MaxPlayerCount={_configuration.MaxPlayerCount}, " +
                $"DiscoveryPolicy={_configuration.DiscoveryPolicy}, " +
                $"InvitePolicy={_configuration.InvitePolicy}, " +
                $"GameplayServerMode={_configuration.GameplayServerMode}, " +
                $"SongSelectionMode={_configuration.SongSelectionMode}, " +
                $"GameplayServerControlSettings={_configuration.GameplayServerControlSettings})"
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

            await base.Start();
            
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
            }, IgnoranceChannelTypes.Reliable);

            KickAllPeers();

            _stopServerCts!.Cancel();
            _waitForPlayerCts!.Cancel();
            
            StopEvent?.Invoke(this);

            await base.Stop();
        }

        public int GetNextSortIndex()
        {
            if (_releasedSortIndices.TryDequeue(out var sortIndex))
                return sortIndex;

            _lastSortIndex++;
            if (_lastSortIndex > _configuration.MaxPlayerCount)
            {
                return 0;
            }
            return _lastSortIndex;
        }

        public void ReleaseSortIndex(int sortIndex)
        {
            _releasedSortIndices.Enqueue(sortIndex);
        }
                    
        public byte GetNextConnectionId() //ID 0 is server, ID 127 means send to all players
        {
            if (_releasedConnectionIds.TryDequeue(out var connectionId))
                return connectionId;

            _connectionIdCount++;
            if (_connectionIdCount == 127)
                _connectionIdCount++;
            if (_connectionIdCount > (byte.MaxValue - 5)) //Currently not implimented to use ID's over 126 client side
                return 255; //Give them an unusedID so they dont conflict with anyone
            return _connectionIdCount;
        }

        public void ReleaseConnectionId(byte connectionId)
        {

            _releasedConnectionIds.Enqueue(connectionId);
        }

        public void SetState(MultiplayerGameState state)
        {
            State = state;
            PacketDispatcher.SendToNearbyPlayers(new SetMultiplayerGameStatePacket()
            {
                State = state
            }, IgnoranceChannelTypes.Reliable);
            GameIsInLobby?.Invoke(_configuration.Secret, state == MultiplayerGameState.Lobby);
        }

        #endregion

        #region EnetCompat

        public override IPlayer? TryAcceptConnection(IPEndPoint endPoint, ref SpanBuffer Data)
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
                $"PlayerSessionId='{connectionRequestData.PlayerSessionId}', " +
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
            if (_playerRegistry.GetPlayerCount() >= _configuration.MaxPlayerCount)
            {
                _logger.Warning("Master server sent a player to a full server");
                PlayerNoJoin = true;
                goto EndOfTryAccept;
            }
            if (connectionRequestData.UserName == "IGGAMES" || connectionRequestData.UserName == "IGGGAMES")
            {
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
                $"PlayerSessionId='{player.PlayerSessionId}', " +
                $"UserId='{player.UserId}', " +
                $"UserName='{player.UserName}', " +
                $"SortIndex={player.SortIndex})."
            );

            if (_waitForPlayerCts != null)
                _waitForPlayerCts.Cancel();

            if (GetPlayerRegistry().RemoveExtraPlayerSessionData(connectionRequestData.PlayerSessionId, out var ClientVer, out var Platform, out var PlayerPlatformUserId))
            {
                player.ClientVersion = ClientVer;
                player.Platform = (Enums.Platform)Platform;
                player.PlatformUserId = PlayerPlatformUserId;
            }


            PacketDispatcher.SendExcludingPlayer(player, new INetSerializable[]{
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
                    fullStateUpdateFrequency = 100L,
                    deltaUpdateFrequency = _playerRegistry.GetMillisBetweenSyncStatePackets()
                },
            }, IgnoranceChannelTypes.Reliable);

            return player;

            EndOfTryAccept:
            if (PlayerNoJoin)
            {
                GetPlayerRegistry().RemoveExtraPlayerSessionData(connectionRequestData.PlayerSessionId, out _, out _, out _);
                string[] Players = _playerRegistry.Players.Select(p => p.UserId).ToArray();
                PlayerDisconnectBeforeJoining?.Invoke(_configuration.Secret, endPoint, Players);
                return null;
            }
            return null;
        }

        public override void OnReceive(EndPoint remoteEndPoint, ref SpanBuffer reader, IgnoranceChannelTypes method)
        {
            ConnectedMessageSource.OnReceive(remoteEndPoint, ref reader, method);
        }

        #endregion

        public override void OnConnect(EndPoint endPoint)
        {
            _logger.Information($"Endpoint connected (RemoteEndPoint='{endPoint}')");

            if (!_playerRegistry.TryGetPlayer(endPoint, out var player))
            {
                _logger.Warning(
                    "Failed to retrieve player " +
                    $"(RemoteEndPoint='{endPoint}')."
                );
                //Disconnect(endPoint);
                //TODO impliment for enet, enet peers cannot be located at this point as a peer ID is required to kick them
                return;
            }
            PlayerConnectedEvent?.Invoke(player);
            //The player has already been sent to every other player in the should connect function.

            //Start of sending other players to player
            //Now send all players already in the game to the new player
            IPlayer[] PlayersAtJoin = _playerRegistry.Players;

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
                        fullStateUpdateFrequency = 100L,
                        deltaUpdateFrequency = _playerRegistry.GetMillisBetweenSyncStatePackets()
                    },
               };

            PacketDispatcher.SendToPlayer(player, Player_ConnectPacket, IgnoranceChannelTypes.Reliable);

            List<INetSerializable> MakeBigPacketToSendToPlayer = new();
            foreach (IPlayer p in PlayersAtJoin)
            {
                if(p.ConnectionId != player.ConnectionId)
                {
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
                }
            }
            foreach (var SubPacket in MakeBigPacketToSendToPlayer.Chunk(20))
            {
                PacketDispatcher.SendToPlayer(player, SubPacket.ToArray(), IgnoranceChannelTypes.Reliable);
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
                }, IgnoranceChannelTypes.Reliable);
            }

            if (player.Platform == Enums.Platform.Test) //If the player is a bot, send permissions. Normal players request this in a packet when they join
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
                }, IgnoranceChannelTypes.Reliable);
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

                    ((PlayerIdentityPacket)SendToPlayerFromPlayers[0]).PlayerState = p.State;
                    ((PlayerIdentityPacket)SendToPlayerFromPlayers[0]).PlayerAvatar = p.Avatar;
                    ((PlayerIdentityPacket)SendToPlayerFromPlayers[0]).Random = new ByteArray { Data = p.Random };
                    ((PlayerIdentityPacket)SendToPlayerFromPlayers[0]).PublicEncryptionKey = new ByteArray { Data = p.PublicEncryptionKey };
                    ((MpPlayerData)SendToPlayerFromPlayers[1]).PlatformID = p.PlatformUserId;
                    ((MpPlayerData)SendToPlayerFromPlayers[1]).Platform = p.Platform.Convert();
                    ((MpPlayerData)SendToPlayerFromPlayers[1]).ClientVersion = p.ClientVersion;
                    
                    
                    // Send all player avatars and states to just joined player
                   PacketDispatcher.SendFromPlayerToPlayer(p, player, SendToPlayerFromPlayers, IgnoranceChannelTypes.Reliable);
                    //Sends them one by one to avoid server lag
                }
            }
            //End of sending avatars and states to new player

            foreach (IPlayer p in PlayersAtJoin)
            {
                if (p.ConnectionId != player.ConnectionId)
                {
                    PacketDispatcher.SendFromPlayerToPlayer(player, p, new MpPlayerData
                    {
                        PlatformID = player.PlatformUserId!,
                        Platform = player.Platform.Convert(),
                        ClientVersion = player.ClientVersion!
                    }, IgnoranceChannelTypes.Reliable);
                }
            }

            var packet = new MpcTextChatPacket { Text = player.UserName + " Joined, Platform: " + player.Platform.ToString() + " Version: " + player.ClientVersion };
            foreach (var p in PlayersAtJoin)
            {
                if (p.CanTextChat)
                {
                    PacketDispatcher.SendToPlayer(p, packet, IgnoranceChannelTypes.Reliable);
                }
            }
        }

        public void DisconnectPlayer(string UserId) //Used by master servers kick player event
        {

            if(_playerRegistry.TryGetPlayer(UserId, out var player))
                PacketDispatcher.SendToPlayer(player, new KickPlayerPacket
                {
                    DisconnectedReason = DisconnectedReason.Kicked
                }, IgnoranceChannelTypes.Reliable);
        }

        public override void OnDisconnect(EndPoint endPoint)
        {
            _logger.Information(
                "Endpoint disconnected " +
                $"(RemoteEndPoint='{endPoint}')."
            );


            if (_playerRegistry.TryGetPlayer(endPoint, out var player))
            {
                //Sends to all players that they have disconnected

                PacketDispatcher.SendFromPlayer(player, new PlayerDisconnectedPacket
                {
                    DisconnectedReason = DisconnectedReason.ClientConnectionClosed
                }, IgnoranceChannelTypes.Reliable);

                if (_configuration.ServerOwnerId == player.UserId)
                    _configuration.ServerOwnerId = "";

                _playerRegistry.RemovePlayer(player);
                ReleaseSortIndex(player.SortIndex);
                ReleaseConnectionId(player.ConnectionId);

                PlayerDisconnectedEvent?.Invoke(player);
            }

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
                }, IgnoranceChannelTypes.Reliable);

                // Disable start button if they are server owner without selected song
                if (serverOwner.BeatmapIdentifier == null)
                    PacketDispatcher.SendToPlayer(serverOwner, new SetIsStartButtonEnabledPacket
                    {
                        Reason = CannotStartGameReason.NoSongSelected
                    }, IgnoranceChannelTypes.Reliable);
            }


            PacketDispatcher.SendToNearbyPlayers(new MpNodePoseSyncStatePacket
            {
                fullStateUpdateFrequency = 100L,
                deltaUpdateFrequency = _playerRegistry.GetMillisBetweenSyncStatePackets()
            }, IgnoranceChannelTypes.Reliable);

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
                    }, IgnoranceChannelTypes.Reliable);
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
