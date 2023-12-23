using BeatTogether.DedicatedServer.Messaging.Packets.Legacy;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerLatencyPacket : IVersionedNetSerializable
    {
        public long Latency { get; set; }
        public void ReadFrom(ref SpanBuffer reader)
        {
            Latency = (long)reader.ReadVarULong();
        }

        public void ReadFrom(ref SpanBuffer reader, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                Latency = (long)reader.ReadFloat32() * 1000;
                return;
            }
            else
            {
                ReadFrom(ref reader);
            }
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteVarULong((ulong)Latency);
        }

        public void WriteTo(ref SpanBuffer writer, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                writer.WriteFloat32((float)Latency / 1000);
                return;
            }
            else
            {
                WriteTo(ref writer);
            }
        }
    }
}
