using BeatTogether.DedicatedServer.Messaging.Models;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IPermissionsManager
    {
        bool AllowBeatmapSelect { get; }
        bool AllowVoteKick { get; }
        PlayersPermissionConfiguration Permissions { get; }

		bool PlayerCanInvite(string userId);
		bool PlayerCanKickVote(string userId);
		bool PlayerCanRecommendBeatmaps(string userId);
		bool PlayerCanRecommendModifiers(string userId);
		void UpdatePermissions();
    }
}
