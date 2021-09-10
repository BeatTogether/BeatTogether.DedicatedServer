using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.Extensions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetIsEntitledToLevelPacket : BaseRpcPacket
    {
        public string LevelId { get; set; } = null!;
        public EntitlementStatus Entitlement { get; set; }

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            LevelId = reader.GetString();
            Entitlement = (EntitlementStatus)reader.GetVarInt();
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.Put(LevelId);
            writer.PutVarInt((int)Entitlement);
        }
    }
}
