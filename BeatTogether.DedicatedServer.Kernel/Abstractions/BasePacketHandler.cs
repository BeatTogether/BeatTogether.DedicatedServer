using BeatTogether.LiteNetLib.Abstractions;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public abstract class BasePacketHandler<TPacket> : IPacketHandler<TPacket>
        where TPacket : class, INetSerializable
    {
        public abstract Task Handle(IPlayer sender, TPacket packet);

        public Task Handle(IPlayer sender, INetSerializable packet) =>
            Handle(sender, (TPacket)packet);
    }
}
