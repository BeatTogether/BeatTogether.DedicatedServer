using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Abstractions
{
    public abstract class BaseRpcPacket : INetSerializable
    {
        public float SyncTime { get; set; }

        public virtual void ReadFrom(ref SpanBuffer reader)
        {
            SyncTime = reader.ReadFloat32();
        }

        public virtual void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteFloat32(SyncTime);
        }
        public virtual void ReadFrom(ref MemoryBuffer reader)
        {
            SyncTime = reader.ReadFloat32();
        }

        public virtual void WriteTo(ref MemoryBuffer writer)
        {
            writer.WriteFloat32(SyncTime);
        }
    }
}
