using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using LiteNetLib;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
	public sealed class SetRecommendedBeatmapPacketHandler : BasePacketHandler<SetRecommendedBeatmapPacket>
	{
		private readonly IPermissionsManager _permissionsManager;
		private readonly IPacketDispatcher _packetDispatcher;
		private readonly ILogger _logger = Log.ForContext<SetRecommendedBeatmapPacket>();

		public SetRecommendedBeatmapPacketHandler(
			IPermissionsManager permissionsManager,
			IPacketDispatcher packetDispatcher)
		{
			_permissionsManager = permissionsManager;
			_packetDispatcher = packetDispatcher;
		}

		public override Task Handle(IPlayer sender, SetRecommendedBeatmapPacket packet)
		{
			_logger.Debug(
				$"Handling packet of type '{nameof(SetRecommendedBeatmapPacket)}' " +
				$"(SenderId={sender.ConnectionId}, LevelId={packet.BeatmapIdentifier.LevelId}, Difficulty={packet.BeatmapIdentifier.Difficulty})."
			);

			if (_permissionsManager.PlayerCanRecommendBeatmaps(sender.UserId))
			{
				sender.BeatmapIdentifier = packet.BeatmapIdentifier;

				_packetDispatcher.SendToNearbyPlayers(new GetIsEntitledToLevelPacket
				{
					LevelId = packet.BeatmapIdentifier.LevelId
				}, DeliveryMethod.ReliableOrdered);
			}

			return Task.CompletedTask;
		}
	}
}
