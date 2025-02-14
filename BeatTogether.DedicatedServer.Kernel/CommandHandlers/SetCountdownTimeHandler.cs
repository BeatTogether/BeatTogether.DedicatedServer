﻿using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.CommandHandlers;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MPChatPackets;

namespace BeatTogether.DedicatedServer.Kernel.Commands.CommandHandlers
{
    class SetCountdownTimeHandler : BaseCommandHandler<SetCountdown>
    {
        private readonly IPacketDispatcher _packetDisapatcher;
        private readonly InstanceConfiguration _Configuration;

        public SetCountdownTimeHandler(IPacketDispatcher packetDisapatcher, InstanceConfiguration instanceConfiguration)
        {
            _packetDisapatcher = packetDisapatcher;
            _Configuration = instanceConfiguration;
        }

        public override void Handle(IPlayer player, SetCountdown command)
        {
            _Configuration.CountdownConfig.CountdownTimePlayersReady = command.Countdown * 1000;
            _packetDisapatcher.SendToNearbyPlayers(new MpcTextChatPacket
            {
                Text = "Countdown time is now: " + command.Countdown
            }, IgnoranceChannelTypes.Reliable);
        }
    }
}
