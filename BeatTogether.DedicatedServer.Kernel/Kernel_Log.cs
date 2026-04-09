using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MPChatPackets;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using System;

namespace BeatTogether.DedicatedServer.Kernel
{
    public class Internal_Logger
    {
        private readonly ILogger _logger;
        private IPlayerRegistry? _playerRegistry;
        private IPacketDispatcher? _packetDispatcher;
        private readonly InstanceConfiguration _instanceConfiguration;
        private readonly IServiceProvider _serviceProvider;

        public Internal_Logger(ILogger logger, IServiceProvider serviceProvider, InstanceConfiguration instanceConfiguration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _instanceConfiguration = instanceConfiguration;
        }

        private IPacketDispatcher Get_packet_dispatcher()
        {
            _packetDispatcher ??= _serviceProvider.GetRequiredService<IPacketDispatcher>();
            return _packetDispatcher;
        }

        private IPlayerRegistry Get_player_registry()
        {
            _playerRegistry ??= _serviceProvider.GetRequiredService<IPlayerRegistry>();
            return _playerRegistry;
        }

        public void Information(string messageTemplate, bool SendOverMpchat = true)
        {
            _logger.Information(messageTemplate);
            if (!SendOverMpchat)
                return;
            SendMpChatLog(LogEventLevel.Information, messageTemplate);
        }

        public void Error(string messageTemplate, bool SendOverMpchat = true)
        {
            _logger.Error(messageTemplate);
            if (!SendOverMpchat)
                return;
            SendMpChatLog(LogEventLevel.Error, messageTemplate);
        }

        public void Verbose(string messageTemplate, bool SendOverMpchat = true)
        {
            _logger.Verbose(messageTemplate);
            if (!SendOverMpchat)
                return;
            SendMpChatLog(LogEventLevel.Verbose, messageTemplate);
        }

        public void Warning(string messageTemplate, bool SendOverMpchat = true)
        {
            _logger.Warning(messageTemplate);
            if (!SendOverMpchat)
                return;
            SendMpChatLog(LogEventLevel.Warning, messageTemplate);
        }

        public void Fatal(string messageTemplate, bool SendOverMpchat = true)
        {
            _logger.Fatal(messageTemplate);
            if (!SendOverMpchat)
                return;
            SendMpChatLog(LogEventLevel.Fatal, messageTemplate);
        }

        public void Debug(string messageTemplate, bool SendOverMpchat = true)
        {
            _logger.Debug(messageTemplate);
            if (!SendOverMpchat)
                return;
            SendMpChatLog(LogEventLevel.Debug, messageTemplate);
        }

        private void SendMpChatLog(LogEventLevel logEventLevel, string LogMessage)
        {
            if (!_instanceConfiguration.LogToMpChat)
                return;

            if (_instanceConfiguration.LogToEveryone)
            {
                if ((int)logEventLevel < (int)_instanceConfiguration.MpChatLogLevel)
                    return;
                var packet = new MpcTextChatPacket
                {
                    Text = logEventLevel.ToString() + " | " + LogMessage,
                };
                Get_packet_dispatcher().SendToNearbyPlayers(packet, IgnoranceChannelTypes.Reliable);
            }
            else
            {
                var packet = new MpcTextChatPacket
                {
                    Text = logEventLevel.ToString() + " | " + LogMessage,
                };
                foreach (var player in Get_player_registry().Players)
                {
                    if(player.LogToMpChat && (int)logEventLevel >= (int)_instanceConfiguration.MpChatLogLevel)
                    {
                        Get_packet_dispatcher().SendToPlayer(player, packet, IgnoranceChannelTypes.Reliable);
                    }
                }
            }
        }
    }


    public sealed class Kernel_Logger
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly InstanceConfiguration _instanceConfiguration;

        public Kernel_Logger(IServiceProvider serviceProvider, InstanceConfiguration instanceConfiguration)
        {
            _serviceProvider = serviceProvider;
            _instanceConfiguration = instanceConfiguration;
        }

        public Internal_Logger ForContext<TSource>()
        {
            return new Internal_Logger(Log.ForContext<TSource>(), _serviceProvider, _instanceConfiguration);
        }
    }
}
