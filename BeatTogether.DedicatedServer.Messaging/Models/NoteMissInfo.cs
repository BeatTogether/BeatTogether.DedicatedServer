using BeatTogether.Extensions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class NoteMissInfo : INetSerializable
    {
        public int ColorType { get; set; }
        public int NoteLineLayer { get; set; }
        public int NoteLineIndex { get; set; }
        public float NoteTime { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            ColorType = reader.GetVarInt();
            NoteLineLayer = reader.GetVarInt();
            NoteLineIndex = reader.GetVarInt();
            NoteTime = reader.GetFloat();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutVarInt(ColorType);
            writer.PutVarInt(NoteLineLayer);
            writer.PutVarInt(NoteLineIndex);
            writer.Put(NoteTime);
        }
    }
}
