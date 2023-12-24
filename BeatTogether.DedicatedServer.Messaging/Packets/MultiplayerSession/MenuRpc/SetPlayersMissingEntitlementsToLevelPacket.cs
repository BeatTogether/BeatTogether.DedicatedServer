using System;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
    public sealed class SetPlayersMissingEntitlementsToLevelPacket : BaseRpcWithValuesPacket
    {
        public string[] PlayersWithoutEntitlements { get; set; } = Array.Empty<string>();

        public override void ReadFrom(ref SpanBuffer reader, Version version)
        {
            base.ReadFrom(ref reader, version);

            if (HasValue0)
            {
                PlayersWithoutEntitlements = new string[reader.ReadInt32()];
                for (int i = 0; i < PlayersWithoutEntitlements.Length; i++)
                {
                    PlayersWithoutEntitlements[i] = reader.ReadString();
                }
            }
        }

        public override void WriteTo(ref SpanBuffer writer, Version version)
        {
            base.WriteTo(ref writer, version);
            
            writer.WriteInt32(PlayersWithoutEntitlements.Length);
            foreach (string player in PlayersWithoutEntitlements)
            {
                writer.WriteString(player);
            }
        }
    }
}
