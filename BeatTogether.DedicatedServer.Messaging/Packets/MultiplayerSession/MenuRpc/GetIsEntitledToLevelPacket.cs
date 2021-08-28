using BeatTogether.DedicatedServer.Messaging.Abstractions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetIsEntitledToLevelPacket : BaseRpcPacket
    {
        public string? LevelId { get; set; }

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            LevelId = reader.GetString();
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.Put(LevelId);
        }
    }
}
