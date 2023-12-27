using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;
using Krypton.Buffers;
using Serilog;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public class Pose : INetSerializable
    {
        public Vector3 Position { get; set; } = new();
        public Quaternion Rotation { get; set; } = new();

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

        public override string ToString()
        {
            return $"(Position: {Position}, Rotation: {Rotation})";
        }
    }
}
