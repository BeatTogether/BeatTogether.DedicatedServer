using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
    public sealed class SyncTimePacketHandler : BasePacketHandler<SyncTimePacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<SyncTimePacketHandler>();

        public SyncTimePacketHandler(IPacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
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
