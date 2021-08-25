using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class ConnectionRequestData : INetSerializable
    {
        public string? Secret { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public bool IsConnectionOwner { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Secret = reader.GetString();
            UserId = reader.GetString();
            UserName = reader.GetString();
            IsConnectionOwner = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Secret);
            writer.Put(UserId);
            writer.Put(UserName);
            writer.Put(IsConnectionOwner);
        }
    }
}
