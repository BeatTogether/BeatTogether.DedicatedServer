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
			
			if (reader.ReadUInt8() == 1)
				StartTime = reader.ReadFloat32();
		}

		public override void WriteTo(ref SpanBufferWriter writer)
        {
			base.WriteTo(ref writer);
			
			writer.WriteUInt8(1);
			writer.WriteFloat32(StartTime);
		}
	}
}
