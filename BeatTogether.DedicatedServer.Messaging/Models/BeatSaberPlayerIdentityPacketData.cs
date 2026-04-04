using BeatTogether.DedicatedServer.Messaging.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Util;
using Krypton.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace BeatTogether.DedicatedServer.Messaging.Models
{
	public sealed class BeatSaberPlayerIdentityPacketData : INetSerializable
	{
		public MultiplayerAvatarsData PlayerAvatar { get; set; } = new();

		public BeatSaberPlayerIdentityPacketData()
		{
		}

		public BeatSaberPlayerIdentityPacketData(MultiplayerAvatarsData playerAvatar)
		{
			PlayerAvatar = playerAvatar;
		}

		public void WriteTo(ref SpanBuffer writer)
		{
			PlayerAvatar.WriteTo(ref writer);
		}

		public void ReadFrom(ref SpanBuffer reader)
		{
			PlayerAvatar.ReadFrom(ref reader);
		}

	}
}
