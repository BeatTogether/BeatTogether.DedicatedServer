using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
	public sealed class SetIsStartButtonEnabledPacket : BaseRpcWithValuesPacket
	{
		public CannotStartGameReason Reason { get; set; }

		public override void ReadFrom(ref SpanBuffer reader)
        {
			base.ReadFrom(ref reader);
			if (HasValue0)
				Reason = (CannotStartGameReason)reader.ReadVarInt();
		}

		public override void WriteTo(ref SpanBuffer writer)
        {
			base.WriteTo(ref writer);
			writer.WriteVarInt((int)Reason);
		}
	}
}