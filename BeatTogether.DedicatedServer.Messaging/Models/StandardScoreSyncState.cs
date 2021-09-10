using BeatTogether.Extensions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class StandardScoreSyncState : INetSerializable
    {
        public int ModifiedScore { get; set; }
        public int RawScore { get; set; }
        public int ImmediateMaxPossibleScore { get; set; }
        public int Combo { get; set; }
        public int Multiplier { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            ModifiedScore = reader.GetVarInt();
            RawScore = reader.GetVarInt();
            ImmediateMaxPossibleScore = reader.GetVarInt();
            Combo = reader.GetVarInt();
            Multiplier = reader.GetVarInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutVarInt(ModifiedScore);
            writer.PutVarInt(RawScore);
            writer.PutVarInt(ImmediateMaxPossibleScore);
            writer.PutVarInt(Combo);
            writer.PutVarInt(Multiplier);
        }
    }
}
