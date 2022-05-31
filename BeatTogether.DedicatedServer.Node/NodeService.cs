using Autobus;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Interface.Events;
using BeatTogether.DedicatedServer.Interface.Requests;
using BeatTogether.DedicatedServer.Interface.Responses;
using BeatTogether.DedicatedServer.Kernel.Encryption;
using BeatTogether.DedicatedServer.Node.Abstractions;
using BeatTogether.DedicatedServer.Node.Configuration;
using Serilog;
using System;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Node
{
    public sealed class NodeService : IMatchmakingService
    {
        private readonly NodeConfiguration _configuration;
        private readonly IInstanceFactory _instanceFactory;
        private readonly IInstanceRegistry _instanceRegistry;
        private readonly PacketEncryptionLayer _packetEncryptionLayer;
        private readonly IAutobus _autobus;
        private readonly ILogger _logger = Log.ForContext<NodeService>();

        public NodeService(
            NodeConfiguration configuration,
            IInstanceFactory instanceFactory,
            IInstanceRegistry instanceRegistry,
            PacketEncryptionLayer packetEncryptionLayer,
            IAutobus autobus)
        {
            _configuration = configuration;
            _instanceFactory = instanceFactory;
            _instanceRegistry = instanceRegistry;
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
                request.permanentManager,
                request.Timeout
            );
            if (matchmakingServer is null) // TODO: can also be no available slots
                return new CreateMatchmakingServerResponse(CreateMatchmakingServerError.InvalidSecret, string.Empty, Array.Empty<byte>(), Array.Empty<byte>());

            await matchmakingServer.Start();
            _autobus.Publish(new MatchmakingServerStartedEvent(request.Secret, request.ManagerId, request.Configuration));//Tells the master server to add a server
            matchmakingServer.StopEvent += () => _autobus.Publish(new MatchmakingServerStoppedEvent(request.Secret));//Tells the master server when the newly added server has stopped

            return new CreateMatchmakingServerResponse(
                CreateMatchmakingServerError.None,
                $"{_configuration.HostName}:{matchmakingServer.Port}",
                _packetEncryptionLayer.Random,
                _packetEncryptionLayer.KeyPair.PublicKey
            );
        }
        //TODO add code to make custom dedicated instances here from an autobus packet(is literally the code above)-Done

        public async Task<StopMatchmakingServerResponse> StopMatchmakingServer(StopMatchmakingServerRequest request)
        {
            if (_instanceRegistry.TryGetInstance(request.Secret, out var instance))
            {
                await instance.Stop();
                _autobus.Publish(new MatchmakingServerStoppedEvent(request.Secret));
                return new StopMatchmakingServerResponse(true, true);
            }
            return new StopMatchmakingServerResponse(false, false);
        }

        public Task<PublicMatchmakingServerListResponse> GetPublicMatchmakingServerList(GetPublicMatchmakingServerListRequest request)
        {
            return Task.FromResult(new PublicMatchmakingServerListResponse(_instanceRegistry.ListPublicInstanceSecrets()));
        }
        public Task<ServerCountResponse> GetServerCount(GetMatchmakingServerCountRequest request)
        {
            return Task.FromResult(new ServerCountResponse(_instanceRegistry.GetServerCount()));
        }
        public Task<PublicServerCountResponse> GetPublicServerCount(GetPublicMatchmakingServerCountRequest request)
        {
            return Task.FromResult(new PublicServerCountResponse(_instanceRegistry.GetPublicServerCount()));
        }
    }
}
