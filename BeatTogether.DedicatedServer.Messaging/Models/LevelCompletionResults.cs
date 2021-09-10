using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.Extensions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
    public sealed class LevelCompletionResults : INetSerializable
    {
        public GameplayModifiers GameplayModifiers { get; set; } = new();
        public int ModifiedScore { get; set; }
        public int RawScore { get; set; }
        public Rank Rank { get; set; }
        public bool FullCombo { get; set; }
        public float LeftSaberMovementDistance { get; set; }
        public float RightSaberMovementDistance { get; set; }
        public float LeftHandMovementDistance { get; set; }
        public float RightHandMovementDistance { get; set; }
        public float SongDuration { get; set; }
        public LevelEndStateType LevelEndStateType { get; set; }
        public LevelEndAction LevelEndAction { get; set; }
        public float Energy { get; set; }
        public int GoodCutsCount { get; set; }
        public int BadCutsCount { get; set; }
        public int MissedCount { get; set; }
        public int NotGoodCount { get; set; }
        public int OkCount { get; set; }
        public int AverageCutScore { get; set; }
        public int MaxCutScore { get; set; }
        public float AverageCutDistanceRawScore { get; set; }
        public int MaxCombo { get; set; }
        public float MinDirDeviation { get; set; }
        public float MaxDirDeviation { get; set; }
        public float AverageDirDeviation { get; set; }
        public float MinTimeDeviation { get; set; }
        public float MaxTimeDeviation { get; set; }
        public float AverageTimeDeviation { get; set; }
        public float EndSongTime { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            GameplayModifiers.Deserialize(reader);
            ModifiedScore = reader.GetVarInt();
            RawScore = reader.GetVarInt();
            Rank = (Rank)reader.GetVarInt();
            FullCombo = reader.GetBool();
            LeftSaberMovementDistance = reader.GetFloat();
            RightSaberMovementDistance = reader.GetFloat();
            LeftHandMovementDistance = reader.GetFloat();
            RightHandMovementDistance = reader.GetFloat();
            SongDuration = reader.GetFloat();
            LevelEndStateType = (LevelEndStateType)reader.GetVarInt();
            LevelEndAction = (LevelEndAction)reader.GetVarInt();
            Energy = reader.GetFloat();
            GoodCutsCount = reader.GetVarInt();
            BadCutsCount = reader.GetVarInt();
            MissedCount = reader.GetVarInt();
            NotGoodCount = reader.GetVarInt();
            OkCount = reader.GetVarInt();
            AverageCutScore = reader.GetVarInt();
            MaxCutScore = reader.GetVarInt();
            AverageCutDistanceRawScore = reader.GetFloat();
            MaxCombo = reader.GetVarInt();
            MinDirDeviation = reader.GetFloat();
            MaxDirDeviation = reader.GetFloat();
            AverageDirDeviation = reader.GetFloat();
            MinTimeDeviation = reader.GetFloat();
            MaxTimeDeviation = reader.GetFloat();
            AverageTimeDeviation = reader.GetFloat();
            EndSongTime = reader.GetFloat();
        }

        public void Serialize(NetDataWriter writer)
        {
            GameplayModifiers.Serialize(writer);
            writer.PutVarInt(ModifiedScore);
            writer.PutVarInt(RawScore);
            writer.PutVarInt((int)Rank);
            writer.Put(FullCombo);
            writer.Put(LeftSaberMovementDistance);
            writer.Put(RightSaberMovementDistance);
            writer.Put(LeftHandMovementDistance);
            writer.Put(RightHandMovementDistance);
            writer.Put(SongDuration);
            writer.PutVarInt((int)LevelEndStateType);
            writer.PutVarInt((int)LevelEndAction);
            writer.Put(Energy);
            writer.PutVarInt(GoodCutsCount);
            writer.PutVarInt(BadCutsCount);
            writer.PutVarInt(MissedCount);
            writer.PutVarInt(NotGoodCount);
            writer.PutVarInt(OkCount);
            writer.PutVarInt(AverageCutScore);
            writer.PutVarInt(MaxCutScore);
            writer.Put(AverageCutDistanceRawScore);
            writer.PutVarInt(MaxCombo);
            writer.Put(MinDirDeviation);
            writer.Put(MaxDirDeviation);
            writer.Put(AverageDirDeviation);
            writer.Put(MinTimeDeviation);
            writer.Put(MaxTimeDeviation);
            writer.Put(AverageTimeDeviation);
            writer.Put(EndSongTime);
        }
    }
}
