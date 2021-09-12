using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using LiteNetLib;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetPlayersPermissionConfigurationPacketHandler : BasePacketHandler<GetPlayersPermissionConfigurationPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly IPermissionsManager _permissionsManager;
        private readonly ILogger _logger = Log.ForContext<GetPlayersPermissionConfigurationPacketHandler>();

        public GetPlayersPermissionConfigurationPacketHandler(
            IPacketDispatcher packetDispatcher,
            IPermissionsManager permissionsManager)
        {
            _packetDispatcher = packetDispatcher;
            _permissionsManager = permissionsManager;
        }

        public override Task Handle(IPlayer sender, GetPlayersPermissionConfigurationPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetPlayersPermissionConfigurationPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            _permissionsManager.UpdatePermissions();
            _packetDispatcher.SendToPlayer(sender, new SetPlayersPermissionConfigurationPacket
            {
                PermissionConfiguration = _permissionsManager.Permissions
            }, DeliveryMethod.ReliableOrdered);

            return Task.CompletedTask;
        }
    }
}
