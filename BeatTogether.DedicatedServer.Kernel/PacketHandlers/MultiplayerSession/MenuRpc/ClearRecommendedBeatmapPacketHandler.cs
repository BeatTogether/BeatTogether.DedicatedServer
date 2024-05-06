﻿using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    class ClearRecommendedBeatmapPacketHandler : BasePacketHandler<ClearRecommendedBeatmapPacket>
    {
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly ILobbyManager _lobbyManager;
        private readonly ILogger _logger = Log.ForContext<ClearRecommendedBeatmapPacketHandler>();

        public ClearRecommendedBeatmapPacketHandler(
            IPacketDispatcher packetDispatcher,
            ILobbyManager lobbyManager)
        {
            _packetDispatcher = packetDispatcher;
            _lobbyManager = lobbyManager;
        }

        public override void Handle(IPlayer sender, ClearRecommendedBeatmapPacket packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(ClearRecommendedBeatmapPacket)}' " +
                $"(SenderId={sender.ConnectionId})."
            );
            sender.BeatmapIdentifier = null;
            sender.SelectedBeatmapPacket = null;
            _packetDispatcher.SendToPlayer(sender, new SetIsStartButtonEnabledPacket
            {
                Reason = sender.IsServerOwner ? CannotStartGameReason.NoSongSelected : CannotStartGameReason.None
            }, IgnoranceChannelTypes.Reliable);
        }
    }
}
