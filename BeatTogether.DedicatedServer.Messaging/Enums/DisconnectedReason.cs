namespace BeatTogether.DedicatedServer.Messaging.Enums
{
    public enum DisconnectedReason
	{
		Unknown = 1,
		UserInitiated = 2,
		Timeout = 3,
		Kicked = 4,
		ServerAtCapacity = 5,
		ServerConnectionClosed = 6,
		MasterServerUnreachable = 7,
		ClientConnectionClosed = 8,
		NetworkDisconnected = 9,
		ServerTerminated = 10
	}
}
