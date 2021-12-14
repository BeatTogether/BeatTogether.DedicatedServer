using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Abstractions
{
    public interface IPacketWriter
    {
        void WriteTo(ref SpanBufferWriter writer, INetSerializable packet);
    }
}
