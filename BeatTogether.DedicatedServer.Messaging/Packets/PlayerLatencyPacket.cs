using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerLatencyPacket : INetSerializable
    {
        public float Latency { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Latency = reader.GetFloat();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Latency);
        }
    }
}
