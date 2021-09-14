using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetRecommendedBeatmapPacketHandler : BasePacketHandler<GetRecommendedBeatmapPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<GetRecommendedBeatmapPacketHandler>();

        public GetRecommendedBeatmapPacketHandler(IPacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
        }

        public override Task Handle(IPlayer sender, GetRecommendedBeatmapPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetRecommendedBeatmapPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            // Doesn't need to be handled

            return Task.CompletedTask;
        }
    }
}
