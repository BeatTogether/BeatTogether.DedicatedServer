using BeatTogether.LiteNetLib.Abstractions;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IPacketHandler
    {
        void Handle(IPlayer sender, INetSerializable packet);
    }

    public interface IPacketHandler<TPacket> : IPacketHandler
        where TPacket : class, INetSerializable
    {
        void Handle(IPlayer sender, TPacket packet);
    }
}
