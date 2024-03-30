using BeatTogether.LiteNetLib.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public abstract class BasePacketHandler<TPacket> : IPacketHandler<TPacket>
        where TPacket : class, INetSerializable
    {
        public abstract void Handle(IPlayer sender, TPacket packet);

        public void Handle(IPlayer sender, INetSerializable packet) =>
            Handle(sender, (TPacket)packet);
    }
}
