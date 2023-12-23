using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Util;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
	public sealed class SetCountdownEndTimePacket : BaseRpcWithValuesPacket
	{
		public long CountdownTime { get; set; }

		public override void ReadFrom(ref SpanBuffer reader)
        {
			base.ReadFrom(ref reader);
			if (HasValue0)
				CountdownTime = reader.ReadVarLong();
		}

        public override void ReadFrom(ref SpanBuffer reader, Version version)
		{
            base.ReadFrom(ref reader, version);
            if (HasValue0)
                CountdownTime = reader.ReadVarLong();

        }

        public override void WriteTo(ref SpanBuffer writer)
        {
			base.WriteTo(ref writer);
			writer.WriteVarLong(CountdownTime);
		}

		public override void WriteTo(ref SpanBuffer writer, Version version)
		{
            base.WriteTo(ref writer, version);
            writer.WriteVarLong(CountdownTime);
        }
	}
}
