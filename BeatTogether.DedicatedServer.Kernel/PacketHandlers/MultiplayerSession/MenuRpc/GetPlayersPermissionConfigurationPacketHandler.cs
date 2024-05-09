using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetPlayersPermissionConfigurationPacketHandler : BasePacketHandler<GetPlayersPermissionConfigurationPacket>
    {
        private readonly InstanceConfiguration _configuration;
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly ILogger _logger = Log.ForContext<GetPlayersPermissionConfigurationPacketHandler>();

        public GetPlayersPermissionConfigurationPacketHandler(
            InstanceConfiguration configuration,
            IPacketDispatcher packetDispatcher,
            IPlayerRegistry playerRegistry)
        {
            _configuration = configuration;
            _packetDispatcher = packetDispatcher;
            _playerRegistry = playerRegistry;
        }

        public override void Handle(IPlayer sender, GetPlayersPermissionConfigurationPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetPlayersPermissionConfigurationPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            //sends player there own permissions, and those of the player who is the current manager.
            bool HasManager = (_playerRegistry.TryGetPlayer(_configuration.ServerOwnerId, out var ServerOwner) && !sender.IsServerOwner);
            PlayerPermissionConfiguration[] playerPermissionConfigurations = new PlayerPermissionConfiguration[HasManager ? 2 : 1];
            playerPermissionConfigurations[0] = new PlayerPermissionConfiguration
            {
                UserId = sender.HashedUserId,
                IsServerOwner = sender.IsServerOwner,
                HasRecommendBeatmapsPermission = sender.CanRecommendBeatmaps,
                HasRecommendGameplayModifiersPermission = sender.CanRecommendModifiers,
                HasKickVotePermission = sender.CanKickVote,
                HasInvitePermission = sender.CanInvite
            };
            if (HasManager)
                playerPermissionConfigurations[1] = new PlayerPermissionConfiguration
                {
                    UserId = ServerOwner!.HashedUserId,
                    IsServerOwner = ServerOwner!.IsServerOwner,
                    HasRecommendBeatmapsPermission = ServerOwner!.CanRecommendBeatmaps,
                    HasRecommendGameplayModifiersPermission = ServerOwner!.CanRecommendModifiers,
                    HasKickVotePermission = ServerOwner!.CanKickVote,
                    HasInvitePermission = ServerOwner!.CanInvite
                };
            _packetDispatcher.SendToPlayer(sender, new SetPlayersPermissionConfigurationPacket
            {
                PermissionConfiguration = new PlayersPermissionConfiguration
                {
                    PlayersPermission = playerPermissionConfigurations
                }
            }, IgnoranceChannelTypes.Reliable);
        }
    }
}
