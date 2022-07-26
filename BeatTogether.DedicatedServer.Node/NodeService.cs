﻿using Autobus;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Interface.Events;
using BeatTogether.DedicatedServer.Interface.Requests;
using BeatTogether.DedicatedServer.Interface.Responses;
using BeatTogether.DedicatedServer.Kernel.Encryption;
using BeatTogether.DedicatedServer.Node.Abstractions;
using BeatTogether.DedicatedServer.Node.Configuration;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Net;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;

namespace BeatTogether.DedicatedServer.Node
{
    public sealed class NodeService : IMatchmakingService
    {
        private readonly NodeConfiguration _configuration;
        private readonly IInstanceFactory _instanceFactory;
        private readonly PacketEncryptionLayer _packetEncryptionLayer;
        private readonly IAutobus _autobus;
        private readonly ILogger _logger = Log.ForContext<NodeService>();

        public NodeService(
            NodeConfiguration configuration,
            IInstanceFactory instanceFactory,
            PacketEncryptionLayer packetEncryptionLayer,
            IAutobus autobus)
        {
            _configuration = configuration;
            _instanceFactory = instanceFactory;
            _packetEncryptionLayer = packetEncryptionLayer;
            _autobus = autobus;
        }

        public async Task<CreateMatchmakingServerResponse> CreateMatchmakingServer(CreateMatchmakingServerRequest request)
        {
            _logger.Debug($"Received request to create matchmaking server. " +
                $"(Secret={request.Secret}, " +
                $"ManagerId={request.ManagerId}, " +
                $"MaxPlayerCount={request.Configuration.MaxPlayerCount}, " +
                $"DiscoveryPolicy={request.Configuration.DiscoveryPolicy}, " +
                $"InvitePolicy={request.Configuration.InvitePolicy}, " +
                $"GameplayServerMode={request.Configuration.GameplayServerMode}, " +
                $"SongSelectionMode={request.Configuration.SongSelectionMode}, " +
                $"GameplayServerControlSettings={request.Configuration.GameplayServerControlSettings})");

            var matchmakingServer = _instanceFactory.CreateInstance(
                request.Secret,
                request.ManagerId,
                request.Configuration,
                request.PermanentManager,
                request.Timeout,
                request.ServerName,
                request.resultScreenTime,
                request.BeatmapStartTime,
                request.PlayersReadyCountdownTime ,
                request.AllowPerPlayerModifiers,
                request.AllowPerPlayerDifficulties,
                request.AllowChroma ,
                request.AllowME ,
                request.AllowNE 
                );
            if (matchmakingServer is null)
                return new CreateMatchmakingServerResponse(CreateMatchmakingServerError.InvalidSecret, string.Empty, Array.Empty<byte>(), Array.Empty<byte>());

            matchmakingServer.PlayerConnectedEvent += HandleUpdatePlayerEvent;
            matchmakingServer.PlayerDisconnectedEvent += HandlePlayerDisconnectEvent;
            matchmakingServer.PlayerCountChangeEvent += HandlePlayerCountChange;
            matchmakingServer.StartEvent += HandleStartEvent;
            matchmakingServer.StopEvent += HandleStopEvent;
            matchmakingServer.StateChangedEvent += HandleStateChangedEvent;
            matchmakingServer.UpdateBeatmapEvent += HandleBeatmapChangedEvent;
            matchmakingServer.UpdateInstanceEvent += HandleConfigChangeEvent;

            await matchmakingServer.Start();
            return new CreateMatchmakingServerResponse(
                CreateMatchmakingServerError.None,
                $"{_configuration.HostName}:{matchmakingServer.Port}",
                _packetEncryptionLayer.Random,
                _packetEncryptionLayer.KeyPair.PublicKey
            );
        }


        #region EventHandlers
        private void HandleStateChangedEvent(string secret, CountdownState countdownState, MultiplayerGameState gameState, GameplayManagerState GameplayState)
        {
            _autobus.Publish(new UpdateStatusEvent(secret, (Interface.Enums.CountdownState)countdownState, (Interface.Enums.MultiplayerGameState)gameState, (Interface.Enums.GameplayState)GameplayState));
        }
        private void HandleBeatmapChangedEvent(string secret, BeatmapIdentifier? beatmap, GameplayModifiers modifiers, bool IsGameplay, DateTime StartTime)
        {
            _autobus.Publish(new SelectedBeatmapEvent(secret, beatmap is not null ? beatmap.LevelId : string.Empty, beatmap is not null ? beatmap.Characteristic : string.Empty, beatmap is not null ? (uint)beatmap.Difficulty : uint.MinValue, IsGameplay, (Interface.Models.GameplayModifiers)modifiers, StartTime));
        }
        private void HandleConfigChangeEvent(IDedicatedInstance inst)
        {
            _autobus.Publish(new UpdateServerEvent(
                inst._configuration.Secret,
                new Interface.Models.GameplayServerConfiguration(
                    inst._configuration.MaxPlayerCount,
                    (Interface.Enums.DiscoveryPolicy)inst._configuration.DiscoveryPolicy,
                    (Interface.Enums.InvitePolicy)inst._configuration.InvitePolicy,
                    (Interface.Enums.GameplayServerMode)inst._configuration.GameplayServerMode,
                    (Interface.Enums.SongSelectionMode)inst._configuration.SongSelectionMode,
                    (Interface.Enums.GameplayServerControlSettings)inst._configuration.GameplayServerControlSettings
                    ),
                inst._configuration.Port,
                inst._configuration.ManagerId,
                inst._configuration.ServerId,
                inst._configuration.ServerName,
                inst._configuration.DestroyInstanceTimeout,
                inst._configuration.SetConstantManagerFromUserId,
                inst._configuration.AllowPerPlayerDifficulties,
                inst._configuration.AllowPerPlayerModifiers,
                inst._configuration.AllowChroma,
                inst._configuration.AllowMappingExtensions,
                inst._configuration.AllowNoodleExtensions,
                inst._configuration.KickPlayersWithoutEntitlementTimeout,
                inst._configuration.CountdownConfig.CountdownTimePlayersReady,
                inst._configuration.CountdownConfig.BeatMapStartCountdownTime,
                inst._configuration.CountdownConfig.ResultsScreenTime));
        }
        private void HandleStartEvent(IDedicatedInstance inst)
        {
            HandleConfigChangeEvent(inst);
        }
        private void HandleStopEvent(IDedicatedInstance inst)
        {
            _autobus.Publish(new MatchmakingServerStoppedEvent(inst._configuration.Secret));//Tells the master server and api server that the server has stopped
        }
        private void HandleUpdatePlayerEvent(IPlayer player)
        {
            _autobus.Publish(new PlayerJoinEvent(player.Secret, player.Endpoint.ToString()!, player.UserId, player.UserName, player.ConnectionId, player.SortIndex, (Interface.Models.AvatarData)player.Avatar));
        }
        private void HandlePlayerDisconnectEvent(IPlayer player, int count)
        {
            _autobus.Publish(new PlayerLeaveServerEvent(player.Secret, player.UserId, ((IPEndPoint)player.Endpoint).ToString(), count));
        }
        private void HandlePlayerCountChange(string Secret, int count)
        {
            _autobus.Publish(new PlayerLeaveServerEvent(Secret,string.Empty, string.Empty, count));
        }
        #endregion
    }
}