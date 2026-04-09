using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetRecommendedModifiersPacketHandler : BasePacketHandler<GetRecommendedModifiersPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly Internal_Logger _logger;

        public GetRecommendedModifiersPacketHandler(
            IPacketDispatcher packetDispatcher,
            Kernel_Logger kernel_Logger)
        {
            _packetDispatcher = packetDispatcher;
            _logger = kernel_Logger.ForContext<GetRecommendedModifiersPacketHandler>();
        }

        public override void Handle(IPlayer sender, GetRecommendedModifiersPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetRecommendedModifiersPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            _packetDispatcher.SendToPlayer(sender, new SetRecommendedModifiersPacket
            {
                Modifiers = sender.Modifiers
            }, IgnoranceChannelTypes.Reliable);
        }
    }
}
