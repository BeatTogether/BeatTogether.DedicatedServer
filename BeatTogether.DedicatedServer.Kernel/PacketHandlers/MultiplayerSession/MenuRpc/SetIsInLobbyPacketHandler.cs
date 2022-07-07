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
        private readonly PacketDispatcher _packetDispatcher;
        private readonly ILogger _logger = Log.ForContext<SetIsInLobbyPacketHandler>();

        public SetIsInLobbyPacketHandler(
            IDedicatedInstance instance,
            IPlayerRegistry playerRegistry,
            IGameplayManager gameplayManager,
            PacketDispatcher packetDispatcher)
        {
            _instance = instance;
            _playerRegistry = playerRegistry;
            _gameplayManager = gameplayManager;
            _packetDispatcher = packetDispatcher;
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
                    if(_instance.State == MultiplayerGameState.Game && !(_gameplayManager.State == Enums.GameplayManagerState.None || _gameplayManager.State == Enums.GameplayManagerState.Results))
                    {
                        _gameplayManager.HandlePlayerLeaveGameplay(sender);
                        _packetDispatcher.SendToPlayer(sender, new StartLevelPacket
                        {
                            Beatmap = _gameplayManager.CurrentBeatmap!,
                            Modifiers = _gameplayManager.CurrentModifiers,
                            StartTime = _instance.RunTime
                        }, DeliveryMethod.ReliableOrdered);
                        return Task.CompletedTask;
                    }
                    _packetDispatcher.SendToPlayer(sender, new SetIsStartButtonEnabledPacket
                    {
                        Reason = CannotStartGameReason.NoSongSelected
                    }, DeliveryMethod.ReliableOrdered);
                }
                sender.InLobby = packet.IsInLobby;

                if (_instance.State == MultiplayerGameState.Game && packet.IsInLobby == true && _playerRegistry.Players.TrueForAll(p => p.InLobby))
                    _gameplayManager.SignalRequestReturnToMenu();
            }
            return Task.CompletedTask;
        }
    }
}
