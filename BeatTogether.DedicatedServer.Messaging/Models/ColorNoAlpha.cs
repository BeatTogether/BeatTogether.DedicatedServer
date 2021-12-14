using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class ColorNoAlpha : INetSerializable
    {
        public float r { get; set; }
        public float g { get; set; }
        public float b { get; set; }

        public void ReadFrom(ref SpanBufferReader reader)
        {
            r = reader.ReadFloat32();
            g = reader.ReadFloat32();
            b = reader.ReadFloat32();
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteFloat32(r);
            writer.WriteFloat32(g);
            writer.WriteFloat32(b);
        }
    }
}
