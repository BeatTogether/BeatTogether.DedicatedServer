using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public struct Vector3 : INetSerializable
    {
        public int x;
        public int y;
        public int z;

        public void ReadFrom(ref SpanBuffer reader)
        {
            x = reader.ReadVarInt();
            y = reader.ReadVarInt();
            z = reader.ReadVarInt();
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteVarInt(x);
            writer.WriteVarInt(y);
            writer.WriteVarInt(z);
        }
        public void ReadFrom(ref MemoryBuffer reader)
        {
            x = reader.ReadVarInt();
            y = reader.ReadVarInt();
            z = reader.ReadVarInt();
        }

        public void WriteTo(ref MemoryBuffer writer)
        {
            writer.WriteVarInt(x);
            writer.WriteVarInt(y);
            writer.WriteVarInt(z);
        }
    }
}
