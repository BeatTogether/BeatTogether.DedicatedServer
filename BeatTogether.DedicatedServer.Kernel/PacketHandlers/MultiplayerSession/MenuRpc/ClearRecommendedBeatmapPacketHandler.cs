using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    class ClearRecommendedBeatmapPacketHandler : BasePacketHandler<ClearRecommendedBeatmapPacket>
    {
        private readonly ILogger _logger = Log.ForContext<ClearRecommendedBeatmapPacketHandler>();

        public ClearRecommendedBeatmapPacketHandler()
        {
        }

        public override Task Handle(IPlayer sender, ClearRecommendedBeatmapPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(ClearRecommendedBeatmapPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            sender.BeatmapIdentifier = null;
            return Task.CompletedTask;
        }
    }
}
