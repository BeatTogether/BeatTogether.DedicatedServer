using BeatTogether.Extensions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class BeatmapIdentifierNetSerializable : INetSerializable
    {
        public string LevelId { get; set; } = null!;
        public string Characteristic { get; set; } = null!;
        public BeatmapDifficulty Difficulty { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            LevelId = reader.GetString();
            Characteristic = reader.GetString();
            Difficulty = (BeatmapDifficulty)reader.GetVarUInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(LevelId);
            writer.Put(Characteristic);
            writer.PutVarUInt((uint)Difficulty);
        }
    }
}
