using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class SetSongStartTimePacket : BaseRpcWithValuesPacket
    {
        public long StartTime { get; set; }

        public override void ReadFrom(ref SpanBuffer reader)
        {
            base.ReadFrom(ref reader);
            if (HasValue0)
                StartTime = reader.ReadVarLong();
        }

        public override void WriteTo(ref SpanBuffer writer)
        {
            base.WriteTo(ref writer);
            writer.WriteVarLong(StartTime);
        }
    }
}
