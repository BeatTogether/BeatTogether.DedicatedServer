using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Packets.Legacy;
using BeatTogether.Extensions;
using BeatTogether.LiteNetLib.Util;
using Serilog;
using System;

namespace BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc
{
	public sealed class SetCountdownEndTimePacket : BaseRpcWithValuesPacket
	{
		public long CountdownTime { get; set; }

		private readonly ILogger _logger = Log.ForContext<SetCountdownEndTimePacket>();

		public override void ReadFrom(ref SpanBuffer reader, Version version)
		{
            base.ReadFrom(ref reader, version);
            if (HasValue0)
				if (version < ClientVersions.NewPacketVersion)
					CountdownTime = (long)(reader.ReadFloat32() * 1000f);
				else
					CountdownTime = reader.ReadVarLong();

        }

        public override void WriteTo(ref SpanBuffer writer, Version version)
		{
            base.WriteTo(ref writer, version);
			if (version < ClientVersions.NewPacketVersion)
			{
				float legacyValue = (CountdownTime / 1000f);
				float rounded = (float)Math.Round(legacyValue, 4, MidpointRounding.AwayFromZero);
				_logger.Debug($"CountdownTime: {CountdownTime} | LegacyValue: {legacyValue} LegacyRounded: {rounded}");
				writer.WriteFloat32(CountdownTime / 1000f);
				return;
			}
			else
				writer.WriteVarLong(CountdownTime);
        }
	}
}
