using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetIsInLobbyPacket : BaseRpcWithValuesPacket
    {
        public bool IsInLobby { get; set; }

        public override void ReadFrom(ref SpanBuffer reader, Version version)
        {
            base.ReadFrom(ref reader, version);
            if (HasValue0)
                IsInLobby = reader.ReadBool();
        }

        public override void WriteTo(ref SpanBuffer writer, Version version)
        {
            base.WriteTo(ref writer, version);
            writer.WriteBool(IsInLobby);
        }
    }
}
