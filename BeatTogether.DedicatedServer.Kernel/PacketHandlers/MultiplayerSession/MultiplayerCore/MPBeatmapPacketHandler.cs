using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    class MpBeatmapPacketHandler : BasePacketHandler<MpBeatmapPacket>
    {
        private readonly ILobbyManager _lobbyManager;
        private readonly ILogger _logger = Log.ForContext<MpBeatmapPacketHandler>();

        public MpBeatmapPacketHandler(
            ILobbyManager lobbyManager)
        {
            _lobbyManager = lobbyManager;
        }

        public override void Handle(IPlayer sender, MpBeatmapPacket packet)
        {

            _logger.Debug(
                $"Handling packet of type '{nameof(MpBeatmapPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            if(sender.BeatmapIdentifier == null)
                sender.BeatmapIdentifier = new BeatmapIdentifier();
            sender.BeatmapIdentifier.LevelId = "custom_level_" + packet.levelHash;
            sender.BeatmapIdentifier.Characteristic = packet.characteristic;
            sender.BeatmapIdentifier.Difficulty = (BeatmapDifficulty)packet.difficulty;

            sender.SelectedBeatmapPacket = packet;

            sender.UpdateEntitlement = true;
        }
    }
}
