using LiteNetLib.Utils;
using System.Collections.Generic;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class PlayersPermissionConfiguration : INetSerializable
    {
        public List<PlayerPermissionConfiguration> PlayersPermission = new();

        public void Deserialize(NetDataReader reader)
        {
            var length = reader.GetInt();
            for(int i = 0; i < length; i++)
            {
                var permission = new PlayerPermissionConfiguration();
                permission.Deserialize(reader);
                PlayersPermission.Add(permission);
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayersPermission.Count);
            foreach(var permission in PlayersPermission)
            {
                permission.Serialize(writer);
            }
        }
    }
}
