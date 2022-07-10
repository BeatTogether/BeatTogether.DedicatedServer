using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using BeatTogether.LiteNetLib.Enums;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
	public sealed class SetRecommendedBeatmapPacketHandler : BasePacketHandler<SetRecommendedBeatmapPacket>
	{
		private readonly IPacketDispatcher _packetDispatcher;
		private readonly ILobbyManager _lobbyManager;
		private readonly ILogger _logger = Log.ForContext<SetRecommendedBeatmapPacket>();

		public SetRecommendedBeatmapPacketHandler(
			IPacketDispatcher packetDispatcher,
			ILobbyManager lobbyManager)
		{
			_packetDispatcher = packetDispatcher;
			_lobbyManager = lobbyManager;
		}

		public override Task Handle(IPlayer sender, SetRecommendedBeatmapPacket packet)
		{
			_logger.Debug(
				$"Handling packet of type '{nameof(SetRecommendedBeatmapPacket)}' " +
				$"(SenderId={sender.ConnectionId}, LevelId={packet.BeatmapIdentifier.LevelId}, Difficulty={packet.BeatmapIdentifier.Difficulty})."
			);

            lock (sender.BeatmapLock)
            {
				if (sender.CanRecommendBeatmaps)
				{
					sender.BeatmapIdentifier.Characteristic = packet.BeatmapIdentifier.Characteristic;
					sender.BeatmapIdentifier.LevelId = packet.BeatmapIdentifier.LevelId;
					sender.BeatmapIdentifier.Difficulty = packet.BeatmapIdentifier.Difficulty;
					_packetDispatcher.SendToNearbyPlayers(new GetIsEntitledToLevelPacket
					{
						LevelId = packet.BeatmapIdentifier.LevelId
					}, DeliveryMethod.ReliableOrdered);
				}
			}
			return Task.CompletedTask;
		}
	}
}
