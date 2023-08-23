using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class NoteMissInfo : INetSerializable
    {
        public int ColorType { get; set; }
        public int NoteLineLayer { get; set; }
        public int NoteLineIndex { get; set; }
        public float NoteTime { get; set; }

        public void ReadFrom(ref SpanBuffer reader)
        {
            ColorType = reader.ReadVarInt();
            NoteLineLayer = reader.ReadVarInt();
            NoteLineIndex = reader.ReadVarInt();
            NoteTime = reader.ReadFloat32();
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteVarInt(ColorType);
            writer.WriteVarInt(NoteLineLayer);
            writer.WriteVarInt(NoteLineIndex);
            writer.WriteFloat32(NoteTime);
        }
    }
}
