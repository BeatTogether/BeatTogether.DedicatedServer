using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetRecommendedGameplayModifiersPacketHandler : BasePacketHandler<GetRecommendedGameplayModifiersPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<GetRecommendedGameplayModifiersPacketHandler>();

        public GetRecommendedGameplayModifiersPacketHandler(IPacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
        }

        public override Task Handle(IPlayer sender, GetRecommendedGameplayModifiersPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetRecommendedGameplayModifiersPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            // TODO
            return Task.CompletedTask;
        }
    }
}
