using System;

namespace BeatTogether.DedicatedServer.Node.Configuration
{
    public sealed class NodeConfiguration
    {
        //public string HostName { get; set; } = "192.168.0.21";
        public string HostName { get; set; } = "127.0.0.1";
        public Version NodeVersion { get; } = new Version(2,0,0);
    }
}
