using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Messaging.Util;

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
    }
}
