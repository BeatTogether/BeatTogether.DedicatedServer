using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class BeatmapIdentifier : INetSerializable
    {
        public string LevelId { get; set; } = null!;
        public string Characteristic { get; set; } = null!;
        public BeatmapDifficulty Difficulty { get; set; }

        public void ReadFrom(ref SpanBufferReader reader)
        {
            LevelId = reader.ReadUTF8String();
            Characteristic = reader.ReadUTF8String();
            Difficulty = (BeatmapDifficulty)reader.ReadVarUInt();
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteUTF8String(LevelId);
            writer.WriteUTF8String(Characteristic);
            writer.WriteVarUInt((uint)Difficulty);
        }
    }
}
