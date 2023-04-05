using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class PlayersPermissionConfiguration : INetSerializable
    {
        public PlayerPermissionConfiguration[] PlayersPermission = Array.Empty<PlayerPermissionConfiguration>();

        public void ReadFrom(ref SpanBufferReader reader)
        {
            PlayersPermission = new PlayerPermissionConfiguration[reader.ReadInt32()];
            for (int i = 0; i < PlayersPermission.Length; i++)
            {
                var permission = new PlayerPermissionConfiguration();
                permission.ReadFrom(ref reader);
                PlayersPermission[i] = permission;
            }
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteInt32(PlayersPermission.Length);
            for (int i = 0; i < PlayersPermission.Length; i++)
            {
                PlayersPermission[i].WriteTo(ref writer);
            }
        }
    }
}
