using BeatTogether.DedicatedServer.Kernel.Abstractions;
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
            if (_instance.State == MultiplayerGameState.Game)
            {
                if (_gameplayManager.CurrentBeatmap != null && _gameplayManager.CurrentModifiers != null)
                    _packetDispatcher.SendToPlayer(sender, new StartLevelPacket
                    {
                        Beatmap = _gameplayManager.CurrentBeatmap,
                        Modifiers = _gameplayManager.CurrentModifiers,
                        StartTime = _instance.RunTime
                    }, DeliveryMethod.ReliableOrdered);
            }
            else
			{
                if (_lobbyManager.SelectedBeatmap != null)
                {
                    _packetDispatcher.SendToPlayer(sender, new GetIsEntitledToLevelPacket
                    {
                        LevelId = _lobbyManager.SelectedBeatmap.LevelId
                    }, DeliveryMethod.ReliableOrdered);
                }
                else
                    _packetDispatcher.SendToPlayer(sender, new CancelLevelStartPacket(), DeliveryMethod.ReliableOrdered);
			}

            return Task.CompletedTask;
        }
    }
}
