using BeatTogether.DedicatedServer.Messaging.Packets.Legacy;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerLatencyPacket : IVersionedNetSerializable
    {
        public long Latency { get; set; }

        public void ReadFrom(ref SpanBuffer reader, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                Latency = (long)(reader.ReadFloat32() * 1000f);
                return;
            }
            else
            {
                Latency = (long)reader.ReadVarULong();
            }
        }

        public void WriteTo(ref SpanBuffer writer, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                writer.WriteFloat32(Latency / 1000f);
                return;
            }
            else
            {
                writer.WriteVarULong((ulong)Latency);
            }
        }
    }
}
