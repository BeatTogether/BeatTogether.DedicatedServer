using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public class Quaternion : INetSerializable
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

        public override string ToString()
        {
            return $"(a: {a}, b: {b}, c: {c})";
        }
    }
}
