namespace BeatTogether.DedicatedServer.Kernel.Configuration
{
    public class DedicatedServerConfiguration
    {
        public string HostName { get; set; } = "127.0.0.1";
        public int BasePort { get; set; } = 30000;
        public RelayServerConfiguration RelayServers { get; set; } = new RelayServerConfiguration();
    }
}
