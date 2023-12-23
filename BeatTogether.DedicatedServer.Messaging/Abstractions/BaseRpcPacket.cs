using BeatTogether.DedicatedServer.Messaging.Packets.Legacy;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Abstractions
{
    public abstract class BaseRpcPacket : IVersionedNetSerializable
    {
        public long SyncTime { get; set; }

        public virtual void ReadFrom(ref SpanBuffer reader)
        {
            SyncTime = (long)reader.ReadVarULong();
        }

        public virtual void ReadFrom(ref SpanBuffer reader, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                SyncTime = (long)reader.ReadFloat32() * 1000;
                return;
            }
            else
            {
                ReadFrom(ref reader);
            }
        }

        public virtual void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteVarULong((ulong)SyncTime);
        }

        public virtual void WriteTo(ref SpanBuffer writer, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                writer.WriteFloat32((float)SyncTime / 1000);
                return;
            }
            else
            {
                WriteTo(ref writer);
            }
        }
        public virtual void ReadFrom(ref MemoryBuffer reader)
        {
            SyncTime = (long)reader.ReadVarULong();
        }

        public virtual void WriteTo(ref MemoryBuffer writer)
        {
            writer.WriteVarULong((ulong)SyncTime);
        }
    }
}
