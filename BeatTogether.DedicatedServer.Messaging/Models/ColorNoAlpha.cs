using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class ColorNoAlpha : INetSerializable
    {
        public float r { get; set; }
        public float g { get; set; }
        public float b { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            r = reader.GetFloat();
            g = reader.GetFloat();
            b = reader.GetFloat();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(r);
            writer.Put(g);
            writer.Put(b);
        }
    }
}
