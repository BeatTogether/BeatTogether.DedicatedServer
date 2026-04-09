using BeatTogether.Core.Enums;
using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetCountdownEndTimePacketHandler : BasePacketHandler<GetCountdownEndTimePacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILobbyManager _lobbyManager;
        private readonly Internal_Logger _logger;

        public GetCountdownEndTimePacketHandler(
            ILobbyManager lobbyManager,
            IPacketDispatcher packetDispatcher,
            Kernel_Logger kernel_Logger)
        {
            _lobbyManager = lobbyManager;
            _packetDispatcher = packetDispatcher;
            _logger = kernel_Logger.ForContext<GetCountdownEndTimePacketHandler>();
        }

        public override void Handle(IPlayer sender, GetCountdownEndTimePacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetCountdownEndTimePacket)}' " +
                $"(SenderId={sender.ConnectionId} CountdownTime={_lobbyManager.CountdownEndTime})."
            );
            if(_lobbyManager.CountDownState == CountdownState.CountingDown)
                _packetDispatcher.SendToPlayer(sender, new SetCountdownEndTimePacket
                {
                    CountdownTime = _lobbyManager.CountdownEndTime
                }, IgnoranceChannelTypes.Reliable);

        }
    }
}
