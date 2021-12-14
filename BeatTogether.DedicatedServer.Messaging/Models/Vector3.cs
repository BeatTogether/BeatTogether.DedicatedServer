using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public struct Vector3 : INetSerializable
    {
        public int x;
        public int y;
        public int z;

        public void ReadFrom(ref SpanBufferReader reader)
        {
            x = reader.ReadVarInt();
            y = reader.ReadVarInt();
            z = reader.ReadVarInt();
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteVarInt(x);
            writer.WriteVarInt(y);
            writer.WriteVarInt(z);
        }
    }
}
