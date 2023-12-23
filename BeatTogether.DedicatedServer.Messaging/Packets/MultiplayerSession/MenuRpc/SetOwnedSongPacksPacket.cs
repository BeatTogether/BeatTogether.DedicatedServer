using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.Legacy;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetOwnedSongPacksPacket : BaseRpcWithValuesPacket
    {
        public SongPackMask SongPackMask { get; set; } = new();

        public override void ReadFrom(ref SpanBuffer reader)
        {
            base.ReadFrom(ref reader);
            if (HasValue0)
                SongPackMask.ReadFrom(ref reader);
        }

        public override void ReadFrom(ref SpanBuffer reader, Version version)
        {
            base.ReadFrom(ref reader, version);
            if (version < ClientVersions.NewPacketVersion)
            {
                SongPackMask.D0 = ulong.MaxValue;
                SongPackMask.D1 = ulong.MaxValue;
                SongPackMask.D2 = ulong.MaxValue;
                SongPackMask.D3 = ulong.MaxValue;
                reader.SkipBytes(16);
                return;
            }
            if (HasValue0)
                SongPackMask.ReadFrom(ref reader); // TODO: Convert to LegacySongPackMask
        }

        public override void WriteTo(ref SpanBuffer writer)
        {
            base.WriteTo(ref writer);
            if (HasValue0)
                SongPackMask.WriteTo(ref writer);
        }

        public override void WriteTo(ref SpanBuffer writer, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                writer.WriteUInt64(ulong.MaxValue);
                writer.WriteUInt64(ulong.MaxValue);
                return;
            }
            base.WriteTo(ref writer, version);
            if (HasValue0)
                SongPackMask.WriteTo(ref writer);  // TODO: Convert to LegacySongPackMask
        }
    }
}
