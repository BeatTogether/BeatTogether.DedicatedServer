using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class NoteMissPacket : BaseRpcWithValuesPacket
    {
        public float SongTime { get; set; }
        public NoteMissInfo Info { get; set; } = new();

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
        public override void ReadFrom(ref MemoryBuffer reader)
        {
            base.ReadFrom(ref reader);
            if (HasValue0)
                SongTime = reader.ReadFloat32();
            if (HasValue1)
                Info.ReadFrom(ref reader);
        }

        public override void WriteTo(ref MemoryBuffer writer)
        {
            base.WriteTo(ref writer);
            writer.WriteFloat32(SongTime);
            Info.WriteTo(ref writer);
        }
    }
}
