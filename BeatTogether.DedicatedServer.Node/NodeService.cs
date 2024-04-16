using System;
using System.Net;
using System.Threading.Tasks;
using Autobus;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Interface.Events;
using BeatTogether.DedicatedServer.Interface.Requests;
using BeatTogether.DedicatedServer.Interface.Responses;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Node.Abstractions;
using BeatTogether.DedicatedServer.Node.Configuration;
using Serilog;

namespace BeatTogether.DedicatedServer.Node
{
    public sealed class NodeService : IMatchmakingService
    {
        private readonly NodeConfiguration _configuration;
        private readonly IInstanceFactory _instanceFactory;
        private readonly IAutobus _autobus;
        private readonly ILogger _logger = Log.ForContext<NodeService>();

        public NodeService(
            NodeConfiguration configuration,
            IInstanceFactory instanceFactory,
            IAutobus autobus)
        {
            _configuration = configuration;
            _instanceFactory = instanceFactory;
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
                (long)request.ResultScreenTime,
                ((long)request.BeatmapStartTime) * 1000,
                ((long)request.PlayersReadyCountdownTime) * 1000,
                request.AllowPerPlayerModifiers,
                request.AllowPerPlayerDifficulties,
                request.AllowChroma,
                request.AllowME,
                request.AllowNE
                );
            if (matchmakingServer is null)
                return new CreateMatchmakingServerResponse(CreateMatchmakingServerError.NoAvailableSlots, string.Empty, Array.Empty<byte>(), Array.Empty<byte>());

            matchmakingServer.PlayerConnectedEvent += HandleUpdatePlayerEvent;
            matchmakingServer.PlayerDisconnectedEvent += HandlePlayerDisconnectEvent;
            matchmakingServer.PlayerDisconnectBeforeJoining += HandlePlayerLeaveBeforeJoining;
            matchmakingServer.StopEvent += HandleStopEvent;
            matchmakingServer.GameIsInLobby += HandleGameInLobbyEvent;
            matchmakingServer.UpdateInstanceEvent += HandleConfigChangeEvent;

            await matchmakingServer.Start();
            return new CreateMatchmakingServerResponse(
                CreateMatchmakingServerError.None,
                $"{_configuration.HostName}:{matchmakingServer.Port}",
                 Array.Empty<byte>(),
                 Array.Empty<byte>()
            );
        }


        #region EventHandlers

        private void HandleGameInLobbyEvent(string secret, bool state)
        {
            _autobus.Publish(new ServerInGameplayEvent(secret, !state, string.Empty));
        }

        private void HandleConfigChangeEvent(IDedicatedInstance inst)
        {
            _autobus.Publish(new UpdateInstanceConfigEvent(
                inst._configuration.Secret,
                inst._configuration.ServerName,
                new Interface.Models.GameplayServerConfiguration(
                    inst._configuration.MaxPlayerCount,
                    (Interface.Enums.DiscoveryPolicy)inst._configuration.DiscoveryPolicy,
                    (Interface.Enums.InvitePolicy)inst._configuration.InvitePolicy,
                    (Interface.Enums.GameplayServerMode)inst._configuration.GameplayServerMode,
                    (Interface.Enums.SongSelectionMode)inst._configuration.SongSelectionMode,
                    (Interface.Enums.GameplayServerControlSettings)inst._configuration.GameplayServerControlSettings
                    )
                ));
        }

        private void HandleStopEvent(IDedicatedInstance inst)
        {
            _autobus.Publish(new MatchmakingServerStoppedEvent(inst._configuration.Secret));
        }
        private void HandleUpdatePlayerEvent(IPlayer player)
        {
            _autobus.Publish(new PlayerJoinEvent(player.Instance._configuration.Secret, ((IPEndPoint)player.Endpoint).ToString(), player.UserId));
        }
        private void HandlePlayerDisconnectEvent(IPlayer player)
        {
            _autobus.Publish(new PlayerLeaveServerEvent(player.Instance._configuration.Secret, player.UserId, ((IPEndPoint)player.Endpoint).ToString()));
        }
        private void HandlePlayerLeaveBeforeJoining(string Secret, EndPoint endPoint, string[] Players)
        {
            _autobus.Publish(new UpdatePlayersEvent(Secret, Players));
        }
        #endregion
    }
}