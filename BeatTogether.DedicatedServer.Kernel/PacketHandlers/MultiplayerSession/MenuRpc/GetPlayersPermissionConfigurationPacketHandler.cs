using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using BeatTogether.LiteNetLib.Enums;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

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

        public override Task Handle(IPlayer sender, GetPlayersPermissionConfigurationPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetPlayersPermissionConfigurationPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            _packetDispatcher.SendToPlayer(sender, new SetPlayersPermissionConfigurationPacket
            {
                PermissionConfiguration = new PlayersPermissionConfiguration
                {
                    PlayersPermission = _playerRegistry.Players.Select(x => new PlayerPermissionConfiguration
                    {
                        UserId = x.UserId,
                        IsServerOwner = x.IsServerOwner,
                        HasRecommendBeatmapsPermission = x.CanRecommendBeatmaps,
                        HasRecommendGameplayModifiersPermission = x.CanRecommendModifiers,
                        HasKickVotePermission = x.CanKickVote,
                        HasInvitePermission = x.CanInvite
                    }).ToArray()
                }
            }, DeliveryMethod.ReliableOrdered);

            return Task.CompletedTask;
        }
    }
}
