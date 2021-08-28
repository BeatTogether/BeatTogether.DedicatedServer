using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class PlayerPermissionConfiguration : INetSerializable
    {
        public string? UserId { get; set; }
        public bool IsServerOwner { get; set; }
        public bool HasRecommendBeatmapsPermission { get; set; }
        public bool HasRecommendGameplayModifiersPermission { get; set; }
        public bool HasKickVotePermission { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            UserId = reader.GetString();
            IsServerOwner = reader.GetBool();
            HasRecommendBeatmapsPermission = reader.GetBool();
            HasRecommendGameplayModifiersPermission = reader.GetBool();
            HasKickVotePermission = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserId);
            writer.Put(IsServerOwner);
            writer.Put(HasRecommendBeatmapsPermission);
            writer.Put(HasRecommendGameplayModifiersPermission);
            writer.Put(HasKickVotePermission);
        }
    }
}
