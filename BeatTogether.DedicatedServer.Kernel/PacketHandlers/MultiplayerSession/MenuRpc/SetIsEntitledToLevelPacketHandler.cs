using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class SetIsEntitledToLevelPacketHandler : BasePacketHandler<SetIsEntitledToLevelPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILobbyManager _lobbyManager;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly Internal_Logger _logger;

        public SetIsEntitledToLevelPacketHandler(
            IPacketDispatcher packetDispatcher,
            ILobbyManager lobbyManager,
            IPlayerRegistry playerRegistry,
            Kernel_Logger kernel_Logger)
        {
            _packetDispatcher = packetDispatcher;
            _lobbyManager = lobbyManager;
            _playerRegistry = playerRegistry;
            _logger = kernel_Logger.ForContext<SetIsEntitledToLevelPacketHandler>();
        }

        public override void Handle(IPlayer sender, SetIsEntitledToLevelPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SetIsEntitledToLevelPacket)}' " +
                $"(SenderId={sender.ConnectionId}, LevelId={packet.LevelId}, Entitlement={packet.Entitlement})."
            );
            sender.SetEntitlement(packet.LevelId, packet.Entitlement);
            foreach (IPlayer player in _playerRegistry.Players)
            {
                if(player.BeatmapIdentifier != null && player.BeatmapIdentifier.LevelId == packet.LevelId)
                    player.UpdateEntitlement = true;
            }
        }
    }
}
