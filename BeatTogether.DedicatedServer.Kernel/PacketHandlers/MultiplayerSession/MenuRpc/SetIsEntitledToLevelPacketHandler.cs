using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class SetIsEntitledToLevelPacketHandler : BasePacketHandler<SetIsEntitledToLevelPacket>
    {
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly IEntitlementManager _entitlementManager;
        private readonly ILogger _logger = Log.ForContext<SetIsEntitledToLevelPacketHandler>();

        public SetIsEntitledToLevelPacketHandler(
            IPlayerRegistry playerRegistry,
            IPacketDispatcher packetDispatcher,
            IEntitlementManager entitlementManager)
        {
            _playerRegistry = playerRegistry;
            _packetDispatcher = packetDispatcher;
            _entitlementManager = entitlementManager;
        }

        public override Task Handle(IPlayer sender, SetIsEntitledToLevelPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SetIsEntitledToLevelPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            _entitlementManager.HandleEntitlement(sender.UserId, packet.LevelId, packet.Entitlement);

            return Task.CompletedTask;
        }
    }
}
