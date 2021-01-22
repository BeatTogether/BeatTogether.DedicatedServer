using System.Net;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Kernel.Abstractions.Providers;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Requests;
using BeatTogether.DedicatedServer.Messaging.Responses;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.Implementations
{
    public class RelayServerService : IRelayServerService
    {
        private readonly RelayServerConfiguration _configuration;
        private readonly IRelayServerFactory _relayServerFactory;
        private readonly ILogger _logger;

        public RelayServerService(
            RelayServerConfiguration configuration,
            IRelayServerFactory relayServerFactory)
        {
            _configuration = configuration;
            _relayServerFactory = relayServerFactory;
            _logger = Log.ForContext<RelayServerService>();
        }

        public Task<GetAvailableRelayServerResponse> GetAvailableRelayServer(GetAvailableRelayServerRequest request)
        {
            var relayServer = _relayServerFactory.GetRelayServer(
                IPEndPoint.Parse(request.SourceEndPoint),
                IPEndPoint.Parse(request.TargetEndPoint)
            );
            if (relayServer == null)
            {
                _logger.Warning(
                    "No available slots for relay server " +
                    $"(SourceEndPoint='{request.SourceEndPoint}', " +
                    $"TargetEndPoint='{request.TargetEndPoint}')."
                );
                return Task.FromResult(
                    new GetAvailableRelayServerResponse(GetAvailableRelayServerError.NoAvailableRelayServers)
                );
            }
            if (!relayServer.Start())
            {
                _logger.Warning(
                    "Failed to start UDP relay server for "+
                    $"(SourceEndPoint='{request.SourceEndPoint}', " +
                    $"TargetEndPoint='{request.TargetEndPoint}')."
                );
                return Task.FromResult(
                    new GetAvailableRelayServerResponse(GetAvailableRelayServerError.FailedToStartRelayServer)
                );
            }
            return Task.FromResult(new GetAvailableRelayServerResponse(
                GetAvailableRelayServerError.None,
                $"{_configuration.HostName}:{relayServer.Endpoint.Port}")
            );
        }
    }
}
