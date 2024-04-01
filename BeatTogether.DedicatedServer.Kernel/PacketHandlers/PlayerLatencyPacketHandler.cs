using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
    class PlayerLatencyPacketHandler : BasePacketHandler<PlayerLatencyPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<PlayerLatencyPacketHandler>();

        public PlayerLatencyPacketHandler(
            IPacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
        }

        public override void Handle(IPlayer sender, PlayerLatencyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(PlayerLatencyPacket)}' " +
                $"(SenderId={sender.ConnectionId}, Latency={packet.Latency})."
            );

            sender.Latency.Update(packet.Latency);
            _packetDispatcher.SendFromPlayer(sender, new PlayerLatencyPacket
            {
                Latency = sender.Latency.CurrentAverage
            }, IgnoranceChannelTypes.Reliable);
        }
    }
}
