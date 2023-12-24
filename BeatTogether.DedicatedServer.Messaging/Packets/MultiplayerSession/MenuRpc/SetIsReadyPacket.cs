using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetIsReadyPacket : BaseRpcWithValuesPacket
    {
        public bool IsReady { get; set; }

        public override void ReadFrom(ref SpanBuffer reader, Version version)
        {
            base.ReadFrom(ref reader, version);
            if (HasValue0)
                IsReady = reader.ReadBool();
        }

        public override void WriteTo(ref SpanBuffer writer, Version version)
        {
            base.WriteTo(ref writer, version);
            writer.WriteBool(IsReady);
        }
    }
}
