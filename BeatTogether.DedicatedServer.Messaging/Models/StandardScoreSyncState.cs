using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class StandardScoreSyncState : INetSerializable
    {
        public int ModifiedScore { get; set; }
        public int RawScore { get; set; }
        public int ImmediateMaxPossibleScore { get; set; }
        public int Combo { get; set; }
        public int Multiplier { get; set; }

        public void ReadFrom(ref SpanBufferReader reader)
        {
            ModifiedScore = reader.ReadVarInt();
            RawScore = reader.ReadVarInt();
            ImmediateMaxPossibleScore = reader.ReadVarInt();
            Combo = reader.ReadVarInt();
            Multiplier = reader.ReadVarInt();
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteVarInt(ModifiedScore);
            writer.WriteVarInt(RawScore);
            writer.WriteVarInt(ImmediateMaxPossibleScore);
            writer.WriteVarInt(Combo);
            writer.WriteVarInt(Multiplier);
        }
    }
}
