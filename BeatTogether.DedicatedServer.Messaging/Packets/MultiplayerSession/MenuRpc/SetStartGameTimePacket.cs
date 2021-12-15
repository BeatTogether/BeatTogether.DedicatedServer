using BeatTogether.DedicatedServer.Messaging.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
	public sealed class SetStartGameTimePacket : BaseRpcPacket
	{
		public float StartTime { get; set; }

		public override void ReadFrom(ref SpanBufferReader reader)
        {
			base.ReadFrom(ref reader);
			StartTime = reader.ReadFloat32();
		}

		public override void WriteTo(ref SpanBufferWriter writer)
        {
			base.WriteTo(ref writer);
			writer.WriteFloat32(StartTime);
		}
	}
}
