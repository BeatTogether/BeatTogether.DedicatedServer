using BeatTogether.DedicatedServer.Instancing.Abstractions;
using BeatTogether.DedicatedServer.Instancing.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace BeatTogether.DedicatedServer.Instancing
{
    public sealed class PortAllocator : IPortAllocator
    {
        private readonly InstancingConfiguration _configuration;

        private readonly object _lock = new();

        private readonly HashSet<int> _acquiredPorts = new();
        private readonly HashSet<int> _releasedPorts = new();

        private int _lastPort;

        public PortAllocator(
            InstancingConfiguration configuration)
        {
            _configuration = configuration;

            _lastPort = configuration.BasePort;
        }

        public int? AcquirePort()
        {
            lock (_lock)
            {
                if (_acquiredPorts.Count >= _configuration.MaximumSlots)
                    return null;
                int port;
                if (_releasedPorts.Any())
                {
                    port = _releasedPorts.First();
                    _releasedPorts.Remove(port);
                }
                else
                    port = ++_lastPort;
                _acquiredPorts.Add(port);
                return port;
            }

        }

        public bool ReleasePort(int port)
        {
            lock (_lock)
            {
                if (!_acquiredPorts.Remove(port))
                    return false;
                _releasedPorts.Add(port);
                return true;
            }

        }
    }
}
