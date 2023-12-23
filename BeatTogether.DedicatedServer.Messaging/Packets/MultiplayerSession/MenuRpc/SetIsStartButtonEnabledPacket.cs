using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.LiteNetLib.Extensions;
using BeatTogether.LiteNetLib.Util;
using System;

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

        public override void ReadFrom(ref SpanBuffer reader, Version version)
		{
            base.ReadFrom(ref reader, version);
            if (HasValue0)
                Reason = (CannotStartGameReason)reader.ReadVarInt();
        }

        public override void WriteTo(ref SpanBuffer writer)
        {
			base.WriteTo(ref writer);
			writer.WriteVarInt((int)Reason);
		}

		public override void WriteTo(ref SpanBuffer writer, Version version)
		{
            base.WriteTo(ref writer, version);
            writer.WriteVarInt((int)Reason);
        }
	}
}