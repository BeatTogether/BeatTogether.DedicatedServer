using System.Collections.Generic;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetPlayersMissingEntitlementsToLevelPacket : BaseRpcWithValuesPacket
    {
        public List<string> PlayersWithoutEntitlements { get; set; } = new();

        public override void ReadFrom(ref SpanBufferReader reader)
        {
            base.ReadFrom(ref reader);

            if (HasValue0)
            {
                // PlayersMissingEntitlementsNetSerializable
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    PlayersWithoutEntitlements.Add(reader.ReadString());
                }
            }
        }

        public override void WriteTo(ref SpanBufferWriter writer)
        {
            base.WriteTo(ref writer);
            
            // PlayersMissingEntitlementsNetSerializable
            writer.WriteInt32(PlayersWithoutEntitlements.Count);
            foreach (string player in PlayersWithoutEntitlements)
            {
                writer.WriteString(player);
            }
        }
    }
}
