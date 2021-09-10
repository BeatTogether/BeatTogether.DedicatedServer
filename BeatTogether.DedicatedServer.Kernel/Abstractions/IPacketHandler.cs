using System.Threading.Tasks;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IPacketHandler
    {
        Task Handle(IPlayer sender, INetSerializable packet);
    }

    public interface IPacketHandler<TPacket> : IPacketHandler
        where TPacket : class, INetSerializable
    {
        Task Handle(IPlayer sender, TPacket packet);
    }
}
