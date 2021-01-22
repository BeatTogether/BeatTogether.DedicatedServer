using System.Collections.Generic;
using System.Linq;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;

namespace BeatTogether.DedicatedServer.Kernel.Implementations
{
    public class DedicatedServerPortAllocator : IDedicatedServerPortAllocator
    {
        private readonly RelayServerConfiguration _relayServerConfiguration;

        private readonly object _lock;

        private readonly HashSet<int> _acquiredRelayServerPorts;
        private readonly HashSet<int> _releasedRelayServerPorts;

        private int _lastPort;

        public DedicatedServerPortAllocator(RelayServerConfiguration relayServerConfiguration)
        {
            _relayServerConfiguration = relayServerConfiguration;

            _lock = new();

            _acquiredRelayServerPorts = new();
            _releasedRelayServerPorts = new();

            _lastPort = relayServerConfiguration.BasePort;
        }

        public int? AcquireRelayServerPort()
        {
            lock (_lock)
            {
                if (_acquiredRelayServerPorts.Count >= _relayServerConfiguration.MaximumSlots)
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
