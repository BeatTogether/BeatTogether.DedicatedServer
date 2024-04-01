using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.CommandHandlers;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MPChatPackets;

namespace BeatTogether.DedicatedServer.Kernel.Commands.CommandHandlers
{
    class StartBeatmapTimeHandler : BaseCommandHandler<SetBeatmapStartTime>
    {
        private readonly IPacketDispatcher _packetDisapatcher;
        private readonly InstanceConfiguration _Configuration;

        public StartBeatmapTimeHandler(IPacketDispatcher packetDisapatcher, InstanceConfiguration instanceConfiguration)
        {
            _packetDisapatcher = packetDisapatcher;
            _Configuration = instanceConfiguration;
        }

        public override void Handle(IPlayer player, SetBeatmapStartTime command)
        {
            _Configuration.CountdownConfig.BeatMapStartCountdownTime = command.Countdown;
            _packetDisapatcher.SendToNearbyPlayers(new MpcTextChatPacket
            {
                Text = "Beatmap start time is now: " + command.Countdown
            }, IgnoranceChannelTypes.Reliable);
        }
    }
}
