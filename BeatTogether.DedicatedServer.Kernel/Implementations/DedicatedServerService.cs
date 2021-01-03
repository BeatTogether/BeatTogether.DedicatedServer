using System.Net;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Abstractions.Providers;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Requests;
using BeatTogether.DedicatedServer.Messaging.Responses;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.Implementations
{
    public class DedicatedServerService : IDedicatedServerService
    {
        private readonly DedicatedServerConfiguration _configuration;
        private readonly IRelayServerFactory _relayServerFactory;
        private readonly ILogger _logger; 

        public DedicatedServerService(
            DedicatedServerConfiguration configuration,
            IRelayServerFactory relayServerFactory)
        {
            _configuration = configuration;
            _relayServerFactory = relayServerFactory;
            _logger = Log.ForContext<DedicatedServerService>();
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
                return Task.FromResult(new GetAvailableRelayServerResponse()
                {
                    Error = GetAvailableRelayServerResponse.ErrorCode.NoAvailableRelayServers
                });
            }
            relayServer.Start();
            return Task.FromResult(new GetAvailableRelayServerResponse()
            {
                RemoteEndPoint = $"{_configuration.HostName}:{relayServer.Endpoint.Port}"
            });
        }
    }
}
