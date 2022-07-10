using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

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

        public override Task Handle(IPlayer sender, MpBeatmapPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(MpBeatmapPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            lock (sender.BeatmapLock)
            {
                sender.BeatmapIdentifier.Chroma = packet.requirements[packet.difficulty].Contains("Chroma");
                sender.BeatmapIdentifier.NoodleExtensions = packet.requirements[packet.difficulty].Contains("Noodle Extensions");
                sender.BeatmapIdentifier.MappingExtensions = packet.requirements[packet.difficulty].Contains("Mapping Extensions");
                sender.BeatmapIdentifier.Difficulties = packet.requirements.Keys.ToList();
            }
            return Task.CompletedTask;
        }
    }
}
