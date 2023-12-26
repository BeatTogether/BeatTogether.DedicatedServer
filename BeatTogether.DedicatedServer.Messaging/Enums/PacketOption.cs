namespace BeatTogether.DedicatedServer.Messaging.Enums
{
    public enum PacketOption
    {
        None = 0, // Default setting, currently we always use this
        Encrypted = 1, // If Player has encryption enabled
        OnlyFirstDegreeConnections = 2 // If the Player has multiple connections, only send to the first one (possibly p2p?)
    }
}
