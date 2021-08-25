using BeatTogether.DedicatedServer.Interface.Enums;

namespace BeatTogether.DedicatedServer.Interface.Models
{
    public record GameplayServerConfiguration(
        int MaxPlayerCount,
        DiscoveryPolicy DiscoveryPolicy,
        InvitePolicy InvitePolicy,
        GameplayServerMode GameplayServerMode,
        SongSelectionMode SongSelectionMode,
        GameplayServerControlSettings GameplayServerControlSettings);
}
