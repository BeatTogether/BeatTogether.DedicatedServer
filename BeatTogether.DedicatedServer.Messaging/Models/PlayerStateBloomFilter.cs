using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class PlayerStateBloomFilter : INetSerializable
    {
        public ulong Top { get; set; }
        public ulong Bottom { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Top = reader.GetULong();
            Bottom = reader.GetULong();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Top);
            writer.Put(Bottom);
        }
    }
}
