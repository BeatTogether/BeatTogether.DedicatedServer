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
        private readonly IPlayerRegistry _playerRegistry;
        private readonly ILogger _logger = Log.ForContext<SetIsEntitledToLevelPacketHandler>();

        public SetIsEntitledToLevelPacketHandler(
            IPacketDispatcher packetDispatcher,
            ILobbyManager lobbyManager,
            IPlayerRegistry playerRegistry)
        {
            _packetDispatcher = packetDispatcher;
            _lobbyManager = lobbyManager;
            _playerRegistry = playerRegistry;
        }

        public override async Task Handle(IPlayer sender, SetIsEntitledToLevelPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SetIsEntitledToLevelPacket)}' " +
                $"(SenderId={sender.ConnectionId}, LevelId={packet.LevelId}, Entitlement={packet.Entitlement})."
            );
            await sender.PlayerAccessSemaphore.WaitAsync();
            sender.SetEntitlement(packet.LevelId, packet.Entitlement);
            foreach (IPlayer player in _playerRegistry.Players)
            {
                if(player.BeatmapIdentifier != null && player.BeatmapIdentifier.LevelId == packet.LevelId)
                    player.UpdateEntitlement = true;
            }
            sender.PlayerAccessSemaphore.Release();
            return;
        }
    }
}
