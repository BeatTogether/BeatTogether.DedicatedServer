using Autobus;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Interface.Events;
using BeatTogether.DedicatedServer.Interface.Requests;
using BeatTogether.DedicatedServer.Interface.Responses;
using BeatTogether.DedicatedServer.Kernel.Encryption;
using BeatTogether.DedicatedServer.Node.Abstractions;
using BeatTogether.DedicatedServer.Node.Configuration;
using System;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Node
{
    public sealed class NodeService : IMatchmakingService
    {
        private readonly NodeConfiguration _configuration;
        private readonly IInstanceFactory _instanceFactory;
        private readonly PacketEncryptionLayer _packetEncryptionLayer;
        private readonly IAutobus _autobus;

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
            var matchmakingServer = _instanceFactory.CreateInstance(
                request.Secret,
                request.ManagerId,
                request.Configuration
            );
            if (matchmakingServer is null) // TODO: can also be no available slots
                return new CreateMatchmakingServerResponse(CreateMatchmakingServerError.InvalidSecret, string.Empty, Array.Empty<byte>(), Array.Empty<byte>());

            await matchmakingServer.Start();
            _autobus.Publish(new MatchmakingServerStartedEvent(request.Secret, request.ManagerId, request.Configuration));
            _ = matchmakingServer.Complete().ContinueWith(_ =>
            {
                _autobus.Publish(new MatchmakingServerStoppedEvent(request.Secret));
            });

            return new CreateMatchmakingServerResponse(
                CreateMatchmakingServerError.None,
                $"{_configuration.HostName}:{matchmakingServer.Port}",
                _packetEncryptionLayer.Random,
                _packetEncryptionLayer.KeyPair.PublicKey
            );
        }
    }
}
