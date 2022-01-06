using BeatTogether.DedicatedServer.Kernel.Enums;

namespace BeatTogether.DedicatedServer.Kernel.Configuration
{
    public sealed class InstanceConfiguration
    {
        public int Port { get; set; }
        public string Secret { get; set; } = string.Empty;
        public string ManagerId { get; set; } = string.Empty;
        public int MaxPlayerCount { get; set; }
        public DiscoveryPolicy DiscoveryPolicy { get; set; }
        public InvitePolicy InvitePolicy { get; set; }
        public GameplayServerMode GameplayServerMode { get; set; }
        public SongSelectionMode SongSelectionMode { get; set; }
        public GameplayServerControlSettings GameplayServerControlSettings { get; set; }
    }
}
