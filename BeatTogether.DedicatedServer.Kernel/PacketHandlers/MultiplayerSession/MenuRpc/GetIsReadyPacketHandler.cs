using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetIsReadyPacketHandler : BasePacketHandler<GetIsReadyPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly Internal_Logger _logger;

        public GetIsReadyPacketHandler(
            IPacketDispatcher packetDispatcher,
            Kernel_Logger kernel_Logger)
        {
            _packetDispatcher = packetDispatcher;
            _logger = kernel_Logger.ForContext<GetIsReadyPacketHandler>();
        }

        public override void Handle(IPlayer sender, GetIsReadyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetIsReadyPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            _packetDispatcher.SendToPlayer(sender, new SetIsReadyPacket
            {
                IsReady = sender.IsReady
            }, IgnoranceChannelTypes.Reliable);
        }
    }
}
