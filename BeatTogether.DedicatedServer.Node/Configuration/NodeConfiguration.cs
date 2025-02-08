using System;

namespace BeatTogether.DedicatedServer.Node.Configuration
{
    public sealed class NodeConfiguration
    {
        public string HostEndpoint { get; set; } = "127.0.0.1";
        public Version NodeVersion { get; } = new Version(2,1,0);
    }
}
