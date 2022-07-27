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
        );

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
