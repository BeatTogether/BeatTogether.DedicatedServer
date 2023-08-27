using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
    public sealed class PlayerStatePacketHandler : BasePacketHandler<PlayerStatePacket>
    {
        private readonly ILogger _logger = Log.ForContext<PlayerStatePacketHandler>();
        private readonly ILobbyManager _lobbyManager;

        public PlayerStatePacketHandler(ILobbyManager lobbyManager)
        {
            _lobbyManager = lobbyManager;
        }

        public override async Task Handle(IPlayer sender, PlayerStatePacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(PlayerStatePacket)}' " +
                $"(SenderId={sender.ConnectionId}, IsPlayer={packet.PlayerState.Contains("player")}" + 
                $"IsActive={packet.PlayerState.Contains("is_active")}, WantsToPlayNextLevel={packet.PlayerState.Contains("wants_to_play_next_level")}" +
                $"IsSpectating={packet.PlayerState.Contains("spectating")}, InMenu={packet.PlayerState.Contains("in_menu")}" +
                $"backgrounded={packet.PlayerState.Contains("backgrounded")}, in_gameplay={packet.PlayerState.Contains("in_gameplay")}" +
                $"was_active_at_level_start={packet.PlayerState.Contains("was_active_at_level_start")}, finished_level={packet.PlayerState.Contains("finished_level")})."
            );

            await sender.PlayerAccessSemaphore.WaitAsync();
            sender.State = packet.PlayerState;
            if (packet.PlayerState.Contains("spectating") != sender.State.Contains("spectating") || packet.PlayerState.Contains("wants_to_play_next_level") != sender.State.Contains("wants_to_play_next_level"))
                _lobbyManager.SpectatingPlayersUpdated = true;
            sender.PlayerAccessSemaphore.Release();
            return;
        }
    }
}
