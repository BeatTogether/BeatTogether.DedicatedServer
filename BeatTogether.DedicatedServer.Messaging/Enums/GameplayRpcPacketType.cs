namespace BeatTogether.DedicatedServer.Messaging.Enums
{
    public enum GameplayRpcPacketType : byte
    {
		SetGameplaySceneSyncFinish = 0,
		SetGameplaySceneReady = 1,
		GetGameplaySceneReady = 2,
		SetActivePlayerFailedToConnect = 3,
		SetGameplaySongReady = 4,
		GetGameplaySongReady = 5,
		SetSongStartTime = 6,
		NoteCut = 7,
		NoteMissed = 8,
		LevelFinished = 9,
		ReturnToMenu = 10,
		RequestReturnToMenu = 11,
		NoteSpawned = 12,
		ObstacleSpawned = 13
	}
}
