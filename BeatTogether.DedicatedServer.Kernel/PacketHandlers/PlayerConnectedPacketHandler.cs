using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers
{
	public sealed class PlayerConnectedPacketHandler : BasePacketHandler<PlayerConnectedPacket>
	{
		private ILogger _logger = Log.ForContext<PlayerConnectedPacketHandler>();

		public override Task Handle(IPlayer sender, PlayerConnectedPacket packet)
		{
			_logger.Debug(
				$"Handling packet of type '{nameof(PlayerConnectedPacket)}' " +
				$"(SenderId={sender.ConnectionId})"
			);

			sender.RemoteConnectionId = packet.RemoteConnectionId;

			return Task.CompletedTask;
		}
	}
}
