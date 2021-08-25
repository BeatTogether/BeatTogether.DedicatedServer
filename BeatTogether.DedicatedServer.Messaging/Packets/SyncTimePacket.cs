using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class SyncTimePacket : INetSerializable
    {
        public float SyncTime { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(SyncTime);
        }

        public void Deserialize(NetDataReader reader)
        {
            SyncTime = reader.GetFloat();
        }
    }
}
