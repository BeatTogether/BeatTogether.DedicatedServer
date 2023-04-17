using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class SetPlayerDidConnectLatePacket : BaseRpcWithValuesPacket
    {
        public string UserId { get; set; } = null!;
        public PlayerSpecificSettingsAtStart PlayersAtStart { get; set; } = new();
        public string SessionGameId { get; set; } = null!;

        public override void ReadFrom(ref SpanBuffer reader)
        {
            base.ReadFrom(ref reader);
            if (HasValue0)
                UserId = reader.ReadString();
            if (HasValue1)
                PlayersAtStart.ReadFrom(ref reader);
            if (HasValue2)
                SessionGameId = reader.ReadString();
        }

        public override void WriteTo(ref SpanBuffer writer)
        {
            base.WriteTo(ref writer);
            writer.WriteString(UserId);
            PlayersAtStart.WriteTo(ref writer);
            writer.WriteString(SessionGameId);
        }
        public override void ReadFrom(ref MemoryBuffer reader)
        {
            base.ReadFrom(ref reader);
            if (HasValue0)
                UserId = reader.ReadString();
            if (HasValue1)
                PlayersAtStart.ReadFrom(ref reader);
            if (HasValue2)
                SessionGameId = reader.ReadString();
        }

        public override void WriteTo(ref MemoryBuffer writer)
        {
            base.WriteTo(ref writer);
            writer.WriteString(UserId);
            PlayersAtStart.WriteTo(ref writer);
            writer.WriteString(SessionGameId);
        }
    }
}
