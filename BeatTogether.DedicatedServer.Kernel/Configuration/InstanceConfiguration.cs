using BeatTogether.DedicatedServer.Kernel.Enums;

namespace BeatTogether.DedicatedServer.Kernel.Configuration
{
    public sealed class InstanceConfiguration
    {
        public int Port { get; }
        public string Secret { get; }
        public string ManagerId { get; set; }
        public int MaxPlayerCount { get; }
        public DiscoveryPolicy DiscoveryPolicy { get; }
        public InvitePolicy InvitePolicy { get; }
        public GameplayServerMode GameplayServerMode { get; }
        public SongSelectionMode SongSelectionMode { get; }
        public GameplayServerControlSettings GameplayServerControlSettings { get; }

        public InstanceConfiguration(
            int port,
            string secret,
            string managerId,
            int maxPlayerCount,
            DiscoveryPolicy discoveryPolicy,
            InvitePolicy invitePolicy,
            GameplayServerMode gameplayServerMode,
            SongSelectionMode songSelectionMode,
            GameplayServerControlSettings gameplayServerControlSettings)
        {
            Port = port;
            Secret = secret;
            ManagerId = managerId;
            MaxPlayerCount = maxPlayerCount;
            DiscoveryPolicy = discoveryPolicy;
            InvitePolicy = invitePolicy;
            GameplayServerMode = gameplayServerMode;
            SongSelectionMode = songSelectionMode;
            GameplayServerControlSettings = gameplayServerControlSettings;
        }
    }
}
