using System.Net;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Abstractions.Providers;
using BeatTogether.DedicatedServer.Kernel.Configuration;

namespace BeatTogether.DedicatedServer.Kernel.Implementations.Factories
{
    public class RelayServerFactory : IRelayServerFactory
    {
        private readonly RelayServerConfiguration _configuration;
        private readonly IDedicatedServerPortAllocator _dedicatedServerPortAllocator;

        public RelayServerFactory(
            RelayServerConfiguration configuration,
            IDedicatedServerPortAllocator dedicatedServerPortAllocator)
        {
            _configuration = configuration;
            _dedicatedServerPortAllocator = dedicatedServerPortAllocator;
        }

        public RelayServer? GetRelayServer(IPEndPoint sourceEndPoint, IPEndPoint targetEndPoint)
        {
            var port = _dedicatedServerPortAllocator.AcquireRelayServerPort();
            if (!port.HasValue)
                return null;
            return new RelayServer(
                _dedicatedServerPortAllocator,
                new IPEndPoint(IPAddress.Any, port.Value),
                sourceEndPoint,
                targetEndPoint,
                _configuration.InactivityTimeout
            );
        }
    }
}
