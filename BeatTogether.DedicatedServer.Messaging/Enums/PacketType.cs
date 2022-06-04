namespace BeatTogether.DedicatedServer.Messaging.Enums
{
    public enum PacketType : byte
    {
		SyncTime = 0,
		PlayerConnected = 1,
		PlayerIdentity = 2,
		PlayerLatencyUpdate = 3,
		PlayerDisconnected = 4,
		PlayerSortOrderUpdate = 5,
		Party = 6,
		MultiplayerSession = 7,
		KickPlayer = 8,
		PlayerStateUpdate = 9,
		PlayerAvatarUpdate = 10,
		Ping = 11,
		Pong = 12
	}
}
