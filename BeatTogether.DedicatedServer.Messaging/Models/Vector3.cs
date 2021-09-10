using BeatTogether.Extensions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public struct Vector3 : INetSerializable
    {
        public int x;
        public int y;
        public int z;

        public void Deserialize(NetDataReader reader)
        {
            x = reader.GetVarInt();
            y = reader.GetVarInt();
            z = reader.GetVarInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutVarInt(x);
            writer.PutVarInt(y);
            writer.PutVarInt(z);
        }
    }
}
