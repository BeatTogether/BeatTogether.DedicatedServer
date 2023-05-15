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
    public sealed class SetIsReadyPacketHandler : BasePacketHandler<SetIsReadyPacket>
    {
        private readonly ILobbyManager _lobbyManager;
        private readonly ILogger _logger = Log.ForContext<SetIsReadyPacketHandler>();
        private readonly IDedicatedInstance _instance;
        private readonly IGameplayManager _gameplayManager;
        private readonly IPacketDispatcher _packetDispatcher;

        public SetIsReadyPacketHandler(
            ILobbyManager lobbyManager,
            IDedicatedInstance dedicatedInstance,
            IGameplayManager gameplayManager,
            IPacketDispatcher packetDispatcher)
        {
            _lobbyManager = lobbyManager;
            _instance = dedicatedInstance;
            _gameplayManager = gameplayManager;
            _packetDispatcher = packetDispatcher;
        }

        public override async Task Handle(IPlayer sender, SetIsReadyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SetIsReadyPacket)}' " +
                $"(SenderId={sender.ConnectionId}, IsReady={packet.IsReady})."
            );
            bool SendToSpectate = false;
            await sender.PlayerAccessSemaphore.WaitAsync();
            sender.IsReady = packet.IsReady;
            //If the player somehow is in the lobby during gameplay then readying should send them to spectate
            SendToSpectate = sender.IsReady && _instance.State == MultiplayerGameState.Game && _gameplayManager.CurrentBeatmap != null && _gameplayManager.State == GameplayManagerState.Gameplay;
            sender.PlayerAccessSemaphore.Release();
            if (SendToSpectate)
                _packetDispatcher.SendToPlayer(sender, new StartLevelPacket
                {
                    Beatmap = _gameplayManager.CurrentBeatmap!,
                    Modifiers = _gameplayManager.CurrentModifiers,
                    StartTime = _instance.RunTime
                }, DeliveryMethod.ReliableOrdered);
        }
    }
}
