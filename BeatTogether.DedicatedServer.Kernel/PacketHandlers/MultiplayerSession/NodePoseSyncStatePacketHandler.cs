using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession
{
    public sealed class NodePoseSyncStatePacketHandler : BasePacketHandler<NodePoseSyncStatePacket>
    {
        public override Task Handle(IPlayer sender, NodePoseSyncStatePacket packet)
        {
            return Task.CompletedTask;
        }
    }
}
