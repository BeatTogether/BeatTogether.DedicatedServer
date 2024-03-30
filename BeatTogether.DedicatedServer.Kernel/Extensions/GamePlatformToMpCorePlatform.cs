using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;

namespace BeatTogether.DedicatedServer.Kernel.Extensions
{
    public static class GamePlatformToMpCorePlatform
    {
        public static Platform Convert(this Enums.Platform gamePlatform)
        {
            switch (gamePlatform)
            {
                case Enums.Platform.Test:
                    return Platform.Unknown;
                case Enums.Platform.OculusRift:
                    return Platform.OculusPC;
                case Enums.Platform.OculusQuest:
                    return Platform.OculusQuest;
                case Enums.Platform.Steam:
                    return Platform.Steam;
                case Enums.Platform.PS4:
                case Enums.Platform.PS4Dev:
                case Enums.Platform.PS4Cert:
                    return Platform.PS4;
                default:
                    return 0;
            }
        }
    }
}
