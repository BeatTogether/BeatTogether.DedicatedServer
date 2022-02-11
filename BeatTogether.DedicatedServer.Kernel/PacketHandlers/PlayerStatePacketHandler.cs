using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using BeatTogether.LiteNetLib.Enums;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
    public sealed class PlayerStatePacketHandler : BasePacketHandler<PlayerStatePacket>
    {
        private readonly ILogger _logger = Log.ForContext<PlayerStatePacketHandler>();

        public override Task Handle(IPlayer sender, PlayerStatePacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(PlayerStatePacket)}' " +
                $"(SenderId={sender.ConnectionId}, IsPlayer={packet.PlayerState.Contains("player")}, IsModded={packet.PlayerState.Contains("modded")}, " + 
                $"IsActive={packet.PlayerState.Contains("is_active")}, WantsToPlayNextLevel={packet.PlayerState.Contains("wants_to_play_next_level")})."
            );

            sender.State = packet.PlayerState;

            return Task.CompletedTask;
        }
    }
}
