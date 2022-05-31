using BeatTogether.DedicatedServer.Interface.Enums;

namespace BeatTogether.DedicatedServer.Interface.Models
{
    public enum MultiplayerGameState
    {
        None = 0,
        Lobby = 1,
        Game = 2
    }
    public enum BeatmapDifficulty
    {
        Easy,
        Normal,
        Hard,
        Expert,
        ExpertPlus
    }
    public enum CountdownState : byte
    {
        NotCountingDown = 0,
        CountingDown = 1,
        StartBeatmapCountdown = 2,
        WaitingForEntitlement = 3
    }

	public enum EnabledObstacleType
	{
		All,
		FullHeightOnly,
		NoObstacles
	}

	public enum EnergyType
	{
		Bar,
		Battery
	}

	public enum SongSpeed
	{
		Normal,
		Faster,
		Slower,
		SuperFast
	}

	public record GameplayModifiers(
	 EnergyType Energy,
	 bool NoFailOn0Energy,
	 bool DemoNoFail,
	 bool InstaFail,
	 bool FailOnSaberClash,
	 EnabledObstacleType EnabledObstacle,
	 bool DemoNoObstacles,
	 bool FastNotes,
	 bool StrictAngles,
	 bool DisappearingArrows,
	 bool GhostNotes,
	 bool NoBombs,
	 SongSpeed Speed,
	 bool NoArrows,
	 bool ProMode,
	 bool ZenMode,
	 bool SmallCubes);

	public record Beatmap(string LevelId, string Characteristic, BeatmapDifficulty Difficulty);

    public record AdvancedInstance(
        GameplayServerConfiguration GameplayServerConfiguration,
        bool IsRunning,
        float RunTime,
        int Port,
        string UserId,
        string UserName,//This is the servers name
        MultiplayerGameState MultiplayerGameState,
        float NoPlayersTime,
        float DestroyInstanceTimeout,
        string SetManagerFromUserId,
        float CountdownEndTime,
		CountdownState CountdownState,
		GameplayModifiers SelectedGameplayModifiers);
}
