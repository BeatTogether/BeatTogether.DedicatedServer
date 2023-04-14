using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class PlayersPermissionConfiguration : INetSerializable
    {
        public PlayerPermissionConfiguration[] PlayersPermission = null!;

        public void ReadFrom(ref SpanBufferReader reader)
        {
            PlayersPermission = new PlayerPermissionConfiguration[reader.ReadInt32()];
            for (int i = 0; i < PlayersPermission.Length; i++)
            {
                PlayersPermission[i].ReadFrom(ref reader);
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
