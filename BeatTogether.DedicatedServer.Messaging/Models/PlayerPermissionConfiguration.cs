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
        public bool HasInvitePermission { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            UserId = reader.GetString();
            byte num = reader.GetByte();
            IsServerOwner = (num & 1) > 0;
            HasRecommendBeatmapsPermission = (num & 2) > 0;
            HasRecommendGameplayModifiersPermission = (num & 4) > 0;
            HasKickVotePermission = (num & 8) > 0;
            HasInvitePermission = (num & 16) > 0;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserId);
            int num = (IsServerOwner ? 1 : 0) | (HasRecommendBeatmapsPermission ? 2 : 0) | (HasRecommendGameplayModifiersPermission ? 4 : 0) | (HasKickVotePermission ? 8 : 0) | (HasInvitePermission ? 16 : 0);
            writer.Put((byte)num);
        }
    }
}
