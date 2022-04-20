using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.GameplayRpc;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using BeatTogether.LiteNetLib.Enums;
using Serilog;
using WinFormsLibrary;

/*TODO
 * Why is lobby and gameplay manager ran seperatly, i think it may work better if its all managed together.
 * 
 * and so, when the countdown gets shortened,
 * for the client end it seems to cancel the countdown, 
 * thus removing all there song selections. 
 * Whilst on server side, the countdown is still going down. 
 * When the countdown gets to 0 though, because no one has songs selected anymore 
 * the game does not start and the countdown starts instantly again 
 * and just repeats like that. 
 * The only reason we got into final-boss-chan song, 
 * is because i clicked it whilst looking at the server countdown on the laptop, 
 * it saw i had it selected when the countdown hit 0 and told everyone to play
 * 
 */

namespace BeatTogether.DedicatedServer.Kernel.Managers
{
    public sealed class LobbyManager : ILobbyManager, IDisposable
    {
        private const float CountdownTimeSomeReady = 30.0f;
        private const float CountdownTimeManagerReady = 15.0f;
        private const float CountdownTimeEveryoneReady = 5.0f;
        private const float CountdownAfterGameplayCooldown = 5f;

        public bool AllPlayersReady => _playerRegistry.Players.All(p => p.IsReady || !p.WantsToPlayNextLevel); //if all players are ready OR spectating
        public bool SomePlayersReady => _playerRegistry.Players.Any(p => p.IsReady); //if *any* are ready, dont we want this to be 50% not any?
        public bool NoPlayersReady => _playerRegistry.Players.All(p => !p.IsReady || !p.WantsToPlayNextLevel); //players not ready or spectating 
        public bool AllPlayersSpectating => _playerRegistry.Players.All(p => !p.WantsToPlayNextLevel); //if all spectating

        public BeatmapIdentifier? SelectedBeatmap { get; private set; }           //this is the beatmap that has been selected to be played
        public GameplayModifiers SelectedModifiers { get; private set; } = new(); //these are the modifiers that have been selected to be played
        public float CountdownEndTime { get; private set; }                       //the instance time that the level/beatmap should start at

        private BeatmapIdentifier? _lastBeatmap;     //beatmap selected in the last lobby loop
        private bool _lastSpectatorState;            //if all players were spectating in the last lobby loop
        private bool _lastEntitlementState;          //if all players had the beatmap in the last lobby loop
        private string _lastManagerId = null!;       //id of manager in the last lobby loop
        private CancellationTokenSource _stopCts = new();

        private readonly InstanceConfiguration _configuration;
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
        }

        public void Dispose()
        {
            _instance.StopEvent -= Stop;
        }

        private void Stop()
            => _stopCts.Cancel();

        private async void UpdateLoop(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(100, cancellationToken);
                Update();
                MessageForm.UpdtFromLobby(CountdownEndTime);
                UpdateLoop(cancellationToken);
            }
            catch
            {

            }
        }

        public void Update()
        {
            if (_instance.State != MultiplayerGameState.Lobby)
            {
                if (_playerRegistry.Players.Any(p => p.InLobby) && _instance.State == MultiplayerGameState.Game && (_gameplayManager.State == GameplayManagerState.Gameplay || _gameplayManager.State == GameplayManagerState.Results))
                {
                    foreach (var p in _playerRegistry.Players)
                    {
                        if (p.InLobby && p.WasActiveAtLevelStart && _gameplayManager.CurrentBeatmap != null)
                        {
                            LevelFinishedPacket packet = new LevelFinishedPacket();
                            packet.Results.PlayerLevelEndState = MultiplayerPlayerLevelEndState.NotStarted;
                            packet.Results.LevelCompletionResults = new LevelCompletionResults();
                            packet.Results.PlayerLevelEndReason = MultiplayerPlayerLevelEndReason.StartupFailed;
                            _gameplayManager.HandleLevelFinished(p, packet);
                            _packetDispatcher.SendToPlayer(p, new StartLevelPacket
                            {
                                Beatmap = _gameplayManager.CurrentBeatmap!,
                                Modifiers = _gameplayManager.CurrentModifiers!,
                                StartTime = CountdownEndTime
                            }, DeliveryMethod.ReliableOrdered);
                        }
                    }
                }
                return;
            }

            if (!_playerRegistry.TryGetPlayer(_configuration.ManagerId, out var manager) && _configuration.SongSelectionMode == SongSelectionMode.ManagerPicks)
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

                switch (_configuration.SongSelectionMode)
                {
                    case SongSelectionMode.ManagerPicks:
                        CountingDown(manager!.IsReady, CountdownTimeManagerReady, !manager!.IsReady, allPlayersOwnBeatmap, beatmap, modifiers);
                        break;
                    case SongSelectionMode.Vote: //quickplay
                        CountingDown(SomePlayersReady, CountdownTimeSomeReady, NoPlayersReady, allPlayersOwnBeatmap, beatmap, modifiers);
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


        private void CountingDown(bool isReady, float CountDownTime, bool NotStartable, bool allPlayersOwnBeatmap, BeatmapIdentifier? beatmap, GameplayModifiers modifiers)
        {
            // If not already counting down
            if (CountdownEndTime == 0)
            {
                UpdateBeatmap(beatmap, modifiers);
                if ((AllPlayersReady && !AllPlayersSpectating && allPlayersOwnBeatmap))
                {
                    CountdownEndTime = _instance.RunTime + CountdownTimeEveryoneReady;
                    _packetDispatcher.SendToNearbyPlayers(new StartLevelPacket
                    {
                        Beatmap = SelectedBeatmap!,
                        Modifiers = SelectedModifiers,
                        StartTime = CountdownEndTime
                    }, DeliveryMethod.ReliableOrdered);
                }
                else if (isReady && allPlayersOwnBeatmap)
                {
                    CountdownEndTime = _instance.RunTime + CountDownTime;
                    _packetDispatcher.SendToNearbyPlayers(new SetCountdownEndTimePacket
                    {
                        CountdownTime = CountdownEndTime
                    }, DeliveryMethod.ReliableOrdered);
                }

            }
            // If counting down / 
            else
            {
                // If beatmap or modifiers changed, update them
                UpdateBeatmap(beatmap, modifiers);
                if (CountdownEndTime <= _instance.RunTime)
                {
                    // If countdown just finished, send map one last time then pause lobby untill all players have map downloaded
                    if (CountdownEndTime != -1)
                    {
                        _packetDispatcher.SendToNearbyPlayers(new StartLevelPacket
                        {
                            Beatmap = SelectedBeatmap!,
                            Modifiers = SelectedModifiers,
                            StartTime = CountdownEndTime
                        }, DeliveryMethod.ReliableOrdered);
                        CountdownEndTime = -1;
                    }
                    if (_playerRegistry.Players.All(p => p.GetEntitlement(SelectedBeatmap!.LevelId) is EntitlementStatus.Ok))
                    {
                        // sends entitlements to players
                        _packetDispatcher.SendToNearbyPlayers(new SetPlayersMissingEntitlementsToLevelPacket
                        {
                            PlayersWithoutEntitlements = new List<string>()
                        }, DeliveryMethod.ReliableOrdered);
                        // Starts beatmap
                        Console.WriteLine("Starting GameplayManager, ManagerID: " + _instance.Configuration.ManagerId);
                        _gameplayManager.StartSong(SelectedBeatmap!, SelectedModifiers, CancellationToken.None);
                        // Reset and stop counting down
                        CountdownReset();
                        return;
                    }
                }

                // If manager/all players are no longer ready or not all players own beatmap(new player may have joined)
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
                    if (AllPlayersReady && (CountdownEndTime - _instance.RunTime) > CountdownTimeEveryoneReady )
                    {
                        CountdownEndTime = _instance.RunTime + CountdownTimeEveryoneReady;

                        //ok so do not send Cancel CountdownPacket to quest, will completely screw the countdown for some reason
                        /*
                        if (_instance.Configuration.ManagerId != "ziuMSceapEuNN7wRGQXrZg")
                        {
                            // Cancel countdown (bc of stupid client garbage) 
                            _packetDispatcher.SendToNearbyPlayers(new CancelCountdownPacket(), DeliveryMethod.ReliableOrdered); //Not in old quickplay logic

                            _packetDispatcher.SendToNearbyPlayers(new SetCountdownEndTimePacket                                 //Not in old quickplay logic
                            {
                                CountdownTime = CountdownEndTime
                            }, DeliveryMethod.ReliableOrdered);
                        }
                        */

                        // Set start time & start level time
                        _packetDispatcher.SendToNearbyPlayers(new StartLevelPacket
                        {
                            Beatmap = SelectedBeatmap!,
                            Modifiers = SelectedModifiers,
                            StartTime = CountdownEndTime
                        }, DeliveryMethod.ReliableOrdered);
                        Console.WriteLine("Shortened map start time, Countdown at: " + (CountdownEndTime - _instance.RunTime).ToString() + " Seconds remaining, ManagerID: " + _instance.Configuration.ManagerId);
                    }
                }
            }
        }

        private void CountdownReset()
        {
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
                case SongSelectionMode.ManagerPicks: return _playerRegistry.GetPlayer(_configuration.ManagerId).BeatmapIdentifier;
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

                    voteDictionary.OrderByDescending(n => n.Value);       
                    return voteDictionary.First().Key;
            };
            return null;
        }

        public GameplayModifiers GetSelectedModifiers()
		{
            switch(_configuration.SongSelectionMode)
			{
                case SongSelectionMode.ManagerPicks: return _playerRegistry.GetPlayer(_configuration.ManagerId).Modifiers;
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

                    voteDictionary.OrderByDescending(n => n.Value);
                    return voteDictionary.First().Key;
            };
            return new GameplayModifiers();
		}
    }
}
