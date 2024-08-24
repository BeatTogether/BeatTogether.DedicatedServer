using System;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetPlayersMissingEntitlementsToLevelPacket : BaseRpcWithValuesPacket
    {
        public string[] PlayersWithoutEntitlements { get; set; } = Array.Empty<string>();

        public override void ReadFrom(ref SpanBuffer reader)
        {
            base.ReadFrom(ref reader);

            if (HasValue0)
            {
                PlayersWithoutEntitlements = new string[reader.ReadInt32()];
                for (int i = 0; i < PlayersWithoutEntitlements.Length; i++)
                {
                    PlayersWithoutEntitlements[i] = reader.ReadString();
                }
            }
        }

        public override void WriteTo(ref SpanBuffer writer)
        {
            base.WriteTo(ref writer);
            
            writer.WriteInt32(PlayersWithoutEntitlements.Length);
            foreach (string player in PlayersWithoutEntitlements)
            {
                writer.WriteString(player);
            }
        }
    }
}
