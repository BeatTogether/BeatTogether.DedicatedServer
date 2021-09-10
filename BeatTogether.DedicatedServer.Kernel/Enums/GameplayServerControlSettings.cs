using System;

namespace BeatTogether.DedicatedServer.Kernel.Enums
{
    [Flags]
    public enum GameplayServerControlSettings
    {
        None = 0,
        AllowModifierSelection = 1,
        AllowSpectate = 2,
        All = 3
    }
}
