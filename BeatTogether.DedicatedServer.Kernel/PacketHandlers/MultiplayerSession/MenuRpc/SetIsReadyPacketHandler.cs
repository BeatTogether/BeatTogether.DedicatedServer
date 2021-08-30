using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession.MenuRpc
{
    public sealed class SetIsReadyPacketHandler : BasePacketHandler<SetIsReadyPacket>
    {
        public override Task Handle(IPlayer sender, SetIsReadyPacket packet)
        {
            sender.IsReady = packet.IsReady;
            return Task.CompletedTask;
        }
    }
}
