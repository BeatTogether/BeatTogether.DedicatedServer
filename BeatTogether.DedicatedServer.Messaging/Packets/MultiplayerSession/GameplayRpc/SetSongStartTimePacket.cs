using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.Legacy;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class SetSongStartTimePacket : BaseRpcWithValuesPacket
    {
        public long StartTime { get; set; }

        public override void ReadFrom(ref SpanBuffer reader, Version version)
        {
            base.ReadFrom(ref reader, version);
            if (HasValue0)
                //if (version < ClientVersions.NewPacketVersion)
                //    StartTime = (long)(reader.ReadFloat32() * 1000f);
                //else
                    StartTime = reader.ReadVarLong();
        }

        public override void WriteTo(ref SpanBuffer writer, Version version)
        {
            base.WriteTo(ref writer, version);
            if (version < ClientVersions.NewPacketVersion)
                writer.WriteFloat32(StartTime / 1000f);
            else
                writer.WriteVarLong(StartTime);
        }
    }
}
