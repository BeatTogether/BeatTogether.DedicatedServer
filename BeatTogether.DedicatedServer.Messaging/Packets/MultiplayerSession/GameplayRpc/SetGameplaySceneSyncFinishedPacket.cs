﻿using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class SetGameplaySceneSyncFinishedPacket : BaseRpcWithValuesPacket
    {
        public PlayerSpecificSettingsAtStart PlayersAtStart { get; set; } = null!;
        public string SessionGameId { get; set; } = null!;

        public override void ReadFrom(ref SpanBuffer reader, Version version)
        {
            base.ReadFrom(ref reader, version);
            if (HasValue0)
                PlayersAtStart.ReadFrom(ref reader);
            if (HasValue1)
                SessionGameId = reader.ReadString();
        }

        public override void WriteTo(ref SpanBuffer writer, Version version)
        {
            base.WriteTo(ref writer, version);
            PlayersAtStart.WriteTo(ref writer);
            writer.WriteString(SessionGameId);
        }
    }
}
