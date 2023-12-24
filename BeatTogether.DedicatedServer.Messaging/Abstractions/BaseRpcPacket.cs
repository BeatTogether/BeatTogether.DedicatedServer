using BeatTogether.DedicatedServer.Messaging.Packets.Legacy;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Abstractions
{
    public abstract class BaseRpcPacket : IVersionedNetSerializable
    {
        public long SyncTime { get; set; }

        //public virtual void ReadFrom(ref SpanBuffer reader)
        //{
        //    throw new NotImplementedException("For Versioned Packets only call the versioned ReadFrom function");
        //    //SyncTime = (long)reader.ReadVarULong();
        //}

        public virtual void ReadFrom(ref SpanBuffer reader, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                SyncTime = (long)reader.ReadFloat32() * 1000;
                return;
            }
            else
            {
                SyncTime = (long)reader.ReadVarULong();
                //ReadFrom(ref reader);
            }
        }

        //public virtual void WriteTo(ref SpanBuffer writer)
        //{
        //    throw new NotImplementedException("For Versioned Packets only call the versioned WriteTo function");
        //    //writer.WriteVarULong((ulong)SyncTime);
        //}

        public virtual void WriteTo(ref SpanBuffer writer, Version version)
        {
            if (version < ClientVersions.NewPacketVersion)
            {
                writer.WriteFloat32((float)SyncTime / 1000);
                return;
            }
            else
            {
                writer.WriteVarULong((ulong)SyncTime);
                //WriteTo(ref writer);
            }
        }
    }
}
