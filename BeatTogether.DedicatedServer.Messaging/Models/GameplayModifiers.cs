using BeatTogether.LiteNetLib.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
	public sealed class GameplayModifiers : INetSerializable
	{
		public EnergyType Energy { get; set; }
		public bool NoFailOn0Energy { get; set; }
		public bool DemoNoFail { get; set; }
		public bool InstaFail { get; set; }
		public bool FailOnSaberClash { get; set; }
		public EnabledObstacleType EnabledObstacle { get; set; }
		public bool DemoNoObstacles { get; set; }
		public bool FastNotes { get; set; }
		public bool StrictAngles { get; set; }
		public bool DisappearingArrows { get; set; }
		public bool GhostNotes { get; set; }
		public bool NoBombs { get; set; }
		public SongSpeed Speed { get; set; }
		public bool NoArrows { get; set; }
		public bool ProMode { get; set; }
		public bool ZenMode { get; set; }
		public bool SmallCubes { get; set; }

		public void ReadFrom(ref SpanBufferReader reader)
		{
			int @int = reader.ReadInt32();
			Energy = (EnergyType)(@int & 15);
			DemoNoFail = (@int & 32) != 0;
			InstaFail = (@int & 64) != 0;
			FailOnSaberClash = (@int & 128) != 0;
			EnabledObstacle = (EnabledObstacleType)(@int >> 8 & 15);
			DemoNoObstacles = (@int & 4096) != 0;
			NoBombs = (@int & 8192) != 0;
			FastNotes = (@int & 16384) != 0;
			StrictAngles = (@int & 32768) != 0;
			DisappearingArrows = (@int & 65536) != 0;
			GhostNotes = (@int & 131072) != 0;
			Speed = (GameplayModifiers.SongSpeed)(@int >> 18 & 15);
			NoArrows = (@int & 4194304) != 0;
			NoFailOn0Energy = (@int & 8388608) != 0;
			ProMode = (@int & 16777216) != 0;
			ZenMode = (@int & 33554432) != 0;
			SmallCubes = (@int & 67108864) != 0;
		}

		public void WriteTo(ref SpanBufferWriter writer)
		{
			int num = 0;
			num |= (int)(Energy & (EnergyType)15);
			num |= (DemoNoFail ? 32 : 0);
			num |= (InstaFail ? 64 : 0);
			num |= (FailOnSaberClash ? 128 : 0);
			num |= (int)((int)(EnabledObstacle & (EnabledObstacleType)15) << 8);
			num |= (DemoNoObstacles ? 4096 : 0);
			num |= (NoBombs ? 8192 : 0);
			num |= (FastNotes ? 16384 : 0);
			num |= (StrictAngles ? 32768 : 0);
			num |= (DisappearingArrows ? 65536 : 0);
			num |= (GhostNotes ? 131072 : 0);
			num |= (int)((int)(Speed & (SongSpeed)15) << 18);
			num |= (NoArrows ? 4194304 : 0);
			num |= (NoFailOn0Energy ? 8388608 : 0);
			num |= (ProMode ? 16777216 : 0);
			num |= (ZenMode ? 33554432 : 0);
			num |= (SmallCubes ? 67108864 : 0);
			writer.WriteInt32(num);
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
}
