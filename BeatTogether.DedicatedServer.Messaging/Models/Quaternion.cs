using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public struct Quaternion : INetSerializable
    {
        public int a;
        public int b;
        public int c;

        public void ReadFrom(ref SpanBufferReader reader)
        {
            a = reader.ReadVarInt();
            b = reader.ReadVarInt();
            c = reader.ReadVarInt();
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteVarInt(a);
            writer.WriteVarInt(b);
            writer.WriteVarInt(c);
        }
    }
}
