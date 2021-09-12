using BeatTogether.DedicatedServer.Messaging.Abstractions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
	public sealed class RequestKickPlayerPacket : BaseRpcPacket
	{
		public string KickedPlayerId { get; set; } = null!;

		public override void Deserialize(NetDataReader reader)
		{
			base.Deserialize(reader);
			KickedPlayerId = reader.GetString();
		}

		public override void Serialize(NetDataWriter writer)
		{
			base.Serialize(writer);
			writer.Put(KickedPlayerId);
		}
	}
}
