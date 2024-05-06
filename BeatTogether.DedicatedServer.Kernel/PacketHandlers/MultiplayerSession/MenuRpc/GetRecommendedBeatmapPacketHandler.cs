using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetRecommendedBeatmapPacketHandler : BasePacketHandler<GetRecommendedBeatmapPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<GetRecommendedBeatmapPacketHandler>();

        public GetRecommendedBeatmapPacketHandler(
            IPacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
        }

        public override void Handle(IPlayer sender, GetRecommendedBeatmapPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetRecommendedBeatmapPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            //TODO send custom packet details
            if (sender.BeatmapIdentifier != null)
                _packetDispatcher.SendToPlayer(sender, new SetRecommendedBeatmapPacket
                {
                    BeatmapIdentifier = sender.BeatmapIdentifier
                }, IgnoranceChannelTypes.Reliable);
        }
    }
}
