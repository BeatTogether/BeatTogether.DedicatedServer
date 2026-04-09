using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
    public sealed class PingPacketHandler_1_40_8 : BasePacketHandler<PingPacket_1_40_8>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        //private readonly ILogger _logger = Log.ForContext<PingPacketHandler_1_40_8>();
        private readonly Internal_Logger _logger;

        public PingPacketHandler_1_40_8(IPacketDispatcher packetDispatcher, Kernel_Logger kernel_Logger)
        {
            _packetDispatcher = packetDispatcher;
            _logger = kernel_Logger.ForContext<PingPacketHandler_1_40_8>();
        }

        public override void Handle(IPlayer sender, PingPacket_1_40_8 packet)
        {
            _logger.Verbose(
                $"Handling packet of type '{nameof(PingPacket_1_40_8)}' " +
                $"(SenderId={sender.ConnectionId}, PingTime={packet.PingTime})."
            );

            _packetDispatcher.SendToPlayer(sender, new PongPacket_1_40_8
            {
                PingTime = packet.PingTime
            }, IgnoranceChannelTypes.Reliable);
        }
    }
}
