using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    class MpBeatmapPacketHandler : BasePacketHandler<MpBeatmapPacket>
    {
        private readonly ILobbyManager _lobbyManager;
        private readonly Internal_Logger _logger;

        public MpBeatmapPacketHandler(
            ILobbyManager lobbyManager,
            Kernel_Logger kernel_Logger)
        {
            _lobbyManager = lobbyManager;
            _logger = kernel_Logger.ForContext<MpBeatmapPacketHandler>();
        }

        public override void Handle(IPlayer sender, MpBeatmapPacket packet)
        {

            _logger.Debug(
                $"Handling packet of type '{nameof(MpBeatmapPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            sender.MapHash = packet.levelHash;

            if(sender.BeatmapIdentifier == null)
                sender.BeatmapIdentifier = new BeatmapIdentifier();
            sender.BeatmapIdentifier.LevelId = "custom_level_" + packet.levelHash;
            sender.BeatmapIdentifier.Characteristic = packet.characteristic;
            sender.BeatmapIdentifier.Difficulty = (BeatmapDifficulty)packet.difficulty;
            sender.BeatmapDifficultiesRequirements = packet.requirements;

            sender.UpdateEntitlement = true;
        }
    }
}
