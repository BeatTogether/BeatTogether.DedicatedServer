using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;
using BeatTogether.LiteNetLib.Enums;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
    public sealed class PingPacketHandler : BasePacketHandler<PingPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<PingPacketHandler>();

        public PingPacketHandler(IPacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
        }

        public override void Handle(IPlayer sender, PingPacket packet)
        {
            _logger.Verbose(
                $"Handling packet of type '{nameof(PingPacket)}' " +
                $"(SenderId={sender.ConnectionId}, PingTime={packet.PingTime})."
            );

            _packetDispatcher.SendToPlayer(sender, new PongPacket
            {
                PingTime = packet.PingTime
            }, DeliveryMethod.ReliableOrdered);
        }
    }
}
