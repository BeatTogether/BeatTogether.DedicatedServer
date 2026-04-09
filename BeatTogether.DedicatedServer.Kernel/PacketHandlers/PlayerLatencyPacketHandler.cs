using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
    class PlayerLatencyPacketHandler : BasePacketHandler<PlayerLatencyPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly Internal_Logger _logger;

        public PlayerLatencyPacketHandler(
            IPacketDispatcher packetDispatcher,
            Kernel_Logger kernel_Logger)
        {
            _packetDispatcher = packetDispatcher;
            _logger = kernel_Logger.ForContext<PlayerLatencyPacketHandler>();
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
