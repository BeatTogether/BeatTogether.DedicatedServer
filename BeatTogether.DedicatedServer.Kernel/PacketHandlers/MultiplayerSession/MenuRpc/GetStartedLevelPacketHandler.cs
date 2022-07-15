using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using BeatTogether.LiteNetLib.Enums;
using Serilog;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class GetStartedLevelPacketHandler : BasePacketHandler<GetStartedLevelPacket>
    {
        private readonly IDedicatedInstance _instance;
        private readonly ILobbyManager _lobbyManager;
        private readonly IGameplayManager _gameplayManager;
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<GetStartedLevelPacketHandler>();

        public GetStartedLevelPacketHandler(
            IDedicatedInstance instance, 
            ILobbyManager lobbyManager,
            IGameplayManager gameplayManager, 
            IPacketDispatcher packetDispatcher)
        {
            _instance = instance;
            _lobbyManager = lobbyManager;
            _gameplayManager = gameplayManager;
            _packetDispatcher = packetDispatcher;
        }

        public override Task Handle(IPlayer sender, GetStartedLevelPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(GetStartedLevelPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            if (_lobbyManager.SelectedBeatmap != null && _instance.State != MultiplayerGameState.Game)
            {
                _packetDispatcher.SendToPlayer(sender, new GetIsEntitledToLevelPacket
                {
                    LevelId = _lobbyManager.SelectedBeatmap.LevelId
                }, DeliveryMethod.ReliableOrdered);
            }
            _packetDispatcher.SendToPlayer(sender, new CancelLevelStartPacket(), DeliveryMethod.ReliableOrdered);

            return Task.CompletedTask;
        }
    }
}
