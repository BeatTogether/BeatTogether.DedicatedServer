using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class PlayerPermissionConfiguration : INetSerializable
    {
        public string UserId { get; set; } = null!;
        public bool IsServerOwner { get; set; }
        public bool HasRecommendBeatmapsPermission { get; set; }
        public bool HasRecommendGameplayModifiersPermission { get; set; }
        public bool HasKickVotePermission { get; set; }
        public bool HasInvitePermission { get; set; }

        public void ReadFrom(ref SpanBuffer reader)
        {
            UserId = reader.ReadString();
            byte num = reader.ReadUInt8();
            IsServerOwner = (num & 1) > 0;
            HasRecommendBeatmapsPermission = (num & 2) > 0;
            HasRecommendGameplayModifiersPermission = (num & 4) > 0;
            HasKickVotePermission = (num & 8) > 0;
            HasInvitePermission = (num & 16) > 0;
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteString(UserId);
            int num = (IsServerOwner ? 1 : 0) | (HasRecommendBeatmapsPermission ? 2 : 0) | (HasRecommendGameplayModifiersPermission ? 4 : 0) | (HasKickVotePermission ? 8 : 0) | (HasInvitePermission ? 16 : 0);
            writer.WriteUInt8((byte)num);
        }
    }
}
