using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;
using Serilog;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class NodePoseSyncState : INetSerializable
    {
        public Pose Head { get; set; } = new();
        public Pose LeftController { get; set; } = new();
        public Pose RightController { get; set; } = new();


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

        public override string ToString()
        {
            return $"(Head: {Head}, LeftController: {LeftController}, RightController: {RightController})";
        }
    }
}
