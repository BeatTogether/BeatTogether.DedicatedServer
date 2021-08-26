using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetStartedLevelPacketHandler : BasePacketHandler<GetStartedLevelPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<GetStartedLevelPacketHandler>();

        public GetStartedLevelPacketHandler(IPacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
        }

        public override Task Handle(IPlayer sender, GetStartedLevelPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetStartedLevelPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            // TODO
            return Task.CompletedTask;
        }
    }
}
