using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Extensions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    class GetMpPlayerDataPacketHandler : BasePacketHandler<MpPlayerData>
    {
        private readonly IPacketDispatcher _PacketDispatcher;
        private readonly IPlayerRegistry _PlayerRegistry;
        private readonly Internal_Logger _logger;

        public GetMpPlayerDataPacketHandler(
            IPacketDispatcher packetDispatcher,
            IPlayerRegistry playerRegistry,
            Kernel_Logger kernel_Logger)
        {
            _PacketDispatcher = packetDispatcher;
            _PlayerRegistry = playerRegistry;
            _logger = kernel_Logger.ForContext<GetMpPlayerDataPacketHandler>();
        }

        public override void Handle(IPlayer sender, MpPlayerData packet)
        {
            _logger.Debug(
                $"Handling packet of type '{nameof(MpPlayerData)}' " +
                $"(SenderId={sender.ConnectionId})."
            );

            foreach (var Player in _PlayerRegistry.Players)
            {
                _PacketDispatcher.SendFromPlayerToPlayer(Player, sender, new MpPlayerData()
                {
                    PlatformID = Player.PlatformUserId,
                    Platform = Player.PlayerPlatform.Convert(),
                    ClientVersion = Player.PlayerClientVersion.ToString(),
                }, IgnoranceChannelTypes.Reliable);
            }
        }
    }
}
