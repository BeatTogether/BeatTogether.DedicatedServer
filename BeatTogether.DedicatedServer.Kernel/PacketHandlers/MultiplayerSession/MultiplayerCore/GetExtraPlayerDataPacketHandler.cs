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

        public GetMpPlayerDataPacketHandler(
            IPacketDispatcher packetDispatcher,
            IPlayerRegistry playerRegistry)
        {
            _PacketDispatcher = packetDispatcher;
            _PlayerRegistry = playerRegistry;
        }

        public override void Handle(IPlayer sender, MpPlayerData packet)
        {

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
