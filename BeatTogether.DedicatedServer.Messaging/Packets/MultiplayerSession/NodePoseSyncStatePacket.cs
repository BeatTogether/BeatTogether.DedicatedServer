using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.Legacy;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession
{
    public sealed class NodePoseSyncStatePacket : IVersionedNetSerializable
    {
        public byte SyncStateId { get; set; }
        public long Time { get; set; }
        public NodePoseSyncState State { get; set; } = new();

        public void ReadFrom(ref SpanBuffer reader)
        {
            SyncStateId = reader.ReadUInt8();
            Time = (long)reader.ReadVarULong();
            State.ReadFrom(ref reader);
        }

        public void ReadFrom(ref SpanBuffer reader, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                SyncStateId = reader.ReadUInt8();
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
            if (version < ClientVersions.NewPacketVersion)
            {
                writer.WriteUInt8(SyncStateId);
                //float dividedTime = Time / 1000f;
                //float roundedTime = (float)Math.Round(dividedTime, 4, MidpointRounding.AwayFromZero);
                //_logger.Verbose($"Writing LegacyNodePoseSyncStatePacket Time: {Time}, MultipliedTime: {dividedTime}, RoundedTime: {roundedTime}");
                writer.WriteFloat32(Time / 1000);
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
