using BeatTogether.DedicatedServer.Messaging.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
	public sealed class RequestKickPlayerPacket : BaseRpcPacket
	{
		public string KickedPlayerId { get; set; } = null!;

		public override void ReadFrom(ref SpanBufferReader reader)
        {
			base.ReadFrom(ref reader);
			KickedPlayerId = reader.ReadUTF8String();
		}

		public override void WriteTo(ref SpanBufferWriter writer)
        {
			base.WriteTo(ref writer);
			writer.WriteUTF8String(KickedPlayerId);
		}
	}
}
