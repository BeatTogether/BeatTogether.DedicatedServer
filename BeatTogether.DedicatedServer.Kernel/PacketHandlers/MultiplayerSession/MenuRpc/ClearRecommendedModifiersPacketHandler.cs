using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    class ClearRecommendedModifiersPacketHandler : BasePacketHandler<ClearRecommendedModifiersPacket>
    {
        private readonly ILogger _logger = Log.ForContext<ClearRecommendedModifiersPacketHandler>();

        public ClearRecommendedModifiersPacketHandler()
        {
        }

        public override Task Handle(IPlayer sender, ClearRecommendedModifiersPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(ClearRecommendedModifiersPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            sender.Modifiers = new GameplayModifiers();
            return Task.CompletedTask;
        }
    }
}
