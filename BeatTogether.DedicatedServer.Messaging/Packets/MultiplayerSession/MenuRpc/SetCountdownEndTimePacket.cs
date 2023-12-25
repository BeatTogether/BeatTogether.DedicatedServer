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
				{
					float originalValue = reader.ReadFloat32();
                    CountdownTime = (long)(originalValue * 1000f);
                    _logger.Debug($"CountdownTime Read: {CountdownTime} | LegacyValue: {originalValue}");
				}
				else
					CountdownTime = reader.ReadVarLong();

        }

        public override void WriteTo(ref SpanBuffer writer, Version version)
		{
            base.WriteTo(ref writer, version);
			if (version < ClientVersions.NewPacketVersion)
			{
				_logger.Debug($"CountdownTime Write: {CountdownTime} | LegacyValue: {CountdownTime / 1000f}");
				writer.WriteFloat32(CountdownTime / 1000f);
				return;
			}
			else
				writer.WriteVarLong(CountdownTime);
        }
	}
}
