using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;
using System.Collections.Generic;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class PlayersPermissionConfiguration : INetSerializable
    {
        public List<PlayerPermissionConfiguration> PlayersPermission = new();

        public void ReadFrom(ref SpanBufferReader reader)
        {
            var length = reader.ReadInt32();
            for(int i = 0; i < length; i++)
            {
                var permission = new PlayerPermissionConfiguration();
                permission.ReadFrom(ref reader);
                PlayersPermission.Add(permission);
            }
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteInt32(PlayersPermission.Count);
            foreach(var permission in PlayersPermission)
            {
                permission.WriteTo(ref writer);
            }
        }
    }
}
