using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using LiteNetLib;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetIsReadyPacketHandler : BasePacketHandler<GetIsReadyPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<GetIsReadyPacketHandler>();

        public GetIsReadyPacketHandler(IPacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
        }

        public override Task Handle(IPlayer sender, GetIsReadyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetIsReadyPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            // Doesn't need to be handled

            return Task.CompletedTask;
        }
    }
}
