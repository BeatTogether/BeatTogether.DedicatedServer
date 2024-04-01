using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class MultiplayerLevelCompletionResults : INetSerializable
    {
        public MultiplayerPlayerLevelEndState PlayerLevelEndState { get; set; }
        public MultiplayerPlayerLevelEndReason PlayerLevelEndReason { get; set; }
        public LevelCompletionResults LevelCompletionResults { get; set; } = new();

        public void ReadFrom(ref SpanBuffer reader)
        {
            PlayerLevelEndState = (MultiplayerPlayerLevelEndState) reader.ReadVarInt();
            PlayerLevelEndReason = (MultiplayerPlayerLevelEndReason) reader.ReadVarInt();

            if (HasAnyResult())
                LevelCompletionResults.ReadFrom(ref reader);
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            writer.WriteVarInt((int) PlayerLevelEndState);
            writer.WriteVarInt((int) PlayerLevelEndReason);

            if (HasAnyResult())
                LevelCompletionResults.WriteTo(ref writer);
        }

        public bool HasAnyResult()
        {
            return PlayerLevelEndState is MultiplayerPlayerLevelEndState.SongFinished
                or MultiplayerPlayerLevelEndState.NotFinished;
        }
    }
}