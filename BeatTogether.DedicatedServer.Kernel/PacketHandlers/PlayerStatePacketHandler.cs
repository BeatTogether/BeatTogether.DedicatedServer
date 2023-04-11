using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;
using BeatTogether.LiteNetLib.Enums;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
    public sealed class PlayerStatePacketHandler : BasePacketHandler<PlayerStatePacket>
    {
        private readonly ILogger _logger = Log.ForContext<PlayerStatePacketHandler>();
        private readonly IPacketDispatcher _packetDispatcher;

        PlayerStatePacketHandler(IPacketDispatcher packetDispatcher)
        {
            _packetDispatcher = packetDispatcher;
        }
        public override Task Handle(IPlayer sender, PlayerStatePacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(PlayerStatePacket)}' " +
                $"(SenderId={sender.ConnectionId}, IsPlayer={packet.PlayerState.Contains("player")}, IsModded={packet.PlayerState.Contains("modded")}, " + 
                $"IsActive={packet.PlayerState.Contains("is_active")}, WantsToPlayNextLevel={packet.PlayerState.Contains("wants_to_play_next_level")})."
            );
            lock (sender.StateLock)
            {
                sender.State = packet.PlayerState;
            }
            _packetDispatcher.SendExcludingPlayer(sender, packet, DeliveryMethod.ReliableOrdered); //TODO testing what adding this does
            return Task.CompletedTask;
        }
    }
}
