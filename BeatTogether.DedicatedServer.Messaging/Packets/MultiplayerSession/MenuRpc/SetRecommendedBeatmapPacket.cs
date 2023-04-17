using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetRecommendedBeatmapPacket : BaseRpcWithValuesPacket
    {
        public BeatmapIdentifier BeatmapIdentifier { get; set; } = new();

        public override void ReadFrom(ref SpanBuffer reader)
        {
            base.ReadFrom(ref reader);
            if (HasValue0)
                BeatmapIdentifier.ReadFrom(ref reader);
        }

        public override void WriteTo(ref SpanBuffer writer)
        {
            base.WriteTo(ref writer);
            BeatmapIdentifier.WriteTo(ref writer);
        }
        public override void ReadFrom(ref MemoryBuffer reader)
        {
            base.ReadFrom(ref reader);
            if (HasValue0)
                BeatmapIdentifier.ReadFrom(ref reader);
        }

        public override void WriteTo(ref MemoryBuffer writer)
        {
            base.WriteTo(ref writer);
            BeatmapIdentifier.WriteTo(ref writer);
        }
    }
}
