using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetIsInLobbyPacketHandler : BasePacketHandler<GetIsInLobbyPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly IDedicatedInstance _instance;
        private readonly ILogger _logger = Log.ForContext<GetIsInLobbyPacketHandler>();
        private readonly IGameplayManager _gameplayManager;

        public GetIsInLobbyPacketHandler(
            IPacketDispatcher packetDispatcher, IDedicatedInstance instance, IGameplayManager gameplayManager)
        {
            _packetDispatcher = packetDispatcher;
            _instance = instance;
            _gameplayManager = gameplayManager;
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
