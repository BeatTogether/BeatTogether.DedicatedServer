using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using BeatTogether.LiteNetLib.Enums;
using Serilog;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class SetIsInLobbyPacketHandler : BasePacketHandler<SetIsInLobbyPacket>
    {
        private readonly IDedicatedInstance _instance;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IGameplayManager _gameplayManager;
        private readonly ILobbyManager _lobbyManager;
        private readonly PacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<SetIsInLobbyPacketHandler>();

        public SetIsInLobbyPacketHandler(
            IDedicatedInstance instance,
            IPlayerRegistry playerRegistry,
            IGameplayManager gameplayManager,
            PacketDispatcher packetDispatcher,
            ILobbyManager lobbyManager)
        {
            _instance = instance;
            _playerRegistry = playerRegistry;
            _gameplayManager = gameplayManager;
            _packetDispatcher = packetDispatcher;
            _lobbyManager = lobbyManager;
        }

        public override async Task Handle(IPlayer sender, SetIsInLobbyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SetIsInLobbyPacket)}' " +
                $"(SenderId={sender.ConnectionId}, InLobby={packet.IsInLobby})."
            );
            await sender.PlayerAccessSemaphore.WaitAsync();
            bool InLobby = sender.InLobby;
            if (packet.IsInLobby == sender.InLobby)
                return;
            sender.InLobby = packet.IsInLobby;
            sender.PlayerAccessSemaphore.Release();
            if (InLobby)
            {
                _packetDispatcher.SendToPlayer(sender, new SetIsStartButtonEnabledPacket
                {
                    Reason = sender.IsServerOwner ? _lobbyManager.GetCannotStartGameReason(sender, _lobbyManager.DoesEveryoneOwnBeatmap) : CannotStartGameReason.None
                }, DeliveryMethod.ReliableOrdered);

                if (_lobbyManager.SelectedBeatmap is null)
                    _packetDispatcher.SendToPlayer(sender, new ClearSelectedBeatmap(), DeliveryMethod.ReliableOrdered);
                else
                    _packetDispatcher.SendToPlayer(sender, new SetSelectedBeatmap
                    {
                        Beatmap = _lobbyManager.SelectedBeatmap
                    }, DeliveryMethod.ReliableOrdered);

                if (_lobbyManager.SelectedModifiers == _lobbyManager.EmptyModifiers)
                    _packetDispatcher.SendToPlayer(sender, new ClearSelectedGameplayModifiers(), DeliveryMethod.ReliableOrdered);
                else
                    _packetDispatcher.SendToPlayer(sender, new SetSelectedGameplayModifiers
                    {
                        Modifiers = _lobbyManager.SelectedModifiers
                    }, DeliveryMethod.ReliableOrdered);
            }
            return;
        }
    }
}
