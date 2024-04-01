using System;

namespace BeatTogether.DedicatedServer.Node.Configuration
{
    public sealed class NodeConfiguration
    {
        public string HostName { get; set; } = "127.0.0.1";
        public Version NodeVersion { get; } = new Version(1,6,0);
        public int BasePort { get; set; } = 30000;
        public int MaximumSlots { get; set; } = 10000;
    }
}
