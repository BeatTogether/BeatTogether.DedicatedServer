using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
	public sealed class SetRecommendedModifiersPacketHandler : BasePacketHandler<SetRecommendedModifiersPacket>
	{
		private readonly IPacketDispatcher _packetDispatcher;
		private readonly ILobbyManager _lobbyManager;
		private readonly ILogger _logger = Log.ForContext<SetRecommendedModifiersPacketHandler>();

		public SetRecommendedModifiersPacketHandler(
			IPacketDispatcher packetDispatcher,
			ILobbyManager lobbyManager)
		{
			_packetDispatcher = packetDispatcher;
			_lobbyManager = lobbyManager;
		}

		public override void Handle(IPlayer sender, SetRecommendedModifiersPacket packet)
		{
			_logger.Debug(
				$"Handling packet of type '{nameof(SetRecommendedModifiersPacket)}' " +
				$"(SenderId={sender.ConnectionId})."
			);
			if (sender.CanRecommendModifiers)
				sender.Modifiers = packet.Modifiers;
        }
	}
}
