namespace BeatTogether.DedicatedServer.Messaging.Enums
{
	public enum CannotStartGameReason
	{
		None = 1,
		AllPlayersSpectating,
		NoSongSelected,
		AllPlayersNotInLobby,
		DoNotOwnSong
	}
}
