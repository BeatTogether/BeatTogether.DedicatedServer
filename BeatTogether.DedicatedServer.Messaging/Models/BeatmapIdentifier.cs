using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class BeatmapIdentifier : INetSerializable
    {
        public string LevelId { get; set; } = null!;
        public string Characteristic { get; set; } = null!;
        public BeatmapDifficulty Difficulty { get; set; }

        public void ReadFrom(ref SpanBuffer reader)
        {
            LevelId = reader.ReadString();
            Characteristic = reader.ReadString();
            Difficulty = (BeatmapDifficulty)reader.ReadVarUInt();
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteString(LevelId);
            writer.WriteString(Characteristic);
            writer.WriteVarUInt((uint)Difficulty);
        }
    }
}
