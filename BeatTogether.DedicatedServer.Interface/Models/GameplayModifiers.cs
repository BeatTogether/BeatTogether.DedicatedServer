using System;

namespace BeatTogether.DedicatedServer.Interface.Models
{
    public record GameplayModifiers(
        EnergyType Energy,
        bool NoFailOn0Energy,
        bool DemoNoFail,
        bool InstaFail,
        bool FailOnSaberClash,
        EnabledObstacleType EnabledObstacle,
        bool DemoNoObstacles,
        bool FastNotes,
        bool StrictAngles,
        bool DisappearingArrows,
        bool GhostNotes,
        bool NoBombs,
        SongSpeed Speed,
        bool NoArrows,
        bool ProMode,
        bool ZenMode,
        bool SmallCubes
        )
    {
        public static explicit operator GameplayModifiers(Messaging.Models.GameplayModifiers v)
        {
            return new GameplayModifiers((EnergyType)v.Energy, v.NoFailOn0Energy, v.DemoNoFail, v.InstaFail, v.FailOnSaberClash, (EnabledObstacleType)v.EnabledObstacle, v.DemoNoObstacles, v.FastNotes, v.StrictAngles, v.DisappearingArrows, v.GhostNotes, v.NoBombs, (SongSpeed)v.Speed, v.NoArrows, v.ProMode, v.ZenMode, v.SmallCubes);
        }
    }

    public enum EnabledObstacleType
	{
		All,
		FullHeightOnly,
		NoObstacles
	}

	public enum EnergyType
	{
		Bar,
		Battery
	}

	public enum SongSpeed
	{
		Normal,
		Faster,
		Slower,
		SuperFast
	}
}
