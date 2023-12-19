using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
	public sealed class SetCountdownEndTimePacket : BaseRpcWithValuesPacket
	{
		public long CountdownTime { get; set; }

		public override void ReadFrom(ref SpanBuffer reader)
        {
			base.ReadFrom(ref reader);
			if (HasValue0)
				CountdownTime = (long)reader.ReadVarULong();
		}

		public override void WriteTo(ref SpanBuffer writer)
        {
			base.WriteTo(ref writer);
			writer.WriteVarULong((ulong)CountdownTime);
		}
	}
}
