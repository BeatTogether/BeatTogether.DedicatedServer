using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.LiteNetLib.Extensions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class SetPlayerDidConnectLatePacket : BaseRpcPacket
    {
        public string UserId { get; set; } = null!;
        public PlayerSpecificSettingsAtStart PlayersAtStart { get; set; } = new();
        public string SessionGameId { get; set; } = null!;

        public override void ReadFrom(ref SpanBufferReader reader)
        {
            base.ReadFrom(ref reader);
            UserId = reader.ReadString();
            PlayersAtStart.ReadFrom(ref reader);
            SessionGameId = reader.ReadString();
        }

        public override void WriteTo(ref SpanBufferWriter writer)
        {
            base.WriteTo(ref writer);
            writer.WriteString(UserId);
            PlayersAtStart.WriteTo(ref writer);
            writer.WriteString(SessionGameId);
        }
    }
}
