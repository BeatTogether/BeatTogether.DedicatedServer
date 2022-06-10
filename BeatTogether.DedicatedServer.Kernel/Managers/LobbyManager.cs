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

/*Lobby manager code
 * Contains the logic code for
 * - different game modes
 * - setting the beatmap
 * - setting the modifiers
 * - managing the countdown
 * - checking player entitlements
 * - when to start gameplay
 */
namespace BeatTogether.DedicatedServer.Kernel.Managers
{
    public sealed class LobbyManager : ILobbyManager, IDisposable
    {
        private const float CountdownTimeSomeReady = 30.0f;
        private const float CountdownTimeManagerReady = 15.0f;
        private const float CountdownTimeEveryoneReady = 5.0f;

        public bool AllPlayersReady => _playerRegistry.Players.All(p => p.IsReady || !p.WantsToPlayNextLevel); //if all players are ready OR spectating
        public bool SomePlayersReady => _playerRegistry.Players.Any(p => p.IsReady);                           //if *any* are ready
        public bool NoPlayersReady => _playerRegistry.Players.All(p => !p.IsReady || !p.WantsToPlayNextLevel); //players not ready or spectating 
        public bool AllPlayersSpectating => _playerRegistry.Players.All(p => !p.WantsToPlayNextLevel);         //if all spectating

        public BeatmapIdentifier? SelectedBeatmap { get; private set; }
        public GameplayModifiers SelectedModifiers { get; private set; } = new();
        public CountdownState CountDownState { get; private set; } = CountdownState.NotCountingDown;
        public float CountdownEndTime { get; private set; } = 0;

        private BeatmapIdentifier? _lastBeatmap;
        private bool _lastSpectatorState;
        private bool _lastAllOwnMap;          
        private string _lastManagerId = null!;
        private readonly CancellationTokenSource _stopCts = new();
        private const int ActiveLoopTime = 100;
        private const int NoPlayersLoopTIme = 1000;
        private int LoopTime = 100;

        private readonly InstanceConfiguration _configuration;
        private readonly IDedicatedInstance _instance;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IPacketDispatcher _packetDispatcher;
        private readonly IGameplayManager _gameplayManager;
        private readonly ILogger _logger = Log.ForContext<LobbyManager>();
        private readonly IBeatmapRepository _beatmapRepository;

        public LobbyManager(
            InstanceConfiguration configuration,
            IDedicatedInstance instance,
            IPlayerRegistry playerRegistry,
            IPacketDispatcher packetDispatcher,
            IGameplayManager gameplayManager,
            IBeatmapRepository beatmapRepository
            )
        {
            _configuration = configuration;
            _instance = instance;
            _playerRegistry = playerRegistry;
            _packetDispatcher = packetDispatcher;
            _gameplayManager = gameplayManager;
            _beatmapRepository = beatmapRepository;

            _instance.StopEvent += Stop;
            Task.Run(() => UpdateLoop(_stopCts.Token));
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
                await Task.Delay(LoopTime, cancellationToken);
                Update();
                UpdateLoop(cancellationToken);
            }
            catch
            {

            }
        }

        public void Update()
        {
            if(_playerRegistry.Players.Count == 0)
            {
                LoopTime = NoPlayersLoopTIme;
                return;
            }
            else
            {
                LoopTime = ActiveLoopTime;
            }
            if (_instance.State != MultiplayerGameState.Lobby)
            {
                //Sends players stuck in the lobby to spectate the ongoing game, prevents a rare quest issue with loss of tracking causing the game to pause on map start
                if (_gameplayManager.State == GameplayManagerState.Gameplay && _playerRegistry.Players.Any(p => p.InLobby) && _instance.State == MultiplayerGameState.Game && _gameplayManager.CurrentBeatmap != null)
                {
                    foreach (var p in _playerRegistry.Players.FindAll(p => p.InLobby))
                    {
                        _packetDispatcher.SendToPlayer(p, new SetPlayersMissingEntitlementsToLevelPacket
                        {
                            PlayersWithoutEntitlements = _playerRegistry.Players
                                .Where(p => p.GetEntitlement(_gameplayManager.CurrentBeatmap!.LevelId) is EntitlementStatus.NotOwned or EntitlementStatus.NotDownloaded)
                                .Select(p => p.UserId).ToList()
                        }, DeliveryMethod.ReliableOrdered);
                        _packetDispatcher.SendToPlayer(p, new StartLevelPacket
                        {
                            Beatmap = _gameplayManager.CurrentBeatmap!,
                            Modifiers = _gameplayManager.CurrentModifiers!,
                            StartTime = _instance.RunTime
                        }, DeliveryMethod.ReliableOrdered);
                        _gameplayManager.HandleLevelFinished(p, new LevelFinishedPacket
                        {
                            Results = new MultiplayerLevelCompletionResults
                            {
                                PlayerLevelEndState = MultiplayerPlayerLevelEndState.NotStarted,
                                LevelCompletionResults = new LevelCompletionResults(),
                                PlayerLevelEndReason = MultiplayerPlayerLevelEndReason.StartupFailed
                            }
                        });
                    }
                }
                return;
            }

            if (!_playerRegistry.TryGetPlayer(_configuration.ManagerId, out var manager) && _configuration.SongSelectionMode == SongSelectionMode.ManagerPicks)
                return;
            
            UpdateBeatmap(GetSelectedBeatmap(), GetSelectedModifiers());

            if (SelectedBeatmap != null)
            {
                bool allPlayersOwnBeatmap = _playerRegistry.Players
                    .All(p => p.GetEntitlement(SelectedBeatmap.LevelId) is EntitlementStatus.Ok or EntitlementStatus.NotDownloaded);

                // If new beatmap selected or entitlement state changed or spectator state changed or manager changed
                if (_lastBeatmap != SelectedBeatmap || _lastAllOwnMap != allPlayersOwnBeatmap || _lastSpectatorState != AllPlayersSpectating || _lastManagerId != _configuration.ManagerId)
                {
                    // If not all players have beatmap
                    if (!allPlayersOwnBeatmap)
                    {
                        // Set players missing entitlements
                        _packetDispatcher.SendToNearbyPlayers(new SetPlayersMissingEntitlementsToLevelPacket
                        {
                            PlayersWithoutEntitlements = _playerRegistry.Players
                                .Where(p => p.GetEntitlement(SelectedBeatmap.LevelId) is EntitlementStatus.NotOwned)
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

                        // Allow start map if at least one player is not spectating
                        if (!AllPlayersSpectating)
                            _packetDispatcher.SendToNearbyPlayers(new SetIsStartButtonEnabledPacket
                            {
                                Reason = CannotStartGameReason.None
                            }, DeliveryMethod.ReliableOrdered);
                        else// Cannot start map because all players are spectating
                            _packetDispatcher.SendToNearbyPlayers(new SetIsStartButtonEnabledPacket
                            {
                                Reason = CannotStartGameReason.AllPlayersSpectating
                            }, DeliveryMethod.ReliableOrdered);
                    }
                }
                _lastAllOwnMap = allPlayersOwnBeatmap;

                switch (_configuration.SongSelectionMode) //server modes
                {
                    case SongSelectionMode.ManagerPicks:
                        CountingDown(manager!.IsReady, CountdownTimeManagerReady, !manager!.IsReady, allPlayersOwnBeatmap);
                        break;
                    case SongSelectionMode.Vote:
                        CountingDown(SomePlayersReady, CountdownTimeSomeReady, NoPlayersReady, allPlayersOwnBeatmap);
                        break;
                    case SongSelectionMode.RandomPlayerPicks:
                        CountingDown(SomePlayersReady, CountdownTimeSomeReady, NoPlayersReady, allPlayersOwnBeatmap);
                        break;
                    case SongSelectionMode.ServerPicks:
                        TournamentCountDown();
                        break;
                }
            }
            // If beatmap is null and it wasn't previously or manager changed
            else if (_lastBeatmap != SelectedBeatmap || _lastManagerId != _configuration.ManagerId)
            {
                // Cannot select beatmap because no beatmap is selected
                _packetDispatcher.SendToNearbyPlayers(new SetIsStartButtonEnabledPacket
                {
                    Reason = CannotStartGameReason.NoSongSelected
                }, DeliveryMethod.ReliableOrdered);
                //Send stop countdown packet if the beatmap is somehow set to null (manager may disconnect, or if tournament server setting the beatmap to null should stop the countdown)
                if(CountDownState != CountdownState.NotCountingDown)
                {
                    CancelCountdown();
                }
            }

            _lastManagerId = _configuration.ManagerId;
            _lastSpectatorState = AllPlayersSpectating;
            _lastBeatmap = SelectedBeatmap;
        }

        private void CountingDown(bool isReady, float CountDownTime, bool NotStartable, bool allPlayersOwnBeatmap)
        {
            // If not already counting down
            if (CountDownState == CountdownState.NotCountingDown)
            {
                if ((AllPlayersReady && !AllPlayersSpectating && allPlayersOwnBeatmap))
                    SetCountdown(CountdownState.StartBeatmapCountdown);
                else if (isReady && allPlayersOwnBeatmap)
                    SetCountdown(CountdownState.CountingDown, CountDownTime);
            }
            // If counting down
            else
            {
                if (CountdownEndTime <= _instance.RunTime)
                {
                    // If countdown just finished, send map then pause lobby untill all players have map downloaded
                    if (CountDownState != CountdownState.WaitingForEntitlement)
                        SetCountdown(CountdownState.WaitingForEntitlement);
                    if (_playerRegistry.Players.All(p => p.GetEntitlement(SelectedBeatmap!.LevelId) is EntitlementStatus.Ok))
                    {
                        // sends entitlements to players
                        _packetDispatcher.SendToNearbyPlayers(new SetPlayersMissingEntitlementsToLevelPacket
                        {
                            PlayersWithoutEntitlements = _playerRegistry.Players
                                .Where(p => p.GetEntitlement(SelectedBeatmap!.LevelId) is EntitlementStatus.NotOwned)
                                .Select(p => p.UserId).ToList()
                        }, DeliveryMethod.ReliableOrdered);
                        //starts beatmap
                        _gameplayManager.StartSong(SelectedBeatmap!, SelectedModifiers, CancellationToken.None);
                        //stops countdown
                        SetCountdown(CountdownState.NotCountingDown);
                        return;
                    }
                }
                else if (CountDownState == CountdownState.CountingDown)
                {
                    SendNotCountingPlayersCountdown();
                }
                // If manager/all players are no longer ready or not all players own beatmap
                if (NotStartable || !allPlayersOwnBeatmap)
                    CancelCountdown();
                else if (AllPlayersReady && (CountdownEndTime - _instance.RunTime) > CountdownTimeEveryoneReady)
                    SetCountdown(CountdownState.StartBeatmapCountdown);
            }
        }

        private void TournamentCountDown()
        {
            if (CountDownState != CountdownState.NotCountingDown)
            {
                if (CountdownEndTime <= _instance.RunTime)
                {
                    // If countdown just finished, send map then pause lobby untill all players have map downloaded
                    if (CountDownState != CountdownState.WaitingForEntitlement)
                        SetCountdown(CountdownState.WaitingForEntitlement);
                    if (_playerRegistry.Players.All(p => p.GetEntitlement(SelectedBeatmap!.LevelId) is EntitlementStatus.Ok))
                    {
                        // sends entitlements to players
                        _packetDispatcher.SendToNearbyPlayers(new SetPlayersMissingEntitlementsToLevelPacket
                        {
                            PlayersWithoutEntitlements = _playerRegistry.Players
                                .Where(p => p.GetEntitlement(SelectedBeatmap!.LevelId) is EntitlementStatus.NotOwned)
                                .Select(p => p.UserId).ToList()
                        }, DeliveryMethod.ReliableOrdered);
                        //starts beatmap
                        _gameplayManager.StartSong(SelectedBeatmap!, SelectedModifiers, CancellationToken.None);
                        //stops countdown
                        SetCountdown(CountdownState.NotCountingDown);
                        return;
                    }
                }
            }
            else if(CountDownState == CountdownState.NotCountingDown && SelectedBeatmap != null)
            {
                SetCountdown(CountdownState.StartBeatmapCountdown, 30);
            }
        }

        public bool FetchingBeatmap = false;

        public async void UpdateBeatmap(BeatmapIdentifier? beatmap, GameplayModifiers modifiers)
        {
            if (SelectedBeatmap != beatmap && !FetchingBeatmap)
            {
                FetchingBeatmap = true;
                if (beatmap == null || !await _beatmapRepository.CheckBeatmap(beatmap))
                {
                    SelectedBeatmap = null;
                    FetchingBeatmap = false;
                }
                else
                {
                    SelectedBeatmap = beatmap;
                    FetchingBeatmap = false;
                }
            }
            if (SelectedModifiers != modifiers)
            {
                SelectedModifiers = modifiers;
            }
        }

        // Sets countdown and beatmap how the client would expect it to
        // If you want to cancel the countdown use CancelCountdown(), Not SetCountdown as CancelCountdown() ALSO informs the clients it has been canceled, whereas SetCountdown will now
        public void SetCountdown(CountdownState countdownState, float countdown = 0)
        {
            CountDownState = countdownState;
            switch (CountDownState)
            {
                case CountdownState.NotCountingDown:
                    CountdownEndTime = 0;
                    SelectedBeatmap = null;
                    SelectedModifiers = new();
                    SetActivePlayersCountdown(false);
                    break;
                case CountdownState.CountingDown:
                    if (countdown == 0)
                        countdown = 30f;
                    CountdownEndTime = _instance.RunTime + countdown;
                    _packetDispatcher.SendToNearbyPlayers(new SetCountdownEndTimePacket
                    {
                        CountdownTime = CountdownEndTime
                    }, DeliveryMethod.ReliableOrdered);
                    SetActivePlayersCountdown(true);
                    break;
                case CountdownState.StartBeatmapCountdown:
                    if (countdown == 0)
                        countdown = 5f;
                    CountdownEndTime = _instance.RunTime + countdown;
                    _packetDispatcher.SendToNearbyPlayers(new StartLevelPacket
                    {
                        Beatmap = SelectedBeatmap!,
                        Modifiers = SelectedModifiers,
                        StartTime = CountdownEndTime
                    }, DeliveryMethod.ReliableOrdered);
                    break;
                case CountdownState.WaitingForEntitlement:
                    _packetDispatcher.SendToNearbyPlayers(new StartLevelPacket
                    {
                        Beatmap = SelectedBeatmap!,
                        Modifiers = SelectedModifiers,
                        StartTime = CountdownEndTime
                    }, DeliveryMethod.ReliableOrdered);
                    CountdownEndTime = -1;
                    break;
            }
        }

        private void SetActivePlayersCountdown(bool CountingDown)
        {
            foreach (var player in _playerRegistry.Players)
                player.WasActiveAtCountdownStart = CountingDown;
        }

        private void SendNotCountingPlayersCountdown()
        {
            foreach (var player in _playerRegistry.Players)
            {
                if (!player.WasActiveAtCountdownStart)
                {
                    _packetDispatcher.SendToPlayer(player, new SetCountdownEndTimePacket
                    {
                        CountdownTime = CountdownEndTime
                    }, DeliveryMethod.ReliableOrdered);
                    player.WasActiveAtCountdownStart = true;
                }
            }
        }

        public void CancelCountdown()
        {
            switch (CountDownState)
            {
                case CountdownState.CountingDown:
                    _packetDispatcher.SendToNearbyPlayers(new CancelCountdownPacket(), DeliveryMethod.ReliableOrdered);
                    break;
                case CountdownState.StartBeatmapCountdown or CountdownState.WaitingForEntitlement:
                    _packetDispatcher.SendToNearbyPlayers(new CancelLevelStartPacket(), DeliveryMethod.ReliableOrdered);
                    break;
                default:
                    _logger.Information("Canceling countdown when there is no countdown to cancel");
                    break;
            }
            SetCountdown(CountdownState.NotCountingDown);
        }


        public BeatmapIdentifier? GetSelectedBeatmap()
        {
            switch(_configuration.SongSelectionMode)
            {
                case SongSelectionMode.ManagerPicks: return _playerRegistry.GetPlayer(_configuration.ManagerId).BeatmapIdentifier;
                case SongSelectionMode.Vote:
                    Dictionary<BeatmapIdentifier, int> voteDictionary = new();
                    foreach (IPlayer player in _playerRegistry.Players.Where(p => p.BeatmapIdentifier != null && p.IsReady))
                    {
                        if (voteDictionary.ContainsKey(player.BeatmapIdentifier!))
                            voteDictionary[player.BeatmapIdentifier!]++;
                        else
                            voteDictionary.Add(player.BeatmapIdentifier!, 1);
                    }
                    if (!voteDictionary.Any())
                        return null;
                    return voteDictionary.OrderByDescending(n => n.Value).First().Key;
                case SongSelectionMode.RandomPlayerPicks:
                    if (SelectedBeatmap != _lastBeatmap || SelectedBeatmap == null)
                        return _playerRegistry.Players[new Random().Next(_playerRegistry.Players.Count)].BeatmapIdentifier; //TODO, Fix this to work correctly i guess
                    return SelectedBeatmap;
                case SongSelectionMode.ServerPicks:
                    return SelectedBeatmap;
            };
            return null;
        }

        public GameplayModifiers GetSelectedModifiers()
		{
            switch(_configuration.SongSelectionMode)
			{
                case SongSelectionMode.ManagerPicks: return _playerRegistry.GetPlayer(_configuration.ManagerId).Modifiers;
                case SongSelectionMode.Vote or SongSelectionMode.RandomPlayerPicks:
                    Dictionary<GameplayModifiers, int> voteDictionary = new();
                    foreach (IPlayer player in _playerRegistry.Players.Where(p => p.Modifiers != null))
                    {
                        if (voteDictionary.ContainsKey(player.Modifiers))
                            voteDictionary[player.Modifiers]++;
                        else
                            voteDictionary.Add(player.Modifiers!, 1);
                    }
                    if (!voteDictionary.Any())
                        return new GameplayModifiers();
                    return voteDictionary.OrderByDescending(n => n.Value).First().Key;
                case SongSelectionMode.ServerPicks:
                    return SelectedModifiers;
            };
            return new GameplayModifiers();
		}
    }
}
