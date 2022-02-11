using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using BeatTogether.LiteNetLib.Enums;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class SetIsInLobbyPacketHandler : BasePacketHandler<SetIsInLobbyPacket>
    {
        private readonly PacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<SetIsInLobbyPacketHandler>();

        public SetIsInLobbyPacketHandler(
            PacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
        }

        public override Task Handle(IPlayer sender, SetIsInLobbyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SetIsInLobbyPacket)}' " +
                $"(SenderId={sender.ConnectionId}, InLobby={packet.IsInLobby})."
            );

            if (packet.IsInLobby && !sender.InLobby)
                _packetDispatcher.SendToPlayer(sender, new SetIsStartButtonEnabledPacket
                {
                    Reason = CannotStartGameReason.NoSongSelected
                }, DeliveryMethod.ReliableOrdered);

            sender.InLobby = packet.IsInLobby;
            return Task.CompletedTask;
        }
    }
}
