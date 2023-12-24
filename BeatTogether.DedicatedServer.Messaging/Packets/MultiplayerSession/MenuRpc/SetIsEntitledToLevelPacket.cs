using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetIsEntitledToLevelPacket : BaseRpcWithValuesPacket
    {
        public string LevelId { get; set; } = null!;
        public EntitlementStatus Entitlement { get; set; }

        public override void ReadFrom(ref SpanBuffer reader, Version version)
        {
            base.ReadFrom(ref reader, version);
            if (HasValue0)
                LevelId = reader.ReadString();
            if (HasValue1)
                Entitlement = (EntitlementStatus)reader.ReadVarInt();
        }

        public override void WriteTo(ref SpanBuffer writer, Version version)
        {
            base.WriteTo(ref writer, version);
            writer.WriteString(LevelId);
            writer.WriteVarInt((int)Entitlement);
        }
    }
}
