using System.Collections.Generic;
using System.Linq;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;

namespace BeatTogether.DedicatedServer.Kernel
{
    public sealed class PortAllocator : IPortAllocator
    {
        private readonly ServerConfiguration _serverConfiguration;

        private readonly object _lock = new();

        private readonly HashSet<int> _acquiredPorts = new();
        private readonly HashSet<int> _releasedPorts = new();

        private int _lastPort;

        public PortAllocator(ServerConfiguration serverConfiguration)
        {
            _serverConfiguration = serverConfiguration;

            _lastPort = serverConfiguration.BasePort;
        }

        public int? AcquirePort()
        {
            lock (_lock)
            {
                if (_acquiredPorts.Count >= _serverConfiguration.MaximumSlots)
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
