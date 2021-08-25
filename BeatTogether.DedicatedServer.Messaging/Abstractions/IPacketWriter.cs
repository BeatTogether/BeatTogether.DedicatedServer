using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Abstractions
{
    public interface IPacketWriter
    {
        void WriteTo(NetDataWriter writer, INetSerializable packet);
    }
}
