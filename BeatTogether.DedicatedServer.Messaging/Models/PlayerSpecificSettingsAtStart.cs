﻿using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class PlayerSpecificSettingsAtStart : INetSerializable
    {
        public PlayerSpecificSettings[] ActivePlayerSpecificSettingsAtStart { get; set; } = null!;

        public void ReadFrom(ref SpanBuffer reader)
        {
            int count = reader.ReadInt32();
            ActivePlayerSpecificSettingsAtStart = new PlayerSpecificSettings[count];
            for (int i = 0; i < count; i++)
            {
                ActivePlayerSpecificSettingsAtStart[i].ReadFrom(ref reader);
            }
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteInt32(ActivePlayerSpecificSettingsAtStart.Length);
            foreach(PlayerSpecificSettings playerSpecificSettings in ActivePlayerSpecificSettingsAtStart)
            {
                playerSpecificSettings.WriteTo(ref writer);
            }
        }
    }
}
