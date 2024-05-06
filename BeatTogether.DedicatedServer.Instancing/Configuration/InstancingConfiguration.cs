using System;

namespace BeatTogether.DedicatedServer.Instancing.Configuration
{
    public sealed class InstancingConfiguration
    {
        public string HostName { get; set; } = "192.168.0.21";
        public int BasePort { get; set; } = 30000;
        public int MaximumSlots { get; set; } = 10000;
    }
}
