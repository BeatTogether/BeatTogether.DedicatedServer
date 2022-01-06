using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Abstractions
{
    public abstract class BaseRpcPacket : INetSerializable
    {
        public float SyncTime { get; set; }

        public virtual void ReadFrom(ref SpanBufferReader reader)
        {
            SyncTime = reader.ReadFloat32();
        }

        public virtual void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteFloat32(SyncTime);
        }
    }
}
