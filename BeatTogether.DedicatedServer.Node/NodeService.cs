using System.Threading.Tasks;
using BeatTogether.Core.Abstractions;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Interface.Requests;
using BeatTogether.DedicatedServer.Interface.Responses;
using BeatTogether.DedicatedServer.Node.Models;
using Serilog;

namespace BeatTogether.DedicatedServer.Node
{
    public sealed class NodeMatchmakingService : IMatchmakingService
    {
        private readonly ILayer2 _Layer2;
        private readonly ILogger _logger = Log.ForContext<NodeMatchmakingService>();

        public NodeMatchmakingService(
            ILayer2 layer2)
        {
            _Layer2 = layer2;
        }

        public async Task<CreateMatchmakingServerResponse> CreateMatchmakingServer(CreateMatchmakingServerRequest request)
        {
            _logger.Debug($"Received request to create matchmaking server from node messaging. " +
                $"(Secret={request.Server.Secret}, " +
                $"Code={request.Server.Code}, " +
                $"ManagerId={request.Server.ManagerId}, " +
                $"MaxPlayerCount={request.Server.GameplayServerConfiguration.MaxPlayerCount}, " +
                $"DiscoveryPolicy={request.Server.GameplayServerConfiguration.DiscoveryPolicy}, " +
                $"InvitePolicy={request.Server.GameplayServerConfiguration.InvitePolicy}, " +
                $"GameplayServerMode={request.Server.GameplayServerConfiguration.GameplayServerMode}, " +
                $"SongSelectionMode={request.Server.GameplayServerConfiguration.SongSelectionMode}, " +
                $"GameplayServerControlSettings={request.Server.GameplayServerConfiguration.GameplayServerControlSettings})");

            IServerInstance serverInstance = new ServerFromMessage(request.Server);


            var result = await _Layer2.CreateInstance(serverInstance);

            if(!result)
                return new CreateMatchmakingServerResponse(CreateMatchmakingServerError.NoAvailableSlots, string.Empty);

            return new CreateMatchmakingServerResponse(
                CreateMatchmakingServerError.None,
                $"{serverInstance.InstanceEndPoint}"
            );
        }
    }
}