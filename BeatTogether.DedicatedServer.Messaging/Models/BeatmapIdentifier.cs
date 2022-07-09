using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using Krypton.Buffers;
using System.Collections.Generic;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class BeatmapIdentifier : INetSerializable
    {
        public string LevelId { get; set; } = null!;
        public string Characteristic { get; set; } = null!;
        public BeatmapDifficulty Difficulty { get; set; }

        public bool Chroma { get; set; } = false;
        public bool NoodleExtensions { get; set; } = false;
        public bool MappingExtensions { get; set; } = false;
        public List<uint> Difficulties { get; set; } = new List<uint>();

        public void ReadFrom(ref SpanBufferReader reader)
        {
            LevelId = reader.ReadString();
            Characteristic = reader.ReadString();
            Difficulty = (BeatmapDifficulty)reader.ReadVarUInt();
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteString(LevelId);
            writer.WriteString(Characteristic);
            writer.WriteVarUInt((uint)Difficulty);
        }
    }
}
