using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.PacketHandlers.MultiplayerSession
{
    public sealed class NodePoseSyncStateDeltaPacketHandler : BasePacketHandler<NodePoseSyncStateDeltaPacket>
    {
        public override Task Handle(IPlayer sender, NodePoseSyncStateDeltaPacket packet)
        {
            return Task.CompletedTask;
        }
    }
}
