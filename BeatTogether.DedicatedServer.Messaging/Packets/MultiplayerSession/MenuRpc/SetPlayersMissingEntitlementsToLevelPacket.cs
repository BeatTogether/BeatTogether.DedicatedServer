using BeatTogether.DedicatedServer.Messaging.Abstractions;
using LiteNetLib.Utils;
using System.Collections.Generic;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetPlayersMissingEntitlementsToLevelPacket : BaseRpcPacket
    {
        public List<string> PlayersWithoutEntitlements { get; set; } = new();

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            int count = reader.GetInt();
            for (int i = 0; i < count; i++)
            {
                PlayersWithoutEntitlements.Add(reader.GetString());
            }
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.Put(PlayersWithoutEntitlements.Count);
            foreach (string player in PlayersWithoutEntitlements)
            {
                writer.Put(player);
            }
        }
    }
}
