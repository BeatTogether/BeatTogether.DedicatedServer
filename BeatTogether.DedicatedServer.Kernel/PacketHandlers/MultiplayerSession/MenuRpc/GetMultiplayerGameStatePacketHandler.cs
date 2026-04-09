using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetMultiplayerGameStatePacketHandler : BasePacketHandler<GetMultiplayerGameStatePacket>
    {
        private readonly IDedicatedInstance _instance;
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly Internal_Logger _logger;

        public GetMultiplayerGameStatePacketHandler(
            IDedicatedInstance instance,
            IPacketDispatcher packetDispatcher,
            Kernel_Logger kernel_Logger)
        {
            _instance = instance;
            _packetDispatcher = packetDispatcher;
            _logger = kernel_Logger.ForContext<GetMultiplayerGameStatePacketHandler>();
        }

        public override void Handle(IPlayer sender, GetMultiplayerGameStatePacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetMultiplayerGameStatePacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            _packetDispatcher.SendToPlayer(sender, new SetMultiplayerGameStatePacket
            {
                State = _instance.State
            }, IgnoranceChannelTypes.Reliable);
        }
    }
}
