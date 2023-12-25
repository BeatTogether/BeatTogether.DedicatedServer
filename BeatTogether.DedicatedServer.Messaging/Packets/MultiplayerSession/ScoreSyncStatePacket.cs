using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.Legacy;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession
{
    public sealed class ScoreSyncStatePacket : IVersionedNetSerializable
    {
        public byte SyncStateId { get; set; }
        public long Time { get; set; }
        public StandardScoreSyncState State { get; set; } = new();

        public void ReadFrom(ref SpanBuffer reader, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                SyncStateId = reader.ReadUInt8();
                Time = (long)(reader.ReadFloat32() * 1000f);
                State.ReadFrom(ref reader);
                return;
            }
            else
            {
                SyncStateId = reader.ReadUInt8();
                Time = (long)reader.ReadVarULong();
                State.ReadFrom(ref reader);
            }
        }

        public void WriteTo(ref SpanBuffer writer, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                writer.WriteUInt8(SyncStateId);
                writer.WriteFloat32(Time / 1000f);
                State.WriteTo(ref writer);
                return;
            }
            else
            {
                writer.WriteUInt8(SyncStateId);
                writer.WriteVarULong((ulong)Time);
                State.WriteTo(ref writer);
            }
        }
    }
}
