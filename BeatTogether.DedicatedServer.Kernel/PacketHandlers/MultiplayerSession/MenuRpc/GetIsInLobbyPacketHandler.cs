using BeatTogether.Core.Enums;
using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetIsInLobbyPacketHandler : BasePacketHandler<GetIsInLobbyPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly IDedicatedInstance _instance;
        private readonly IGameplayManager _gameplayManager;
        private readonly Internal_Logger _logger;

        public GetIsInLobbyPacketHandler(
            IPacketDispatcher packetDispatcher,
            IDedicatedInstance instance,
            IGameplayManager gameplayManager,
            Kernel_Logger kernel_Logger)
        {
            _packetDispatcher = packetDispatcher;
            _instance = instance;
            _gameplayManager = gameplayManager;
            _logger = kernel_Logger.ForContext<GetIsInLobbyPacketHandler>();
        }

        public override void Handle(IPlayer sender, GetIsInLobbyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetIsInLobbyPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            _packetDispatcher.SendToPlayer(sender, new SetIsInLobbyPacket
            {
                IsInLobby = _instance.State != MultiplayerGameState.Game
            }, IgnoranceChannelTypes.Reliable);

        }
    }
}
