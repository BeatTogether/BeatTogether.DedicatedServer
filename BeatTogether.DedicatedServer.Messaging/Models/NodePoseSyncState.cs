using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class NodePoseSyncState : INetSerializable
    {
        public Pose Head { get; set; }
        public Pose LeftController { get; set; }
        public Pose RightController { get; set; }

        public void ReadFrom(ref SpanBuffer reader)
        {
            Head.ReadFrom(ref reader);
            LeftController.ReadFrom(ref reader);
            RightController.ReadFrom(ref reader);
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            Head.WriteTo(ref writer);
            LeftController.WriteTo(ref writer);
            RightController.WriteTo(ref writer);
        }
    }
}
