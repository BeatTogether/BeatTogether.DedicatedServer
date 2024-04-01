using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class NoteCutPacket : BaseRpcWithValuesPacket
    {
        public float SongTime { get; set; }
        public NoteCutInfo Info { get; set; } = new();

        public override void ReadFrom(ref SpanBuffer reader)
        {
            base.ReadFrom(ref reader);
            if (HasValue0)
                SongTime = reader.ReadFloat32();
            if (HasValue1)
                Info.ReadFrom(ref reader);
        }

        public override void WriteTo(ref SpanBuffer writer)
        {
            base.WriteTo(ref writer);
            writer.WriteFloat32(SongTime);
            Info.WriteTo(ref writer);
        }
    }
}
