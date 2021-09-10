using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using LiteNetLib;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    class ClearRecommendedBeatmapPacketHandler : BasePacketHandler<ClearRecommendedBeatmapPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<ClearRecommendedBeatmapPacketHandler>();

        public ClearRecommendedBeatmapPacketHandler(
            IPacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
        }

        public override Task Handle(IPlayer sender, ClearRecommendedBeatmapPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(ClearRecommendedBeatmapPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            sender.BeatmapIdentifier = null;
            var setIsStartButtonEnabledPacket = new SetIsStartButtonEnabledPacket
            {
                Reason = CannotStartGameReason.NoSongSelected
            };
            _packetDispatcher.SendToPlayer(sender, setIsStartButtonEnabledPacket, DeliveryMethod.ReliableOrdered);

            return Task.CompletedTask;
        }
    }
}
