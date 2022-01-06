using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class NodePoseSyncState : INetSerializable
    {
        public Pose Head { get; set; }
        public Pose LeftController { get; set; }
        public Pose RightController { get; set; }

        public void ReadFrom(ref SpanBufferReader reader)
        {
            Head.ReadFrom(ref reader);
            LeftController.ReadFrom(ref reader);
            RightController.ReadFrom(ref reader);
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            Head.WriteTo(ref writer);
            LeftController.WriteTo(ref writer);
            RightController.WriteTo(ref writer);
        }
    }
}
