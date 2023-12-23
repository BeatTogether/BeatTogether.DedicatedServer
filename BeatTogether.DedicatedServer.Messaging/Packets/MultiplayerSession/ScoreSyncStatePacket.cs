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

        public void ReadFrom(ref SpanBuffer reader)
        {
            SyncStateId = reader.ReadUInt8();
            Time = (long)reader.ReadVarULong();
            State.ReadFrom(ref reader);
        }

        public void ReadFrom(ref SpanBuffer reader, Version version)
        {
            SyncStateId = reader.ReadUInt8();
            if (version < ClientVersions.NewPacketVersion)
            {
                Time = (long)reader.ReadFloat32() * 1000;
                State.ReadFrom(ref reader);
                return;
            }
            else
            {
                ReadFrom(ref reader);
            }
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteUInt8(SyncStateId);
            writer.WriteVarULong((ulong)Time);
            State.WriteTo(ref writer);
        }

        public void WriteTo(ref SpanBuffer writer, Version version)
        {
            writer.WriteUInt8(SyncStateId);
            if (version < ClientVersions.NewPacketVersion)
            {
                writer.WriteFloat32((float)Time / 1000);
                State.WriteTo(ref writer);
                return;
            }
            else
            {
                WriteTo(ref writer);
            }
        }
    }
}
