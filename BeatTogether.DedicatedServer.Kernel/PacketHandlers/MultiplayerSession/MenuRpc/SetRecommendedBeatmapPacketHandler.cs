using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using System.Linq;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
	public sealed class SetRecommendedBeatmapPacketHandler : BasePacketHandler<SetRecommendedBeatmapPacket>
	{
		private readonly IPacketDispatcher _packetDispatcher;
		private readonly ILobbyManager _lobbyManager;
		private readonly IPlayerRegistry _playerRegistry;
        private readonly Internal_Logger _logger;

        public SetRecommendedBeatmapPacketHandler(
            IPacketDispatcher packetDispatcher,
            ILobbyManager lobbyManager,
            IPlayerRegistry playerRegistry,
            Kernel_Logger kernel_Logger)
        {
            _packetDispatcher = packetDispatcher;
            _lobbyManager = lobbyManager;
            _playerRegistry = playerRegistry;
            _logger = kernel_Logger.ForContext<SetRecommendedBeatmapPacketHandler>();
        }

        public override void Handle(IPlayer sender, SetRecommendedBeatmapPacket packet)
		{
			_logger.Debug(
				$"Handling packet of type '{nameof(SetRecommendedBeatmapPacket)}' " +
				$"(SenderId={sender.ConnectionId}, LevelId={packet.BeatmapIdentifier.LevelId}, Difficulty={packet.BeatmapIdentifier.Difficulty})."
			);

            if (sender.CanRecommendBeatmaps)
			{
                if(sender.BeatmapIdentifier != null && sender.BeatmapIdentifier.LevelId != packet.BeatmapIdentifier.LevelId)
                {
                    sender.BeatmapDifficultiesRequirements.Clear();
                    sender.MapHash = string.Empty;
                }
				sender.BeatmapIdentifier = packet.BeatmapIdentifier;
                sender.UpdateEntitlement = true;
                //Our custom mpbeatmap packet stuff gets sent anyway
                //TODO apply this logic to all entitlement checks, and check it works well. Might need to send everyones entitlements to a player when they select a map
                _packetDispatcher.SendToPlayers(_playerRegistry.Players.Where(p => p.GetEntitlement(sender.BeatmapIdentifier.LevelId) == Messaging.Enums.EntitlementStatus.Unknown).ToArray(), new GetIsEntitledToLevelPacket
                {
                    LevelId = packet.BeatmapIdentifier.LevelId
                }, IgnoranceChannelTypes.Reliable);
            }
        }
	}
}
