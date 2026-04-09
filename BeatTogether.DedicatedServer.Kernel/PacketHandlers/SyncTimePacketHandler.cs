using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
    public sealed class SyncTimePacketHandler : BasePacketHandler<SyncTimePacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly Internal_Logger _logger;

        public SyncTimePacketHandler(IPacketDispatcher packetDispatcher, Kernel_Logger kernel_Logger)
        {
            _packetDispatcher = packetDispatcher;
            _logger = kernel_Logger.ForContext<SyncTimePacketHandler>();
        }

        public override void Handle(IPlayer sender, SyncTimePacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SyncTimePacket)}' " +
                $"(SenderId={sender.ConnectionId}, SyncTime={packet.SyncTime})."
            );

            _packetDispatcher.SendToPlayer(sender, new SyncTimePacket
            {
                SyncTime = sender.SyncTime
            }, IgnoranceChannelTypes.Reliable);
        }
    }
}
