using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public struct Pose : INetSerializable
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        public void ReadFrom(ref SpanBufferReader reader)
        {
            Position.ReadFrom(ref reader);
            Rotation.ReadFrom(ref reader);
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            Position.WriteTo(ref writer);
            Rotation.WriteTo(ref writer);
        }
    }
}
