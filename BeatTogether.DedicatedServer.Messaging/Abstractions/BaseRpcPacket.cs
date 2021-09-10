using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Abstractions
{
    public abstract class BaseRpcPacket : INetSerializable
    {
        public float SyncTime { get; set; }

        public virtual void Deserialize(NetDataReader reader)
        {
            SyncTime = reader.GetFloat();
        }

        public virtual void Serialize(NetDataWriter writer)
        {
            writer.Put(SyncTime);
        }
    }
}
