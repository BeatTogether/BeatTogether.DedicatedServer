using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.LiteNetLib.Abstractions;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class LevelCompletionResults : INetSerializable
    {
        public GameplayModifiers GameplayModifiers { get; set; } = new();
        public int ModifiedScore { get; set; }
        public int MultipliedScore { get; set; }
        public Rank Rank { get; set; }
        public bool FullCombo { get; set; }
        public float LeftSaberMovementDistance { get; set; }
        public float RightSaberMovementDistance { get; set; }
        public float LeftHandMovementDistance { get; set; }
        public float RightHandMovementDistance { get; set; }
        public LevelEndStateType LevelEndStateType { get; set; }
        public LevelEndAction LevelEndAction { get; set; }
        public float Energy { get; set; }
        public int GoodCutsCount { get; set; }
        public int BadCutsCount { get; set; }
        public int MissedCount { get; set; }
        public int NotGoodCount { get; set; }
        public int OkCount { get; set; }
        public int MaxCutScore { get; set; }
        public int TotalCutScore { get; set; }
        public int GoodCutsCountForNotesWithFullScoreScoringType { get; set; }
        public float AverageCenterDistanceCutScoreForNotesWithFullScoreScoringType { get; set; }
        public float AverageCutScoreForNotesWithFullScoreScoringType { get; set; }
        public int MaxCombo { get; set; }
        public float EndSongTime { get; set; }

        public void ReadFrom(ref SpanBuffer reader)
        {
            GameplayModifiers.ReadFrom(ref reader);
            ModifiedScore = reader.ReadVarInt();
            MultipliedScore = reader.ReadVarInt();
            Rank = (Rank)reader.ReadVarInt();
            FullCombo = reader.ReadBool();
            LeftSaberMovementDistance = reader.ReadFloat32();
            RightSaberMovementDistance = reader.ReadFloat32();
            LeftHandMovementDistance = reader.ReadFloat32();
            RightHandMovementDistance = reader.ReadFloat32();
            LevelEndStateType = (LevelEndStateType)reader.ReadVarInt();
            LevelEndAction = (LevelEndAction)reader.ReadVarInt();
            Energy = reader.ReadFloat32();
            GoodCutsCount = reader.ReadVarInt();
            BadCutsCount = reader.ReadVarInt();
            MissedCount = reader.ReadVarInt();
            NotGoodCount = reader.ReadVarInt();
            OkCount = reader.ReadVarInt();
            MaxCutScore = reader.ReadVarInt();
            TotalCutScore = reader.ReadVarInt();
            GoodCutsCountForNotesWithFullScoreScoringType = reader.ReadVarInt();
            AverageCenterDistanceCutScoreForNotesWithFullScoreScoringType = reader.ReadFloat32();
            AverageCutScoreForNotesWithFullScoreScoringType = reader.ReadFloat32();
            MaxCombo = reader.ReadVarInt();
            EndSongTime = reader.ReadFloat32();
        }

        public void WriteTo(ref SpanBuffer writer)
        {
            GameplayModifiers.WriteTo(ref writer);
            writer.WriteVarInt(ModifiedScore);
            writer.WriteVarInt(MultipliedScore);
            writer.WriteVarInt((int)Rank);
            writer.WriteBool(FullCombo);
            writer.WriteFloat32(LeftSaberMovementDistance);
            writer.WriteFloat32(RightSaberMovementDistance);
            writer.WriteFloat32(LeftHandMovementDistance);
            writer.WriteFloat32(RightHandMovementDistance);
            writer.WriteVarInt((int)LevelEndStateType);
            writer.WriteVarInt((int)LevelEndAction);
            writer.WriteFloat32(Energy);
            writer.WriteVarInt(GoodCutsCount);
            writer.WriteVarInt(BadCutsCount);
            writer.WriteVarInt(MissedCount);
            writer.WriteVarInt(NotGoodCount);
            writer.WriteVarInt(OkCount);
            writer.WriteVarInt(MaxCutScore);
            writer.WriteVarInt(TotalCutScore);
            writer.WriteVarInt(GoodCutsCountForNotesWithFullScoreScoringType);
            writer.WriteFloat32(AverageCenterDistanceCutScoreForNotesWithFullScoreScoringType);
            writer.WriteFloat32(AverageCutScoreForNotesWithFullScoreScoringType);
            writer.WriteVarInt(MaxCombo);
            writer.WriteFloat32(EndSongTime);
        }
    }
}
