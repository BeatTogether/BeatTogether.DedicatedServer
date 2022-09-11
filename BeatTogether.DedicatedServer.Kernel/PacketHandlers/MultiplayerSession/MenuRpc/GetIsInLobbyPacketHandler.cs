using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using BeatTogether.LiteNetLib.Enums;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetIsInLobbyPacketHandler : BasePacketHandler<GetIsInLobbyPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly IDedicatedInstance _instance;
        private readonly ILogger _logger = Log.ForContext<GetIsInLobbyPacketHandler>();

        public GetIsInLobbyPacketHandler(
            IPacketDispatcher packetDispatcher, IDedicatedInstance instance)
        {
            _packetDispatcher = packetDispatcher;
            _instance = instance;
        }

        public override Task Handle(IPlayer sender, GetIsInLobbyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetIsInLobbyPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            _packetDispatcher.SendToPlayer(sender, new SetIsInLobbyPacket
            {
                IsInLobby = _instance.State != MultiplayerGameState.Game
            }, DeliveryMethod.ReliableOrdered);

            return Task.CompletedTask;
        }
    }
}
