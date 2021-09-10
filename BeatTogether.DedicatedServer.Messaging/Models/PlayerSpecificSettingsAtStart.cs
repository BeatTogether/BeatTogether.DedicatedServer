using LiteNetLib.Utils;
using System.Collections.Generic;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class PlayerSpecificSettingsAtStart : INetSerializable
    {
        public List<PlayerSpecificSettings> ActivePlayerSpecificSettingsAtStart { get; set; } = new();

        public void Deserialize(NetDataReader reader)
        {
            int count = reader.GetInt();
            for (int i = 0; i < count; i++)
            {
                PlayerSpecificSettings playerSpecificSettings = new PlayerSpecificSettings();
                playerSpecificSettings.Deserialize(reader);
                ActivePlayerSpecificSettingsAtStart.Add(playerSpecificSettings);
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ActivePlayerSpecificSettingsAtStart.Count);
            foreach(PlayerSpecificSettings playerSpecificSettings in ActivePlayerSpecificSettingsAtStart)
            {
                playerSpecificSettings.Serialize(writer);
            }
        }
    }
}
