using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MpCorePackets;

namespace BeatTogether.DedicatedServer.Kernel.Extensions
{
    public static class GamePlatformToMpCorePlatform
    {
        public static Platform Convert(this Kernel.Enums.Platform gamePlatform)
        {
            switch (gamePlatform)
            {
                case Kernel.Enums.Platform.Test:
                    return Platform.Unknown;
                case Kernel.Enums.Platform.OculusRift:
                    return Platform.OculusPC;
                case Kernel.Enums.Platform.OculusQuest:
                    return Platform.OculusQuest;
                case Kernel.Enums.Platform.Steam:
                    return Platform.Steam;
                case Kernel.Enums.Platform.PS4:
                case Kernel.Enums.Platform.PS4Dev:
                case Kernel.Enums.Platform.PS4Cert:
                    return Platform.PS4;
                default:
                    return 0;
            }
        }
    }
}
