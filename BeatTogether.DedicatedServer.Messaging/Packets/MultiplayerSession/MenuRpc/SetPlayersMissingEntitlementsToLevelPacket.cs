using System.Collections.Generic;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetPlayersMissingEntitlementsToLevelPacket : BaseRpcPacket
    {
        public List<string> PlayersWithoutEntitlements { get; set; } = new();

        public override void ReadFrom(ref SpanBufferReader reader)
        {
            base.ReadFrom(ref reader);

            if (reader.ReadUInt8() == 1)
            {
                // PlayersMissingEntitlementsNetSerializable
                var count = reader.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    PlayersWithoutEntitlements.Add(reader.ReadString());
                }
            }
        }

        public override void WriteTo(ref SpanBufferWriter writer)
        {
            base.WriteTo(ref writer);

            writer.WriteUInt8(1);
            // PlayersMissingEntitlementsNetSerializable
            writer.WriteInt32(PlayersWithoutEntitlements.Count);
            foreach (var player in PlayersWithoutEntitlements)
            {
                writer.WriteString(player);
            }
        }
    }
}