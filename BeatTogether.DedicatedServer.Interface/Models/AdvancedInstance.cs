using BeatTogether.DedicatedServer.Interface.Enums;

namespace BeatTogether.DedicatedServer.Interface.Models
{
    public enum MultiplayerGameState
    {
        None = 0,
        Lobby = 1,
        Game = 2
    }
    public enum GameplayManagerState
    {
        None,
        SceneLoad,
        SongLoad,
        Gameplay,
        Results
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


    public record AdvancedInstance(
        GameplayServerConfiguration GameplayServerConfiguration,
        int PlayerCount, //If == o, inactive lobby
        bool IsRunning,
        float RunTime,
        int Port,
        string UserId,
        string UserName,//This is the servers name
        MultiplayerGameState MultiplayerGameState,
		GameplayManagerState GameplayManagerState,
        float NoPlayersTime,    //When RunTime-NoPlayersTime >= DestroyInstanceTimeout, it is stopped
        float DestroyInstanceTimeout, //-1 is no timeout
        string SetManagerFromUserId, //If not blank then there is a permenant manager
        float CountdownEndTime,     //CountdownEndTime-RunTime = countdown
		CountdownState CountdownState,
		GameplayModifiers SelectedGameplayModifiers,
		Beatmap SelectedBeatmap);
}
