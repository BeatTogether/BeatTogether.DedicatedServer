using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public struct Pose : INetSerializable
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Position.Deserialize(reader);
            Rotation.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            Position.Serialize(writer);
            Rotation.Serialize(writer);
        }
    }
}
