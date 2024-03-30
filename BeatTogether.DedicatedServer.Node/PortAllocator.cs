using BeatTogether.DedicatedServer.Node.Abstractions;
using BeatTogether.DedicatedServer.Node.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace BeatTogether.DedicatedServer.Node
{
    public sealed class PortAllocator : IPortAllocator
    {
        private readonly NodeConfiguration _configuration;

        private readonly HashSet<int> _acquiredPorts = new();
        private readonly HashSet<int> _releasedPorts = new();

        private int _lastPort;

        public PortAllocator(
            NodeConfiguration configuration)
        {
            _configuration = configuration;

            _lastPort = configuration.BasePort;
        }

        public int? AcquirePort()
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

        public bool ReleasePort(int port)
        {

            if (!_acquiredPorts.Remove(port))
                return false;
            _releasedPorts.Add(port);
            return true;
        }
    }
}
