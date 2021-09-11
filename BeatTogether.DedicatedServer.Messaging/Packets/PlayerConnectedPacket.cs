using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets
{
    public sealed class PlayerConnectedPacket : INetSerializable
    {
        public byte RemoteConnectionId { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public bool IsConnectionOwner { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            RemoteConnectionId = reader.GetByte();
            UserId = reader.GetString();
            UserName = reader.GetString();
            IsConnectionOwner = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(RemoteConnectionId);
            writer.Put(UserId);
            writer.Put(UserName);
            writer.Put(IsConnectionOwner);
        }
    }
}
