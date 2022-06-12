using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class SetIsEntitledToLevelPacketHandler : BasePacketHandler<SetIsEntitledToLevelPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILobbyManager _lobbyManager;
        private readonly ILogger _logger = Log.ForContext<SetIsEntitledToLevelPacketHandler>();

        public SetIsEntitledToLevelPacketHandler(
            IPacketDispatcher packetDispatcher,
            ILobbyManager lobbyManager)
        {
            _packetDispatcher = packetDispatcher;
            _lobbyManager = lobbyManager;
        }

        public override Task Handle(IPlayer sender, SetIsEntitledToLevelPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SetIsEntitledToLevelPacket)}' " +
                $"(SenderId={sender.ConnectionId}, LevelId={packet.LevelId}, Entitlement={packet.Entitlement})."
            );

            sender.SetEntitlement(packet.LevelId, packet.Entitlement);
            _lobbyManager.RunUpdate();
            return Task.CompletedTask;
        }
    }
}
