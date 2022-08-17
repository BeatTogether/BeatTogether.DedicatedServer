
namespace BeatTogether.DedicatedServer.Interface.Models
{
    public record BeatmapIdentifier(string LevelId, string Characteristic, BeatmapDifficulty Difficulty);

	public enum BeatmapDifficulty : uint
	{
		Easy = 0,
		Normal = 1,
		Hard = 2,
		Expert = 3,
		ExpertPlus = 4
	}
}
