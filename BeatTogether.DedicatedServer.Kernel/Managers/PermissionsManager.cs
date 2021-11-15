using System.Linq;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;

namespace BeatTogether.DedicatedServer.Kernel.Managers
{
    public sealed class PermissionsManager : IPermissionsManager
    {
        public bool AllowBeatmapSelect { get; private set; } = true;
        public bool AllowVoteKick { get; private set; } = false;
        public bool AllowInvite => _server.Configuration.DiscoveryPolicy is
            Enums.DiscoveryPolicy.WithCode or Enums.DiscoveryPolicy.Public;

        public PlayersPermissionConfiguration Permissions { get; private set; } = new();

        private IMatchmakingServer _server;
        private IPlayerRegistry _playerRegistry;
        private IPacketDispatcher _packetDispatcher;

        public PermissionsManager(
            IMatchmakingServer server,
            IPlayerRegistry playerRegistry,
            IPacketDispatcher packetDispatcher)
        {
            _server = server;
            _playerRegistry = playerRegistry;
            _packetDispatcher = packetDispatcher;
        }

        public void UpdatePermissions()
        {
            Permissions.PlayersPermission.Clear();
            foreach (IPlayer player in _playerRegistry.Players)
            {
                var playerPermission = new PlayerPermissionConfiguration
                {
                    UserId = player.UserId,
                    IsServerOwner = player.UserId == _server.ManagerId,
                    HasRecommendBeatmapsPermission = AllowBeatmapSelect,
                    HasRecommendGameplayModifiersPermission = _server.Configuration.GameplayServerControlSettings == Enums.GameplayServerControlSettings.AllowModifierSelection || _server.Configuration.GameplayServerControlSettings == Enums.GameplayServerControlSettings.All,
                    HasKickVotePermission = player.UserId == _server.ManagerId || AllowVoteKick,
                    HasInvitePermission = AllowInvite
                };
                Permissions.PlayersPermission.Add(playerPermission);
            }
        }

        public bool PlayerCanRecommendBeatmaps(string userId)
            => Permissions.PlayersPermission.Any(p => p.UserId == userId && p.HasRecommendBeatmapsPermission);

        public bool PlayerCanRecommendModifiers(string userId)
            => Permissions.PlayersPermission.Any(p => p.UserId == userId && p.HasRecommendGameplayModifiersPermission);

        public bool PlayerCanKickVote(string userId)
            => Permissions.PlayersPermission.Any(p => p.UserId == userId && p.HasKickVotePermission);

        public bool PlayerCanInvite(string userId)
            => Permissions.PlayersPermission.Any(p => p.UserId == userId && p.HasInvitePermission);
    }
}
