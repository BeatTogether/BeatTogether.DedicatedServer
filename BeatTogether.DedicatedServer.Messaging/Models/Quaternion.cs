using BeatTogether.Extensions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public struct Quaternion : INetSerializable
    {
        public int a;
        public int b;
        public int c;

        public void Deserialize(NetDataReader reader)
        {
            a = reader.GetVarInt();
            b = reader.GetVarInt();
            c = reader.GetVarInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutVarInt(a);
            writer.PutVarInt(b);
            writer.PutVarInt(c);
        }
    }
}
