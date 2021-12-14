using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class MultiplayerLevelCompletionResults : INetSerializable
    {
        public MultiplayerLevelEndState LevelEndState { get; set; }
        public LevelCompletionResults LevelCompletionResults { get; set; } = new();

        public void ReadFrom(ref SpanBufferReader reader)
        {
            LevelEndState = (MultiplayerLevelEndState)reader.ReadVarInt();
            if (LevelEndState == MultiplayerLevelEndState.Cleared || LevelEndState == MultiplayerLevelEndState.Failed)
                LevelCompletionResults.ReadFrom(ref reader);
        }

        public void WriteTo(ref SpanBufferWriter writer)
        {
            writer.WriteVarInt((int)LevelEndState);
            if (LevelEndState == MultiplayerLevelEndState.Cleared || LevelEndState == MultiplayerLevelEndState.Failed)
                LevelCompletionResults.WriteTo(ref writer);
        }
    }
}
