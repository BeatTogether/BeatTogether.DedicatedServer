using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;
using System;
using System.Collections.Generic;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class PlayerSpecificSettingsAtStart : INetSerializable
    {
        public PlayerSpecificSettings[] ActivePlayerSpecificSettingsAtStart { get; set; } = null!;

        public void ReadFrom(ref SpanBufferReader reader)
        {
            int count = reader.ReadInt32();
            ActivePlayerSpecificSettingsAtStart = new PlayerSpecificSettings[count-1];
            for (int i = 0; i < count; i++)
            {
                PlayerSpecificSettings playerSpecificSettings = new();
                playerSpecificSettings.ReadFrom(ref reader);
                ActivePlayerSpecificSettingsAtStart[i] = playerSpecificSettings;
            }
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteInt32(ActivePlayerSpecificSettingsAtStart.Length);
            foreach(PlayerSpecificSettings playerSpecificSettings in ActivePlayerSpecificSettingsAtStart)
            {
                playerSpecificSettings.WriteTo(ref writer);
            }
        }
    }
}
