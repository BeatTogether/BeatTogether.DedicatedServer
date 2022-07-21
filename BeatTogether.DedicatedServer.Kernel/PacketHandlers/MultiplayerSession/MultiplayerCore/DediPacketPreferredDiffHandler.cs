using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    class DediPacketPreferredDiffHandler : BasePacketHandler<DediPacketPreferredDiff>
    {
        private readonly ILobbyManager _lobbyManager;
        private readonly ILogger _logger = Log.ForContext<DediPacketPreferredDiff>();

        public DediPacketPreferredDiffHandler(
            ILobbyManager lobbyManager)
        {
            _lobbyManager = lobbyManager;
        }

        public override Task Handle(IPlayer sender, DediPacketPreferredDiff packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(DediPacketPreferredDiff)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            lock (sender.PreferDiffLock)
            {
                sender.PreferredDifficulty = (Messaging.Models.BeatmapDifficulty)packet.PreferredDifficulty;
            }
            return Task.CompletedTask;
        }
    }
}
