using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using LiteNetLib;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetPlayersPermissionConfigurationPacketHandler : BasePacketHandler<GetPlayersPermissionConfigurationPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<GetPlayersPermissionConfigurationPacketHandler>();

        public GetPlayersPermissionConfigurationPacketHandler(IPacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
        }

        public override Task Handle(IPlayer sender, GetPlayersPermissionConfigurationPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetPlayersPermissionConfigurationPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            var playerPermissionConfiguration = new PlayerPermissionConfiguration
            {
                UserId = sender.UserId,
                IsServerOwner = true,
                HasRecommendBeatmapsPermission = true,
                HasRecommendGameplayModifiersPermission = true,
                HasKickVotePermission = true
            };

            var permissionConfigurationPacket = new SetPlayersPermissionConfigurationPacket
            {
                PermissionConfiguration = sender.MatchmakingServer.Permissions
            };
            _packetDispatcher.SendToPlayer(sender, permissionConfigurationPacket, DeliveryMethod.ReliableOrdered);

            return Task.CompletedTask;
        }
    }
}
