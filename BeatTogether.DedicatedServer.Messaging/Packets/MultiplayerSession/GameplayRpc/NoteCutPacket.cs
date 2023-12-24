using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class NoteCutPacket : BaseRpcWithValuesPacket
    {
        public float SongTime { get; set; }
        public NoteCutInfo Info { get; set; } = new();

        public override void ReadFrom(ref SpanBuffer reader, Version version)
        {
            base.ReadFrom(ref reader, version);
            if (HasValue0)
                SongTime = reader.ReadFloat32();
            if (HasValue1)
                Info.ReadFrom(ref reader);
        }

        public override void WriteTo(ref SpanBuffer writer, Version version)
        {
            base.WriteTo(ref writer, version);
            writer.WriteFloat32(SongTime);
            Info.WriteTo(ref writer);
        }
    }
}
