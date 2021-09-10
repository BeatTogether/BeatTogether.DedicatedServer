using BeatTogether.DedicatedServer.Messaging.Models;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IPermissionsManager
    {
        bool AllowBeatmapSelect { get; }
        bool AllowVoteKick { get; }
        PlayersPermissionConfiguration Permissions { get; }

        void UpdatePermissions();
    }
}
