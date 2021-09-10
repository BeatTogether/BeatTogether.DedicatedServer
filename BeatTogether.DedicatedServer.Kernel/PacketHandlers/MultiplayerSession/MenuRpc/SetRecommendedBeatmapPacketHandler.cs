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
		private readonly IPacketDispatcher _packetDispatcher;
		private readonly ILogger _logger = Log.ForContext<SetRecommendedBeatmapPacket>();

		public SetRecommendedBeatmapPacketHandler(IPacketDispatcher packetDispatcher)
		{
			_packetDispatcher = packetDispatcher;
		}

		public override Task Handle(IPlayer sender, SetRecommendedBeatmapPacket packet)
		{
			_logger.Debug(
				$"Handling packet of type '{nameof(SetRecommendedBeatmapPacket)}' " +
				$"(SenderId={sender.ConnectionId})."
			);

			sender.BeatmapIdentifier = packet.BeatmapIdentifier;
			var setIsStartButtonEnabledPacket = new SetIsStartButtonEnabledPacket
			{
				Reason = CannotStartGameReason.None
			};
			_packetDispatcher.SendToPlayer(sender, setIsStartButtonEnabledPacket, DeliveryMethod.ReliableOrdered);

			var getIsEntitledToLevelPacket = new GetIsEntitledToLevelPacket
			{
				LevelId = packet.BeatmapIdentifier.LevelId
			};
			_packetDispatcher.SendToNearbyPlayers(getIsEntitledToLevelPacket, DeliveryMethod.ReliableOrdered);

			return Task.CompletedTask;
		}
	}
}
