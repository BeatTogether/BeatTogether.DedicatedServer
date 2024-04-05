using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;

namespace BeatTogether.DedicatedServer.Kernel.Extensions
{
    public static class GamePlatformToMpCorePlatform
    {
        public static Platform Convert(this Enums.Platform gamePlatform)
        {
            return gamePlatform switch
            {
                Enums.Platform.Test => Platform.Unknown,
                Enums.Platform.OculusRift => Platform.OculusPC,
                Enums.Platform.OculusQuest => Platform.OculusQuest,
                Enums.Platform.Steam => Platform.Steam,
                Enums.Platform.PS4 or Enums.Platform.PS4Dev or Enums.Platform.PS4Cert => Platform.PS4,
                _ => 0,
            };
        }
    }
}
