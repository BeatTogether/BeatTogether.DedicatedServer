using BeatTogether.DedicatedServer.Messaging.Abstractions;
using Krypton.Buffers;
using System.Collections.Generic;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetPlayersMissingEntitlementsToLevelPacket : BaseRpcPacket
    {
        public List<string> PlayersWithoutEntitlements { get; set; } = new();

        public override void ReadFrom(ref SpanBufferReader reader)
        {
            base.ReadFrom(ref reader);
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                PlayersWithoutEntitlements.Add(reader.ReadUTF8String());
            }
        }

        public override void WriteTo(ref SpanBufferWriter writer)
        {
            base.WriteTo(ref writer);
            writer.WriteInt32(PlayersWithoutEntitlements.Count);
            foreach (string player in PlayersWithoutEntitlements)
            {
                writer.WriteUTF8String(player);
            }
        }
    }
}
