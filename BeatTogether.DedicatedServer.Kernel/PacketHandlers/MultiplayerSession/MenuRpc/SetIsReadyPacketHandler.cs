using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class SetIsReadyPacketHandler : BasePacketHandler<SetIsReadyPacket>
    {
        private readonly ILobbyManager _lobbyManager;
        private readonly ILogger _logger = Log.ForContext<SetIsReadyPacketHandler>();

        public SetIsReadyPacketHandler(
            ILobbyManager lobbyManager)
        {
            _lobbyManager = lobbyManager;
        }

        public override Task Handle(IPlayer sender, SetIsReadyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SetIsReadyPacket)}' " +
                $"(SenderId={sender.ConnectionId}, IsReady={packet.IsReady})."
            );

            sender.IsReady = packet.IsReady;
            _lobbyManager.Update();

            return Task.CompletedTask;
        }
    }
}
