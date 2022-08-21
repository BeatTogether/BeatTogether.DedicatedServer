
namespace BeatTogether.DedicatedServer.Interface.Models
{
    public record LevelCompletionResults(GameplayModifiers GameplayModifiers,
        int ModifiedScore,
        int MultipliedScore,
        Rank Rank,
        bool FullCombo,
        float LeftSaberMovementDistance,
        float RightSaberMovementDistance,
        float LeftHandMovementDistance,
        float RightHandMovementDistance,
        LevelEndStateType LevelEndStateType,
        LevelEndAction LevelEndAction,
        float Energy,
        int GoodCutsCount,
        int BadCutsCount,
        int MissedCount,
        int NotGoodCount,
        int OkCount,
        int MaxCutScore,
        int TotalCutScore,
        int GoodCutsCountForNotesWithFullScoreScoringType,
        float AverageCenterDistanceCutScoreForNotesWithFullScoreScoringType,
        float AverageCutScoreForNotesWithFullScoreScoringType,
        int MaxCombo,
        float EndSongTime
        );
    public enum Rank
    {
        E,
        D,
        C,
        B,
        A,
        S,
        SS,
        SSS
    }
    public enum LevelEndAction
    {
        None,
        Quit,
        Restart
    }
    public enum LevelEndStateType
    {
        Incomplete,
        Cleared,
        Failed
    }
}
