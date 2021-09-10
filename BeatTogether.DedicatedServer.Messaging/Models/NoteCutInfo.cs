using BeatTogether.Extensions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class NoteCutInfo : INetSerializable
    {
        public byte CutWasOk { get; set; }
        public float SaberSpeed { get; set; }
        public Vector3 SaberDir { get; set; }
        public Vector3 CutPoint { get; set; }
        public Vector3 CutNormal { get; set; }
        public Vector3 NotePosition { get; set; }
        public Vector3 NoteScale { get; set; }
        public Quaternion NoteRotation { get; set; }
        public int ColorType { get; set; }
        public int NoteLineLayer { get; set; }
        public int NoteLineIndex { get; set; }
        public float NoteTime { get; set; }
        public float TimeToNextNote { get; set; }
        public Vector3 MoveVec { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            CutWasOk = reader.GetByte();
            SaberSpeed = reader.GetFloat();
            SaberDir.Deserialize(reader);
            CutPoint.Deserialize(reader);
            CutNormal.Deserialize(reader);
            NotePosition.Deserialize(reader);
            NoteScale.Deserialize(reader);
            NoteRotation.Deserialize(reader);
            ColorType = reader.GetVarInt();
            NoteLineLayer = reader.GetVarInt();
            NoteLineIndex = reader.GetVarInt();
            NoteTime = reader.GetFloat();
            TimeToNextNote = reader.GetFloat();
            MoveVec.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CutWasOk);
            writer.Put(SaberSpeed);
            SaberDir.Serialize(writer);
            CutPoint.Serialize(writer);
            CutNormal.Serialize(writer);
            NotePosition.Serialize(writer);
            NoteScale.Serialize(writer);
            NoteRotation.Serialize(writer);
            writer.PutVarInt(ColorType);
            writer.PutVarInt(NoteLineLayer);
            writer.PutVarInt(NoteLineIndex);
            writer.Put(NoteTime);
            writer.Put(TimeToNextNote);
            MoveVec.Serialize(writer);
        }
    }
}
