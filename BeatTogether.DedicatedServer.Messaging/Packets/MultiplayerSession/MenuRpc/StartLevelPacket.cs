using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.Legacy;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class StartLevelPacket : BaseRpcWithValuesPacket
    {
        public BeatmapIdentifier Beatmap { get; set; } = new();
        public GameplayModifiers Modifiers { get; set; } = new();
        public long StartTime { get; set; }

        public override void ReadFrom(ref SpanBuffer reader, Version version)
        {
            base.ReadFrom(ref reader, version);
            if (HasValue0)
                Beatmap.ReadFrom(ref reader);
            if (HasValue1)
                Modifiers.ReadFrom(ref reader);
            if (HasValue2)
                if (version < ClientVersions.NewPacketVersion)
                    StartTime = (long)(reader.ReadFloat32() * 1000f);
                else
                    StartTime = reader.ReadVarLong();
        }

        public override void WriteTo(ref SpanBuffer writer, Version version)
        {
            base.WriteTo(ref writer, version);
            Beatmap.WriteTo(ref writer);
            Modifiers.WriteTo(ref writer);
            if (version < ClientVersions.NewPacketVersion)
                writer.WriteFloat32(StartTime / 1000f);
            else
                writer.WriteVarLong(StartTime);
        }
    }
}
