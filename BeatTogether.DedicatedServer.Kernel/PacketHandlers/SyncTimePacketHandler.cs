using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;
using LiteNetLib;
using Serilog;
using System.Threading.Tasks;

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

        public override Task Handle(IPlayer sender, SyncTimePacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SyncTimePacket)}' " +
                $"(SenderId={sender.ConnectionId}, SyncTime={packet.SyncTime})."
            );

            var syncTimePacket = new SyncTimePacket
            {
                SyncTime = sender.SyncTime
            };
            _packetDispatcher.SendToPlayer(sender, syncTimePacket, DeliveryMethod.ReliableOrdered);

            return Task.CompletedTask;
        }
    }
}
