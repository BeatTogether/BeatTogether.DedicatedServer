using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;

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
        public NoteGameplayType GameplayType { get; set; }
        public int ColorType { get; set; }
        public int LineLayer { get; set; }
        public int NoteLineIndex { get; set; }
        public float NoteTime { get; set; }
        public float TimeToNextNote { get; set; }
        public Vector3 MoveVec { get; set; }

        public void ReadFrom(ref SpanBuffer reader)
        {
            CutWasOk = reader.ReadUInt8();
            SaberSpeed = reader.ReadFloat32();
            SaberDir.ReadFrom(ref reader);
            CutPoint.ReadFrom(ref reader);
            CutNormal.ReadFrom(ref reader);
            NotePosition.ReadFrom(ref reader);
            NoteScale.ReadFrom(ref reader);
            NoteRotation.ReadFrom(ref reader);
            GameplayType = (NoteGameplayType) reader.ReadVarInt();
            ColorType = reader.ReadVarInt();
            LineLayer = reader.ReadVarInt();
            NoteLineIndex = reader.ReadVarInt();
            NoteTime = reader.ReadFloat32();
            TimeToNextNote = reader.ReadFloat32();
            MoveVec.ReadFrom(ref reader);
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteUInt8(CutWasOk);
            writer.WriteFloat32(SaberSpeed);
            SaberDir.WriteTo(ref writer);
            CutPoint.WriteTo(ref writer);
            CutNormal.WriteTo(ref writer);
            NotePosition.WriteTo(ref writer);
            NoteScale.WriteTo(ref writer);
            NoteRotation.WriteTo(ref writer);
            writer.WriteVarInt((int)GameplayType);
            writer.WriteVarInt(ColorType);
            writer.WriteVarInt(LineLayer);
            writer.WriteVarInt(NoteLineIndex);
            writer.WriteFloat32(NoteTime);
            writer.WriteFloat32(TimeToNextNote);
            MoveVec.WriteTo(ref writer);
        }
        public void ReadFrom(ref MemoryBuffer reader)
        {
            CutWasOk = reader.ReadUInt8();
            SaberSpeed = reader.ReadFloat32();
            SaberDir.ReadFrom(ref reader);
            CutPoint.ReadFrom(ref reader);
            CutNormal.ReadFrom(ref reader);
            NotePosition.ReadFrom(ref reader);
            NoteScale.ReadFrom(ref reader);
            NoteRotation.ReadFrom(ref reader);
            GameplayType = (NoteGameplayType) reader.ReadVarInt();
            ColorType = reader.ReadVarInt();
            LineLayer = reader.ReadVarInt();
            NoteLineIndex = reader.ReadVarInt();
            NoteTime = reader.ReadFloat32();
            TimeToNextNote = reader.ReadFloat32();
            MoveVec.ReadFrom(ref reader);
        }

        public void WriteTo(ref MemoryBuffer writer)
        {
            writer.WriteUInt8(CutWasOk);
            writer.WriteFloat32(SaberSpeed);
            SaberDir.WriteTo(ref writer);
            CutPoint.WriteTo(ref writer);
            CutNormal.WriteTo(ref writer);
            NotePosition.WriteTo(ref writer);
            NoteScale.WriteTo(ref writer);
            NoteRotation.WriteTo(ref writer);
            writer.WriteVarInt((int)GameplayType);
            writer.WriteVarInt(ColorType);
            writer.WriteVarInt(LineLayer);
            writer.WriteVarInt(NoteLineIndex);
            writer.WriteFloat32(NoteTime);
            writer.WriteFloat32(TimeToNextNote);
            MoveVec.WriteTo(ref writer);
        }
    }
}
