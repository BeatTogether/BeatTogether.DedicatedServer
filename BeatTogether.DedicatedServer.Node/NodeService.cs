using Autobus;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Interface.Events;
using BeatTogether.DedicatedServer.Interface.Requests;
using BeatTogether.DedicatedServer.Interface.Responses;
using BeatTogether.DedicatedServer.Kernel.Encryption;
using BeatTogether.DedicatedServer.Node.Abstractions;
using BeatTogether.DedicatedServer.Node.Configuration;
using BeatTogether.DedicatedServer.Interface.Models;
using BeatTogether.DedicatedServer.Interface.Enums;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using Serilog;
using System;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using System.Net;

namespace BeatTogether.DedicatedServer.Node
{
    public sealed class NodeService : IMatchmakingService
    {
        private readonly NodeConfiguration _configuration;
        private readonly IInstanceFactory _instanceFactory;
        private readonly PacketEncryptionLayer _packetEncryptionLayer;
        private readonly IAutobus _autobus;
        private readonly ILogger _logger = Log.ForContext<NodeService>();
        private readonly IBeatmapRepository _beatmapRepository;

        public NodeService(
            NodeConfiguration configuration,
            IInstanceFactory instanceFactory,
            PacketEncryptionLayer packetEncryptionLayer,
            IAutobus autobus,
            IBeatmapRepository beatmapRepository)
        {
            _configuration = configuration;
            _instanceFactory = instanceFactory;
            _packetEncryptionLayer = packetEncryptionLayer;
            _autobus = autobus;
            _beatmapRepository = beatmapRepository;
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
                request.AllowPerPlayerBeatmaps ,
                request.AllowChroma ,
                request.AllowME ,
                request.AllowNE 
                );
            if (matchmakingServer is null)
                return new CreateMatchmakingServerResponse(CreateMatchmakingServerError.InvalidSecret, string.Empty, Array.Empty<byte>(), Array.Empty<byte>());

            await matchmakingServer.Start();
            matchmakingServer.StopEvent += () => _autobus.Publish(new MatchmakingServerStoppedEvent(request.Secret));//Tells the master server when the newly added server has stopped
            matchmakingServer.PlayerDisconnectedEvent +=  HandlePlayerDisconnectEvent;
            return new CreateMatchmakingServerResponse(
                CreateMatchmakingServerError.None,
                $"{_configuration.HostName}:{matchmakingServer.Port}",
                _packetEncryptionLayer.Random,
                _packetEncryptionLayer.KeyPair.PublicKey
            );
        }

        private void HandlePlayerDisconnectEvent(IPlayer player, int count)
        {
            _autobus.Publish(new PlayerLeaveServerEvent(player.Secret, ((IPEndPoint)player.Endpoint).ToString(), count));
        }
    }
}