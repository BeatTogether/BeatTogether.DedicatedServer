using System;
using System.Threading.Tasks;
using AutoMapper;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Interface.Requests;
using BeatTogether.DedicatedServer.Interface.Responses;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class MatchmakingService : IMatchmakingService
    {
        private readonly ServerConfiguration _serverConfiguration;
        private readonly IMapper _mapper;
        private readonly IMatchmakingServerFactory _matchmakingServerFactory;
        private readonly PacketEncryptionLayer _packetEncryptionLayer;

        public MatchmakingService(
            ServerConfiguration serverConfiguration,
            IMapper mapper,
            IMatchmakingServerFactory matchmakingServerFactory,
            PacketEncryptionLayer packetEncryptionLayer)
        {
            _serverConfiguration = serverConfiguration;
            _mapper = mapper;
            _matchmakingServerFactory = matchmakingServerFactory;
            _packetEncryptionLayer = packetEncryptionLayer;
        }

        public async Task<CreateMatchmakingServerResponse> CreateMatchmakingServer(CreateMatchmakingServerRequest request)
        {
            var matchmakingServer = _matchmakingServerFactory.CreateMatchmakingServer(
                request.Secret,
                request.ManagerId,
                _mapper.Map<Models.GameplayServerConfiguration>(request.Configuration)
            );
            if (matchmakingServer is null)
                return new CreateMatchmakingServerResponse(CreateMatchmakingServerError.InvalidSecret, string.Empty, Array.Empty<byte>(), Array.Empty<byte>());

            await matchmakingServer.Start();
            if (!matchmakingServer.IsRunning)
                return new CreateMatchmakingServerResponse(CreateMatchmakingServerError.NoAvailableSlots, string.Empty, Array.Empty<byte>(), Array.Empty<byte>());

            return new CreateMatchmakingServerResponse(
                CreateMatchmakingServerError.None,
                $"{_serverConfiguration.HostName}:{matchmakingServer.Port}",
                _packetEncryptionLayer.Random,
                _packetEncryptionLayer.KeyPair.PublicKey
            );
        }
    }
}
