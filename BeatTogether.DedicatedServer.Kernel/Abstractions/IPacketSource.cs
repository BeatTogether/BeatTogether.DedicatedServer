using LiteNetLib;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Kernel.Abstractions
{
    public interface IPacketSource
    {
        void Signal(NetPeer peer, NetDataReader reader, DeliveryMethod deliveryMethod);
    }
}
