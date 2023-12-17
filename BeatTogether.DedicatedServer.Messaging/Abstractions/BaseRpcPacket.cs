using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Abstractions
{
    public abstract class BaseRpcPacket : INetSerializable
    {
        public long SyncTime { get; set; }

        public virtual void ReadFrom(ref SpanBuffer reader)
        {
            SyncTime = (long)reader.ReadVarULong();
        }

        public virtual void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteVarULong((ulong)SyncTime);
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
