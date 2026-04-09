using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetRecommendedBeatmapPacketHandler : BasePacketHandler<GetRecommendedBeatmapPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly Internal_Logger _logger;

        public GetRecommendedBeatmapPacketHandler(
            IPacketDispatcher packetDispatcher,
            Kernel_Logger kernel_Logger)
        {
            _packetDispatcher = packetDispatcher;
            _logger = kernel_Logger.ForContext<GetRecommendedBeatmapPacketHandler>();
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
