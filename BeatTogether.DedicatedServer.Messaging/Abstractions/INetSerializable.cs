using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Abstractions
{
    public interface INetSerializable
    {
        void WriteTo(ref SpanBuffer bufferWriter);

        void ReadFrom(ref SpanBuffer bufferReader);
    }
}
