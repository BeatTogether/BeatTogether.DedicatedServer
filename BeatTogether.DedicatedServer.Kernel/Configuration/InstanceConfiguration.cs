using BeatTogether.DedicatedServer.Kernel.Enums;

namespace BeatTogether.DedicatedServer.Kernel.Configuration
{
    public sealed class InstanceConfiguration
    {
        //public int LiteNetPort { get; set; }
        public int Port { get; set; }
        public string Secret { get; set; } = string.Empty;
        public string ServerOwnerId { get; set; } = string.Empty;
        public string ServerId { get; set; } = "ziuMSceapEuNN7wRGQXrZg";
        public string WelcomeMessage { get; set; } = string.Empty;
        public string ServerName { get; set; } = string.Empty;
        public float DestroyInstanceTimeout { get; set; } = 10f; //set to -1 for no timeout(must close using api), 0 would be for instant close, 10 seconds is default. Less than 6 seconds can cause cfr-3 issues
        public string SetConstantManagerFromUserId { get; set; } = string.Empty; //If a user creates a server using the api and enteres there userId (eg uses discord bot with linked account))
        public bool AllowPerPlayerDifficulties { get; set; } = false;
        public bool AllowPerPlayerModifiers { get; set; } = false;
        public CountdownConfig CountdownConfig { get; set; } = new();
        public int MaxPlayerCount { get; set; }
        public DiscoveryPolicy DiscoveryPolicy { get; set; }
        public InvitePolicy InvitePolicy { get; set; }
        public GameplayServerMode GameplayServerMode { get; set; }
        public SongSelectionMode SongSelectionMode { get; set; }
        public GameplayServerControlSettings GameplayServerControlSettings { get; set; }
        public bool AllowChroma { get; set; }
        public bool AllowMappingExtensions { get; set; }
        public bool AllowNoodleExtensions { get; set; }
        public bool DisableNotes { get; set; }
        public bool ForceEnableNotes { get; set; } = false;
        public bool ForceStartMode { get; set; } = false;
        public long SendPlayersWithoutEntitlementToSpectateTimeout { get; set; } = 30000L;
        public int MaxLengthCommand { get; set; } = 200;
        public bool ApplyNoFailModifier { get; set; } = true;
    }

    public sealed class CountdownConfig
    {
        public long CountdownTimePlayersReady { get; set; } = 30000L;
        public long BeatMapStartCountdownTime { get; set; } = 5000L;
        public float ResultsScreenTime { get; set; } = 20.0f;
    }
}
