using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
	public sealed class SetRecommendedBeatmapPacketHandler : BasePacketHandler<SetRecommendedBeatmapPacket>
	{
		private readonly IPacketDispatcher _packetDispatcher;
		private readonly ILobbyManager _lobbyManager;
		private readonly IPlayerRegistry _playerRegistry;
		private readonly ILogger _logger = Log.ForContext<SetRecommendedBeatmapPacketHandler>();

		public SetRecommendedBeatmapPacketHandler(
            IPacketDispatcher packetDispatcher,
            ILobbyManager lobbyManager,
            IPlayerRegistry playerRegistry)
        {
            _packetDispatcher = packetDispatcher;
            _lobbyManager = lobbyManager;
            _playerRegistry = playerRegistry;
        }

        public override void Handle(IPlayer sender, SetRecommendedBeatmapPacket packet)
		{
			_logger.Debug(
				$"Handling packet of type '{nameof(SetRecommendedBeatmapPacket)}' " +
				$"(SenderId={sender.ConnectionId}, LevelId={packet.BeatmapIdentifier.LevelId}, Difficulty={packet.BeatmapIdentifier.Difficulty})."
			);

            if (sender.CanRecommendBeatmaps)
			{
				sender.BeatmapIdentifier = packet.BeatmapIdentifier;
				if (sender.BeatmapIdentifier.LevelId != sender.MapHash)
					sender.ResetRecommendedMapRequirements();
                sender.UpdateEntitlement = true;
                _packetDispatcher.SendToNearbyPlayers(new GetIsEntitledToLevelPacket
                {
                    LevelId = packet.BeatmapIdentifier.LevelId
                }, IgnoranceChannelTypes.Reliable);
            }
        }
	}
}
