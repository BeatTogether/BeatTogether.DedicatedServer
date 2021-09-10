using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.Extensions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class MultiplayerLevelCompletionResults : INetSerializable
    {
        public MultiplayerLevelEndState LevelEndState { get; set; }
        public LevelCompletionResults LevelCompletionResults { get; set; } = new();

        public void Deserialize(NetDataReader reader)
        {
            LevelEndState = (MultiplayerLevelEndState)reader.GetVarInt();
            if (LevelEndState == MultiplayerLevelEndState.Cleared || LevelEndState == MultiplayerLevelEndState.Failed)
                LevelCompletionResults.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutVarInt((int)LevelEndState);
            if (LevelEndState == MultiplayerLevelEndState.Cleared || LevelEndState == MultiplayerLevelEndState.Failed)
                LevelCompletionResults.Serialize(writer);
        }
    }
}
