using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.DedicatedServer.Messaging.Util;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
	public sealed class RequestKickPlayerPacket : BaseRpcWithValuesPacket
	{
		public string KickedPlayerId { get; set; } = null!;

		public override void ReadFrom(ref SpanBuffer reader)
        {
			base.ReadFrom(ref reader);
			if (HasValue0)
				KickedPlayerId = reader.ReadString();
		}

		public override void WriteTo(ref SpanBuffer writer)
        {
			base.WriteTo(ref writer);
			writer.WriteString(KickedPlayerId);
		}
	}
}
