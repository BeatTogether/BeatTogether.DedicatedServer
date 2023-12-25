using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.Legacy;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;
using System;
using Serilog;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession
{
    public sealed class NodePoseSyncStatePacket : IVersionedNetSerializable
    {
        public byte SyncStateId { get; set; }
        public long Time { get; set; }
        public NodePoseSyncState State { get; set; } = new();

        private readonly ILogger _logger = Log.ForContext<NodePoseSyncStatePacket>();

        public void ReadFrom(ref SpanBuffer reader, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                SyncStateId = reader.ReadUInt8();
                float readTime = reader.ReadFloat32();
                long convertedTime = (long)(readTime * 1000f);
                _logger.Verbose($"Converted time from {readTime} to {convertedTime}");
                Time = convertedTime;
                State.ReadFrom(ref reader);
                return;
            }
            else
            {
                //ReadFrom(ref reader);
                SyncStateId = reader.ReadUInt8();
                Time = (long)reader.ReadVarULong();
                _logger.Verbose($"Read time as {Time}");
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
                //WriteTo(ref writer);
                writer.WriteUInt8(SyncStateId);
                writer.WriteVarULong((ulong)Time);
                State.WriteTo(ref writer);
            }
        }
    }
}
