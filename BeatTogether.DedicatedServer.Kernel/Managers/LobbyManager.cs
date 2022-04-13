using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using BeatTogether.LiteNetLib.Enums;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.Managers
{
    public sealed class LobbyManager : ILobbyManager, IDisposable
    {
        private const float CountdownTimeForeverAlone = 5f;//guessing this means 1 person
        private const float CountdownTimeSomeReady = 30.0f; //if any players are ready
        private const float CountdownTimeManagerReady = 15.0f;//manager presses start
        private const float CountdownTimeEveryoneReady = 5.0f;
        private const float CountdownAfterGameplayCooldown = 5f; //unsure but its not used so idk

        public bool AllPlayersReady => _playerRegistry.Players.All(p => p.IsReady || !p.WantsToPlayNextLevel); //if all players are ready OR spectating
        public bool SomePlayersReady => _playerRegistry.Players.Any(p => p.IsReady); //if *any* are ready, dont we want this to be 50% not any?
        public bool NoPlayersReady => _playerRegistry.Players.All(p => !p.IsReady || !p.WantsToPlayNextLevel); //players not ready or spectating 
        public bool AllPlayersSpectating => _playerRegistry.Players.All(p => !p.WantsToPlayNextLevel); //if all spectating
        public int PlayerCount { get => _playerRegistry.Players.Count; }                                  //counts players in lobby
        public int PlayerCountInGame { get => _playerRegistry.Players.Count(p => p.InGameplay == true); } //counts players in gameplay

        public BeatmapIdentifier? SelectedBeatmap { get; private set; }           //this is the beatmap that has been selected to be played
        public GameplayModifiers SelectedModifiers { get; private set; } = new(); //these are the modifiers that have been selected to be played
        public float CountdownEndTime { get; private set; }                       //the instance time that the level/beatmap should start at

        private BeatmapIdentifier? _lastBeatmap;     //beatmap selected in the last lobby loop
        private bool _lastSpectatorState;            //if all players were spectating in the last lobby loop
        private bool _lastEntitlementState;          //if all players had the beatmap in the last lobby loop
        private string _lastManagerId = null!;       //id of manager in the last lobby loop
        private CancellationTokenSource _stopCts = new();   //token to stop the thread?

        private readonly InstanceConfiguration _configuration;   //the lobby config(max players, join code etc)
        private readonly IDedicatedInstance _instance;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly IGameplayManager _gameplayManager;
        private readonly ILogger _logger = Log.ForContext<LobbyManager>();

        public LobbyManager(
            InstanceConfiguration configuration,
            IDedicatedInstance instance,
            IPlayerRegistry playerRegistry,
            IPacketDispatcher packetDispatcher,
            IGameplayManager gameplayManager
            )
        {
            _configuration = configuration;
            _instance = instance;
            _playerRegistry = playerRegistry;
            _packetDispatcher = packetDispatcher;
            _gameplayManager = gameplayManager;

            _instance.StopEvent += Stop;
            Task.Run(() => UpdateLoop(_stopCts.Token)); // TODO: fuck this shit
        }// runs new thread when new lobby created

        public void Dispose()
        {
            _instance.StopEvent -= Stop;
        }

        private void Stop()
            => _stopCts.Cancel(); //stops the lobby thread

        private async void UpdateLoop(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(100, cancellationToken);
                Update();
                UpdateLoop(cancellationToken);
            }
            catch
            {

            }
        }// every 100ms, run the update loop untill cancled

        public void Update()
        {
            if (_instance.State != MultiplayerGameState.Lobby)
                return;

            if (!_playerRegistry.TryGetPlayer(_configuration.ManagerId, out var manager) && _configuration.SongSelectionMode == SongSelectionMode.OwnerPicks)
                return;
            
            BeatmapIdentifier? beatmap = GetSelectedBeatmap();
            GameplayModifiers modifiers = GetSelectedModifiers();

            if (beatmap != null)
            {
                bool allPlayersOwnBeatmap = _playerRegistry.Players
                    .All(p => p.GetEntitlement(beatmap.LevelId) is EntitlementStatus.Ok or EntitlementStatus.NotDownloaded);

                // If new beatmap selected or entitlement state changed or spectator state changed or manager changed
                if (_lastBeatmap != beatmap || _lastEntitlementState != allPlayersOwnBeatmap || _lastSpectatorState != AllPlayersSpectating || _lastManagerId != _configuration.ManagerId)
                {
                    // If not all players have beatmap
                    if (!allPlayersOwnBeatmap)
                    {
                        // Set players missing entitlements
                        _packetDispatcher.SendToNearbyPlayers(new SetPlayersMissingEntitlementsToLevelPacket
                        {
                            PlayersWithoutEntitlements = _playerRegistry.Players
                                .Where(p => p.GetEntitlement(beatmap.LevelId) is EntitlementStatus.NotOwned or EntitlementStatus.NotDownloaded)
                                .Select(p => p.UserId).ToList()
                        }, DeliveryMethod.ReliableOrdered);

                        // Cannot start song because losers dont have your map
                        _packetDispatcher.SendToNearbyPlayers(new SetIsStartButtonEnabledPacket
                        {
                            Reason = CannotStartGameReason.DoNotOwnSong
                        }, DeliveryMethod.ReliableOrdered);
                    }
                    // If all players have beatmap
                    else
                    {
                        // Set no players missing entitlements
                        _packetDispatcher.SendToNearbyPlayers(new SetPlayersMissingEntitlementsToLevelPacket
                        {
                            PlayersWithoutEntitlements = new List<string>()
                        }, DeliveryMethod.ReliableOrdered);

                        // Allow start map if *all* players are not spectating
                        if (!AllPlayersSpectating)
                        {
                            _packetDispatcher.SendToNearbyPlayers(new SetIsStartButtonEnabledPacket
                            {
                                Reason = CannotStartGameReason.None
                            }, DeliveryMethod.ReliableOrdered);
                        }

                        // Cannot start map because all players are spectating
                        if (AllPlayersSpectating)
                            _packetDispatcher.SendToNearbyPlayers(new SetIsStartButtonEnabledPacket
                            {
                                Reason = CannotStartGameReason.AllPlayersSpectating
                            }, DeliveryMethod.ReliableOrdered);
                    }
                }

                _lastEntitlementState = allPlayersOwnBeatmap; //updates last to current as it has just been processed

                // condensed code into a function because it was simular
                switch (_configuration.SongSelectionMode)
                {
                    case SongSelectionMode.OwnerPicks:
                        CountingDown(manager!.IsReady, !manager!.IsReady, allPlayersOwnBeatmap, beatmap, modifiers);
                        break;
                    case SongSelectionMode.Vote:
                        CountingDown(SomePlayersReady, NoPlayersReady, allPlayersOwnBeatmap, beatmap, modifiers);
                        break;
                }
            }

            // If beatmap is null and it wasn't previously or manager changed
            else if (_lastBeatmap != beatmap || _lastManagerId != _configuration.ManagerId)
            {
                // Cannot select song because no song is selected
                _packetDispatcher.SendToNearbyPlayers(new SetIsStartButtonEnabledPacket
                {
                    Reason = CannotStartGameReason.NoSongSelected
                }, DeliveryMethod.ReliableOrdered);
            }

            _lastManagerId = _configuration.ManagerId;
            _lastSpectatorState = AllPlayersSpectating;
            _lastBeatmap = beatmap;
        }


        private void CountingDown(bool isReady, bool NotStartable, bool allPlayersOwnBeatmap, BeatmapIdentifier? beatmap, GameplayModifiers modifiers)
        {
            // If not already counting down
            if (CountdownEndTime == 0)
            {
                if (AllPlayersReady && !AllPlayersSpectating && allPlayersOwnBeatmap)
                    CountdownEndTime = _instance.RunTime + CountdownTimeEveryoneReady;
                else if (isReady && allPlayersOwnBeatmap)
                    CountdownEndTime = _instance.RunTime + CountdownTimeManagerReady;

                // If should be counting down, tell players
                if (CountdownEndTime != 0)
                {
                    UpdateBeatmap(beatmap, modifiers);

                    // Set countdown end time
                    _packetDispatcher.SendToNearbyPlayers(new SetCountdownEndTimePacket
                    {
                        CountdownTime = CountdownEndTime
                    }, DeliveryMethod.ReliableOrdered);

                    _packetDispatcher.SendToNearbyPlayers(new StartLevelPacket
                    {
                        Beatmap = SelectedBeatmap!,
                        Modifiers = SelectedModifiers,
                        StartTime = CountdownEndTime
                    }, DeliveryMethod.ReliableOrdered);
                }
            }

            // If counting down / 
            else
            {

                // If beatmap or modifiers changed, update them
                UpdateBeatmap(beatmap, modifiers);

                // If countdown finished
                if (CountdownEndTime < _instance.RunTime)
                {
                    // If countdown just finished
                    if (CountdownEndTime != -1)
                    {
                        // send players selected beatmap
                        _packetDispatcher.SendToNearbyPlayers(new StartLevelPacket
                        {
                            Beatmap = SelectedBeatmap!,
                            Modifiers = SelectedModifiers,
                            StartTime = CountdownEndTime
                        }, DeliveryMethod.ReliableOrdered);

                        CountdownEndTime = -1;
                    }

                    // If all players have map downloaded
                    if (_playerRegistry.Players.All(p => p.GetEntitlement(SelectedBeatmap.LevelId) is EntitlementStatus.Ok))
                    {
                        // Starts beatmap
                        _gameplayManager.StartSong(SelectedBeatmap, SelectedModifiers, CancellationToken.None);
                        Console.WriteLine("Starting song");
                        // Reset and stop counting down
                        CountdownReset();
                        return;
                    }
                }

                // If manager/players is/are no longer ready or not all players own beatmap(new player may have joined)
                if (NotStartable || !allPlayersOwnBeatmap)
                {
                    // Reset and stop counting down
                    CountdownReset();
                    _packetDispatcher.SendToNearbyPlayers(new CancelCountdownPacket(), DeliveryMethod.ReliableOrdered);
                    _packetDispatcher.SendToNearbyPlayers(new CancelLevelStartPacket(), DeliveryMethod.ReliableOrdered);

                }
                else// If manager/players is/are still ready and all players own beatmap
                {
                    // If all players are ready and countdown is too long
                    if (AllPlayersReady && CountdownEndTime - _instance.RunTime > CountdownTimeEveryoneReady)
                    {
                        // Shorten countdown time
                        CountdownEndTime = _instance.RunTime + CountdownTimeEveryoneReady;

                        // Cancel countdown (bc of stupid client garbage) 
                        _packetDispatcher.SendToNearbyPlayers(new CancelCountdownPacket(), DeliveryMethod.ReliableOrdered);

                        // Set countdown end time
                        _packetDispatcher.SendToNearbyPlayers(new SetCountdownEndTimePacket
                        {
                            CountdownTime = CountdownEndTime
                        }, DeliveryMethod.ReliableOrdered);

                        // Set start level
                        _packetDispatcher.SendToNearbyPlayers(new StartLevelPacket
                        {
                            Beatmap = SelectedBeatmap!,
                            Modifiers = SelectedModifiers,
                            StartTime = CountdownEndTime
                        }, DeliveryMethod.ReliableOrdered);
                    }
                }
            }
        }

        private void CountdownReset()
        { //Reset and stop counting down
            Console.WriteLine("Resetting countdown, beatmap and modifiers");
            CountdownEndTime = 0;
            SelectedBeatmap = null;
            SelectedModifiers = new();
        }
        private void UpdateBeatmap(BeatmapIdentifier? beatmap, GameplayModifiers modifiers)
        {
            if (SelectedBeatmap != beatmap)
            {
                SelectedBeatmap = beatmap;
            }
            if (SelectedModifiers != modifiers)
            {
                SelectedModifiers = modifiers;
            }
        }






        public BeatmapIdentifier? GetSelectedBeatmap()
        {
            switch(_configuration.SongSelectionMode)
            {
                case SongSelectionMode.OwnerPicks: return _playerRegistry.GetPlayer(_configuration.ManagerId).BeatmapIdentifier;
                case SongSelectionMode.Vote:
                    Dictionary<BeatmapIdentifier, int> voteDictionary = new();
                    foreach (IPlayer player in _playerRegistry.Players)
                    {
                        if (player.BeatmapIdentifier != null)
                        {
                            if (voteDictionary.ContainsKey(player.BeatmapIdentifier))
                                voteDictionary[player.BeatmapIdentifier]++;
                            else
                                voteDictionary[player.BeatmapIdentifier] = 1;
                        }
                    }

                    if (!voteDictionary.Any())
                        return null;

                    var topBeatmap = voteDictionary.First();
                    voteDictionary.ToList().ForEach(beatmap =>
                    {
                        if (beatmap.Value > topBeatmap.Value)
                            topBeatmap = beatmap;
                    });
                    //voteDictionary.OrderByDescending(n => n.Value);
                    
                    return topBeatmap.Key;// voteDictionary.First().Key;
            };
            return null;
        }

        public GameplayModifiers GetSelectedModifiers()
		{
            switch(_configuration.SongSelectionMode)
			{
                case SongSelectionMode.OwnerPicks: return _playerRegistry.GetPlayer(_configuration.ManagerId).Modifiers;
                case SongSelectionMode.Vote:
                    Dictionary<GameplayModifiers, int> voteDictionary = new();
                    foreach (IPlayer player in _playerRegistry.Players)
                    {
                        if (player.Modifiers != null)
                        {
                            if (voteDictionary.ContainsKey(player.Modifiers))
                                voteDictionary[player.Modifiers]++;
                            else
                                voteDictionary[player.Modifiers] = 1;
                        }
                    }

                    if (!voteDictionary.Any())
                        return new GameplayModifiers();

                    var topModifiers = voteDictionary.First();
                    voteDictionary.ToList().ForEach(modifiers =>
                    {
                        if (modifiers.Value > topModifiers.Value)
                            topModifiers = modifiers;
                    });

                    //voteDictionary.OrderByDescending(n => n.Value);
                    return topModifiers.Key;// voteDictionary.First().Key;
            };
            return new GameplayModifiers();
		}
    }
}
