using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using BeatTogether.LiteNetLib.Enums;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetMultiplayerGameStatePacketHandler : BasePacketHandler<GetMultiplayerGameStatePacket>
    {
        private readonly IDedicatedInstance _instance;
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<GetMultiplayerGameStatePacketHandler>();

        public GetMultiplayerGameStatePacketHandler(
            IDedicatedInstance instance,
            IPacketDispatcher packetDispatcher)
        {
            _instance = instance;
            _packetDispatcher = packetDispatcher;
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
            }, DeliveryMethod.ReliableOrdered);
        }
    }
}
