using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    class ClearRecommendedModifiersPacketHandler : BasePacketHandler<ClearRecommendedModifiersPacket>
    {
        private readonly ILobbyManager _lobbyManager;
        private readonly Internal_Logger _logger;

        public ClearRecommendedModifiersPacketHandler(
            ILobbyManager lobbyManager,
            Kernel_Logger kernel_Logger)
        {
            _lobbyManager = lobbyManager;
            _logger = kernel_Logger.ForContext<ClearRecommendedModifiersPacketHandler>();
        }

        public override void Handle(IPlayer sender, ClearRecommendedModifiersPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(ClearRecommendedModifiersPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            sender.Modifiers = _lobbyManager.EmptyModifiers;
        }
    }
}
