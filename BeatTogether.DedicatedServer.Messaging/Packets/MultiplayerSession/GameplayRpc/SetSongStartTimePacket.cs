using BeatTogether.DedicatedServer.Messaging.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class SetSongStartTimePacket : BaseRpcWithValuesPacket
    {
        public float StartTime { get; set; }

        public override void ReadFrom(ref SpanBufferReader reader)
        {
            base.ReadFrom(ref reader);
            if (HasValue0)
                StartTime = reader.ReadFloat32();
        }

        public override void WriteTo(ref SpanBufferWriter writer)
        {
            base.WriteTo(ref writer);
            writer.WriteFloat32(StartTime);
        }
    }
}
