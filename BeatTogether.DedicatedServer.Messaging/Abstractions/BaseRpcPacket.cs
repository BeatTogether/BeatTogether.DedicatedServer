using BeatTogether.DedicatedServer.Messaging.Packets.Legacy;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Util;
using Serilog;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Abstractions
{
    public abstract class BaseRpcPacket : IVersionedNetSerializable
    {
        public long SyncTime { get; set; }

        private readonly ILogger _logger = Log.ForContext<BaseRpcPacket>();

        public virtual void ReadFrom(ref SpanBuffer reader, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                float readTime = reader.ReadFloat32();
                SyncTime = (long)readTime * 1000;
                _logger.Debug($"Converted time from {readTime} to {SyncTime}");
                return;
            }
            else
            {
                SyncTime = (long)reader.ReadVarULong();
            }
        }

        public virtual void WriteTo(ref SpanBuffer writer, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                writer.WriteFloat32(SyncTime / 1000f);
                return;
            }
            else
            {
                writer.WriteVarULong((ulong)SyncTime);
            }
        }
    }
}
