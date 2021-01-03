namespace BeatTogether.DedicatedServer.Kernel.Configuration
{
    public class RelayServerConfiguration
    {
        public int MaximumSlots { get; set; } = 10000;
        public int InactivityTimeout { get; set; } = 60000;
    }
}
