using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.LiteNetLib.Extensions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc
{
    public sealed class SetGameplaySceneSyncFinishedPacket : BaseRpcPacket
    {
        public PlayerSpecificSettingsAtStart PlayersAtStart { get; set; } = new();
        public string SessionGameId { get; set; } = null!;

        public override void ReadFrom(ref SpanBufferReader reader)
        {
            base.ReadFrom(ref reader);
            PlayersAtStart.ReadFrom(ref reader);
            SessionGameId = reader.ReadString();
        }

        public override void WriteTo(ref SpanBufferWriter writer)
        {
            base.WriteTo(ref writer);
            PlayersAtStart.WriteTo(ref writer);
            writer.WriteString(SessionGameId);
        }
    }
}
