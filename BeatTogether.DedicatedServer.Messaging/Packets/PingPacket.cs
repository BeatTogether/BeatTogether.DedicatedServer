using BeatTogether.DedicatedServer.Messaging.Packets.Legacy;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PingPacket : IVersionedNetSerializable
    {
        public long PingTime { get; set; }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteVarULong((ulong)PingTime);
        }

        public void WriteTo(ref SpanBuffer writer, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                writer.WriteFloat32((float)PingTime / 1000f);
                return;
            }
            else
            {
                WriteTo(ref writer);
            }
        }

        public void ReadFrom(ref SpanBuffer reader, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                PingTime = (long)(reader.ReadFloat32() * 1000);
                return;
            }
            else
            {
                ReadFrom(ref reader);
            }
        }

        public void ReadFrom(ref SpanBuffer reader)
        {
            PingTime = (long)reader.ReadVarULong();
        }
    }
}
