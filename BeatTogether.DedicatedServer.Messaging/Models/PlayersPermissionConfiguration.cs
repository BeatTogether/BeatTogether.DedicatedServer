using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class PlayersPermissionConfiguration : INetSerializable
    {
        public PlayerPermissionConfiguration[] PlayersPermission = null!;

        public void ReadFrom(ref SpanBuffer reader)
        {
            PlayersPermission = new PlayerPermissionConfiguration[reader.ReadInt32()];
            for (int i = 0; i < PlayersPermission.Length; i++)
            {
                PlayersPermission[i].ReadFrom(ref reader);
            }
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteInt32(PlayersPermission.Length);
            for (int i = 0; i < PlayersPermission.Length; i++)
            {
                PlayersPermission[i].WriteTo(ref writer);
            }
        }
    }
}
