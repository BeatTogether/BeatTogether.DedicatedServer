using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using BeatTogether.LiteNetLib.Enums;
using Serilog;
using System;
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

        public override Task Handle(IPlayer sender, SetIsInLobbyPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(SetIsInLobbyPacket)}' " +
                $"(SenderId={sender.ConnectionId}, InLobby={packet.IsInLobby})."
            );
            lock (sender.InLobbyLock)
            {
                if (packet.IsInLobby && !sender.InLobby)
                {
                    _packetDispatcher.SendToPlayer(sender, new SetIsStartButtonEnabledPacket
                    {
                        Reason = CannotStartGameReason.NoSongSelected
                    }, DeliveryMethod.ReliableOrdered);
                }
                sender.InLobby = packet.IsInLobby;

                //if (_instance.State == MultiplayerGameState.Game && packet.IsInLobby == true && _playerRegistry.Players.TrueForAll(p => p.InLobby))
                //    _gameplayManager.SignalRequestReturnToMenu(); //TODO set the game to lobby if all players are in lobby?


                //If your not the lobby manager then the selecteed beatmap dissapears
                if (sender.InLobby && !sender.IsServerOwner)
                {
                    if (_lobbyManager.SelectedBeatmap is not null)
                        //_packetDispatcher.SendToPlayer(sender, new ClearSelectedBeatmap(), DeliveryMethod.ReliableOrdered);
                    //else
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
            }
            return Task.CompletedTask;
        }
    }
}
