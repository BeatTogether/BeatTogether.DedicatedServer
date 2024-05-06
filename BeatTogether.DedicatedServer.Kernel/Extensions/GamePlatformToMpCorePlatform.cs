using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;

namespace BeatTogether.DedicatedServer.Kernel.Extensions
{
    public static class GamePlatformToMpCorePlatform
    {
        public static Platform Convert(this Core.Enums.Platform gamePlatform)
        {
            return gamePlatform switch
            {
                Core.Enums.Platform.Test => Platform.Unknown,
                Core.Enums.Platform.OculusRift => Platform.OculusPC,
                Core.Enums.Platform.OculusQuest => Platform.OculusQuest,
                Core.Enums.Platform.Steam => Platform.Steam,
                Core.Enums.Platform.PS4 or Core.Enums.Platform.PS4Dev or Core.Enums.Platform.PS4Cert => Platform.PS4,
                _ => 0,
            };
        }
    }
}
