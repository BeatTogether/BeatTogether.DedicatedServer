using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Commands;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Kernel.Managers;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BeatTogether.DedicatedServer.Kernel
{
    public class TextCommandRepository : ITextCommandRepository
    {
        public delegate ITextCommand CommandFactory();

        private readonly ConcurrentDictionary<AccessLevel, ConcurrentDictionary<string, CommandFactory>> _Commands = new();
        private readonly ConcurrentDictionary<AccessLevel, ConcurrentDictionary<string, CommandFactory>> _SHCommands = new();
        private readonly ILogger _logger = Log.ForContext<LobbyManager>();
        public TextCommandRepository()
        {
            RegisterCommands();
        }

        public bool GetCommand(string[] CommandValues, AccessLevel accessLevel, out ITextCommand Command)
        {
            bool IsPatreon = (int)accessLevel % 2 == 1;
            if ((int)accessLevel == 6)
                IsPatreon = true;
            CommandFactory CommandDelegate;
            while(accessLevel >= 0)
            {
                if(_Commands.TryGetValue(accessLevel, out var commands))
                {
                    if(commands.TryGetValue(CommandValues[0], out CommandDelegate!))
                    {
                        Command = CommandDelegate();
                        if(CommandValues.Length > 1)
                            Command.ReadValues(CommandValues[1..]);
                        return true;
                    }
                }
                if (_SHCommands.TryGetValue(accessLevel, out commands))
                {
                    if (commands.TryGetValue(CommandValues[0], out CommandDelegate!))
                    {
                        Command = CommandDelegate();
                        if (CommandValues.Length > 1)
                            Command.ReadValues(CommandValues[1..]);
                        return true;
                    }
                }
                accessLevel -= IsPatreon ? 2 : 1;
            }
            Command = null!;
            return false;
        }

        public string[] GetTextCommandNames(AccessLevel accessLevel)
        {
            bool IsPatreon = (int)accessLevel % 2 == 1;
            if ((int)accessLevel == 6)
                IsPatreon = true;
            List<string> Commands = new();
            while (accessLevel >= 0)
            {
                if (_Commands.TryGetValue(accessLevel, out var commands))
                {
                    Commands.AddRange(commands.Keys);
                }
                accessLevel -= IsPatreon ? 2 : 1;
            }
            return Commands.ToArray();
        }

        public void RegisterCommand<T>(AccessLevel accessLevel) where T : class, ITextCommand, new()
        {
            Type typeFromHandle = typeof(T);
            ITextCommand command = new T();
            ConcurrentDictionary<string, CommandFactory> Commands;
            ConcurrentDictionary<string, CommandFactory> SHCommands;
            if (!_Commands.TryGetValue(accessLevel, out Commands!))
            {
                _Commands.TryAdd(accessLevel, new ConcurrentDictionary<string, CommandFactory>());
                _Commands.TryGetValue(accessLevel, out Commands!);
                _SHCommands.TryAdd(accessLevel, new ConcurrentDictionary<string, CommandFactory>());
            }
            _SHCommands.TryGetValue(accessLevel, out SHCommands!);
            Commands.TryAdd(command.CommandName, () => new T());
            SHCommands.TryAdd(command.ShortHandName, () => new T());
        }


        private void RegisterCommands()
        {
            RegisterCommand<HelpCommand>(AccessLevel.Player);

            RegisterCommand<AllowChroma>(AccessLevel.Manager);
            RegisterCommand<AllowNoodle>(AccessLevel.Manager);
            RegisterCommand<AllowMappingExtensions>(AccessLevel.Manager);
            RegisterCommand<AllowPerPlayerDifficulties>(AccessLevel.Manager);
            RegisterCommand<AllowPerPlayerModifiers>(AccessLevel.Manager);
            RegisterCommand<DisableBeatmapNotes>(AccessLevel.Manager);
            RegisterCommand<SetCountdown>(AccessLevel.Manager);
            RegisterCommand<SetBeatmapStartTime>(AccessLevel.Manager);
            RegisterCommand<SetResultsTime>(AccessLevel.Manager);
            RegisterCommand<SetServerName>(AccessLevel.Manager);
            RegisterCommand<SetWelcomeMessage>(AccessLevel.Manager);
        }
    }
}
