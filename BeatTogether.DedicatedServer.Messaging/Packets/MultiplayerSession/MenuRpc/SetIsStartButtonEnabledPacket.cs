using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.Extensions;
using LiteNetLib.Utils;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
	public sealed class SetIsStartButtonEnabledPacket : BaseRpcPacket
	{
		public CannotStartGameReason Reason { get; set; }

		public override void Deserialize(NetDataReader reader)
		{
			base.Deserialize(reader);
			Reason = (CannotStartGameReason)reader.GetVarInt();
		}

		public override void Serialize(NetDataWriter writer)
		{
			base.Serialize(writer);
			writer.PutVarInt((int)Reason);
		}
	}
}