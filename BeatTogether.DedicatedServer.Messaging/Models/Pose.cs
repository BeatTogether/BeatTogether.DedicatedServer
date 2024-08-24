using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public struct Pose : INetSerializable
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        public void ReadFrom(ref SpanBuffer reader)
        {
            Position.ReadFrom(ref reader);
            Rotation.ReadFrom(ref reader);
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            Position.WriteTo(ref writer);
            Rotation.WriteTo(ref writer);
        }
    }
}
