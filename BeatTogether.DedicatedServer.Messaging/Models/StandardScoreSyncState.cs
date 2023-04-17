using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class StandardScoreSyncState : INetSerializable
    {
        public int ModifiedScore { get; set; }
        public int RawScore { get; set; }
        public int ImmediateMaxPossibleScore { get; set; }
        public int Combo { get; set; }
        public int Multiplier { get; set; }

        public void ReadFrom(ref SpanBuffer reader)
        {
            ModifiedScore = reader.ReadVarInt();
            RawScore = reader.ReadVarInt();
            ImmediateMaxPossibleScore = reader.ReadVarInt();
            Combo = reader.ReadVarInt();
            Multiplier = reader.ReadVarInt();
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteVarInt(ModifiedScore);
            writer.WriteVarInt(RawScore);
            writer.WriteVarInt(ImmediateMaxPossibleScore);
            writer.WriteVarInt(Combo);
            writer.WriteVarInt(Multiplier);
        }
        public void ReadFrom(ref MemoryBuffer reader)
        {
            ModifiedScore = reader.ReadVarInt();
            RawScore = reader.ReadVarInt();
            ImmediateMaxPossibleScore = reader.ReadVarInt();
            Combo = reader.ReadVarInt();
            Multiplier = reader.ReadVarInt();
        }

        public void WriteTo(ref MemoryBuffer writer)
        {
            writer.WriteVarInt(ModifiedScore);
            writer.WriteVarInt(RawScore);
            writer.WriteVarInt(ImmediateMaxPossibleScore);
            writer.WriteVarInt(Combo);
            writer.WriteVarInt(Multiplier);
        }
    }
}
