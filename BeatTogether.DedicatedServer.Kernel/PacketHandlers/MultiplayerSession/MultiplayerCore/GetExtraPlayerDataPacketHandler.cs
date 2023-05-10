using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;
using BeatTogether.LiteNetLib.Enums;
using System.Threading.Tasks;

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

        public override Task Handle(IPlayer sender, MpPlayerData packet)
        {

            foreach (var Player in _PlayerRegistry.Players)
            {
                _PacketDispatcher.SendFromPlayerToPlayer(Player, sender, new MpPlayerData()
                {
                    Platform = (byte)Player.Platform,
                    ClientVersion = Player.ClientVersion
                }, DeliveryMethod.ReliableOrdered);
            }
            return Task.CompletedTask;
        }
    }
}
