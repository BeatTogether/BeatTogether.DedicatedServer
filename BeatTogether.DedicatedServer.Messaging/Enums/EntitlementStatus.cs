namespace BeatTogether.DedicatedServer.Messaging.Enums
{
    //Unknown 
    public enum EntitlementStatus
    {
        Unknown = 0,//means the server does not current have that users entitlement
        NotOwned = 1,//Player cannot download or play the map
        NotDownloaded = 2, //Player can play once downloaded
        Ok = 3 //Has map
    }
}
