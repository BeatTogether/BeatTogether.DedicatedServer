using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public struct Quaternion : INetSerializable
    {
        public int a;
        public int b;
        public int c;

        public void ReadFrom(ref SpanBuffer reader)
        {
            a = reader.ReadVarInt();
            b = reader.ReadVarInt();
            c = reader.ReadVarInt();
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteVarInt(a);
            writer.WriteVarInt(b);
            writer.WriteVarInt(c);
        }
    }
}
