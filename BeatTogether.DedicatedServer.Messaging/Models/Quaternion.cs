using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;

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
        public void ReadFrom(ref MemoryBuffer reader)
        {
            a = reader.ReadVarInt();
            b = reader.ReadVarInt();
            c = reader.ReadVarInt();
        }

        public void WriteTo(ref MemoryBuffer writer)
        {
            writer.WriteVarInt(a);
            writer.WriteVarInt(b);
            writer.WriteVarInt(c);
        }
    }
}
