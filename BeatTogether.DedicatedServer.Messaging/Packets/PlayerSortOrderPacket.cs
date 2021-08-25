using BeatTogether.Extensions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerSortOrderPacket : INetSerializable
    {
        public string? UserId { get; set; }
        public int SortIndex { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            UserId = reader.GetString();
            SortIndex = reader.GetVarInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserId);
            writer.PutVarInt(SortIndex);
        }
    }
}
