using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;

namespace BeatTogether.DedicatedServer.Kernel.Implementations
{
    public class DedicatedServerPortAllocator : IDedicatedServerPortAllocator
    {
        private readonly DedicatedServerConfiguration _configuration;

        private readonly object _lock;

        private readonly HashSet<int> _acquiredRelayServerPorts;
        private readonly HashSet<int> _releasedRelayServerPorts;

        private int _lastPort;

        public DedicatedServerPortAllocator(DedicatedServerConfiguration configuration)
        {
            _configuration = configuration;

            _lock = new();

            _acquiredRelayServerPorts = new();
            _releasedRelayServerPorts = new();

            _lastPort = configuration.BasePort;
        }

        public int? AcquireRelayServerPort()
        {
            lock (_lock)
            {
                if (_acquiredRelayServerPorts.Count >= _configuration.RelayServers.MaximumSlots)
                    return null;
                int port;
                if (_releasedRelayServerPorts.Any())
                {
                    port = _releasedRelayServerPorts.First();
                    _releasedRelayServerPorts.Remove(port);
                }
                else
                    port = ++_lastPort;
                _acquiredRelayServerPorts.Add(port);
                return port;
            }
        }

        public bool ReleaseRelayServerPort(int port)
        {
            lock (_lock)
            {
                if (!_acquiredRelayServerPorts.Remove(port))
                    return false;
                _releasedRelayServerPorts.Add(port);
                return true;
            }
        }
    }
}
