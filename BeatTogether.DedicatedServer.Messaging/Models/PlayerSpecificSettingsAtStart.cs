using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;
using System.Collections.Generic;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class PlayerSpecificSettingsAtStart : INetSerializable
    {
        public List<PlayerSpecificSettings> ActivePlayerSpecificSettingsAtStart { get; set; } = new();

        public void ReadFrom(ref SpanBufferReader reader)
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                PlayerSpecificSettings playerSpecificSettings = new PlayerSpecificSettings();
                playerSpecificSettings.ReadFrom(ref reader);
                ActivePlayerSpecificSettingsAtStart.Add(playerSpecificSettings);
            }
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteInt32(ActivePlayerSpecificSettingsAtStart.Count);
            foreach(PlayerSpecificSettings playerSpecificSettings in ActivePlayerSpecificSettingsAtStart)
            {
                playerSpecificSettings.WriteTo(ref writer);
            }
        }
    }
}
