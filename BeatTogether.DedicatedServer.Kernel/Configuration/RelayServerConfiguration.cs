namespace BeatTogether.DedicatedServer.Kernel.Configuration
{
    public class RelayServerConfiguration
    {
        public string HostName { get; set; } = "127.0.0.1";
        public int BasePort { get; set; } = 30000;
        public int MaximumSlots { get; set; } = 10000;
        public int InactivityTimeout { get; set; } = 60000;
    }
}
