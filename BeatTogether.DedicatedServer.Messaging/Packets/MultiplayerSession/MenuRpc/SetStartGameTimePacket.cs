using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
	public sealed class SetStartGameTimePacket : BaseRpcWithValuesPacket
	{
		public float StartTime { get; set; }

		public override void ReadFrom(ref SpanBuffer reader)
        {
			base.ReadFrom(ref reader);
			if (HasValue0)
				StartTime = reader.ReadFloat32();
		}

		public override void WriteTo(ref SpanBuffer writer)
        {
			base.WriteTo(ref writer);
			writer.WriteFloat32(StartTime);
		}
		public override void ReadFrom(ref MemoryBuffer reader)
        {
			base.ReadFrom(ref reader);
			if (HasValue0)
				StartTime = reader.ReadFloat32();
		}

		public override void WriteTo(ref MemoryBuffer writer)
        {
			base.WriteTo(ref writer);
			writer.WriteFloat32(StartTime);
		}
	}
}
