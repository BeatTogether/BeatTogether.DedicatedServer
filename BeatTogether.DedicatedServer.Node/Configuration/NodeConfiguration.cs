namespace BeatTogether.DedicatedServer.Node.Configuration
{
    public sealed class NodeConfiguration
    {
        public string HostName { get; set; } = "109.148.35.230";
        public int BasePort { get; set; } = 30000;
        public int MaximumSlots { get; set; } = 10000;
    }
}
