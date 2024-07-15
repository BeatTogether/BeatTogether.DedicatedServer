using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.Core.Enums;
using BeatTogether.DedicatedServer.Ignorance.IgnoranceCore;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using Serilog;

/*Lobby manager code
 * Contains the logic code for
 * - different game modes
 * - managing the beatmap
 * - managing the modifiers
 * - managing the countdown
 * - managing player entitlements
 * - managing when to start gameplay
 */
namespace BeatTogether.DedicatedServer.Kernel.Managers
{
	public sealed class LobbyManager : ILobbyManager, IDisposable
    {
        public bool AllPlayersReady => _playerRegistry.Players.All(p => p.IsReady || !p.WantsToPlayNextLevel || p.IsBackgrounded || p.IsSpectating); //If all are ready or spectating or backgrounded or a spectator type
        public bool AnyPlayersReady => _playerRegistry.Players.Any(p => p.IsReady && p.WantsToPlayNextLevel && !p.IsBackgrounded && !p.IsSpectating); //If anyone who is active wants to play
        public bool NoPlayersReady => !AnyPlayersReady;//no players want to play right now
        public bool AllPlayersNotWantToPlayNextLevel => _playerRegistry.Players.All(p => !p.WantsToPlayNextLevel);//if all are going to be spectating
        public bool AllPlayersAreInLobby => _playerRegistry.Players.All(p => p.InMenu);
        public bool CanEveryonePlayBeatmap => SelectedBeatmap != null && !_playerRegistry.Players.Any(p => (p.GetEntitlement(SelectedBeatmap.LevelId) is EntitlementStatus.NotOwned) && !p.IsSpectating && !p.IsBackgrounded && p.WantsToPlayNextLevel);
        public bool UpdateSpectatingPlayers { get; set; } = false;
        public bool ForceStartSelectedBeatmap { get; set; } = false; //For future server-side things

        public BeatmapIdentifier? SelectedBeatmap { get; private set; } = null;
        public GameplayModifiers SelectedModifiers { get; private set; } = new();
        public CountdownState CountDownState { get; private set; } = CountdownState.NotCountingDown;
        public long CountdownEndTime { get; private set; } = 0;

        private BeatmapIdentifier? _lastBeatmap = null;
        private bool _lastSpectatorState;
        private bool _LastCanEveryonePlayBeatmap;
        //private string _lastManagerId = null!;
        private readonly CancellationTokenSource _stopCts = new();
        private const int LoopTime = 100;
        public GameplayModifiers EmptyModifiers { get; } = new();

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
            Task.Run(() => UpdateLoop(_stopCts.Token));
        }

        public void Dispose()
        {
            _instance.StopEvent -= Stop;
        }

        private void Stop(IDedicatedInstance inst)
            => _stopCts.Cancel();

        private async void UpdateLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Update();
                try
                {
                    await Task.Delay(LoopTime, cancellationToken);
                }
                catch (TaskCanceledException) { continue; }
                catch (OperationCanceledException) { continue; }
            }
        }

        /// <summary>
        /// Force starts the beatmap without waiting for players to download the map. If they dont download the map in time then they should end up spectating
        /// Currently not used, probably needs fixing
        /// </summary>
        public void ForceStartBeatmapUpdate()
        {
            if(SelectedBeatmap != null)
            {
                SetCountdown(CountdownState.StartBeatmapCountdown, _configuration.CountdownConfig.BeatMapStartCountdownTime);

                if (CountdownEndTime <= _instance.RunTime)
                {
                    if (CountDownState != CountdownState.WaitingForEntitlement)
                    {
                        SetCountdown(CountdownState.WaitingForEntitlement);
                    }
                    if (_playerRegistry.Players.All(p => (p.GetEntitlement(SelectedBeatmap!.LevelId) is EntitlementStatus.Ok) || p.IsSpectating || !p.WantsToPlayNextLevel || p.IsBackgrounded  || p.ForceLateJoin))
                    {
                        //starts beatmap
                        _gameplayManager.SetBeatmap(SelectedBeatmap!, SelectedModifiers);
                        Task.Run(() => _gameplayManager.StartSong(CancellationToken.None));
                        //stops countdown
                        SetCountdown(CountdownState.NotCountingDown);
                        ForceStartSelectedBeatmap = false;
                        return;
                    }
                    else
                    {
                        foreach(IPlayer p in _playerRegistry.Players)
                        {
                            if(p.GetEntitlement(SelectedBeatmap.LevelId) is EntitlementStatus.NotOwned or EntitlementStatus.Unknown || p.IsSpectating || !p.WantsToPlayNextLevel || p.ForceLateJoin)
                            {
                                p.ForceLateJoin = true;
                            }
                        }
                    }
                    if(CountdownEndTime + _configuration.SendPlayersWithoutEntitlementToSpectateTimeout <= _instance.RunTime)
                    {
                        IPlayer[] MissingEntitlement = _playerRegistry.Players.Where(p => p.GetEntitlement(SelectedBeatmap!.LevelId) is not EntitlementStatus.Ok).ToArray();
                        foreach (IPlayer p in MissingEntitlement)
                        {
                            //Force the player to join late
                            p.ForceLateJoin = true;
                            _packetDispatcher.SendToPlayer(p, new CancelLevelStartPacket(), IgnoranceChannelTypes.Reliable);
                            _packetDispatcher.SendToPlayer(p, new SetIsReadyPacket() { IsReady = false }, IgnoranceChannelTypes.Reliable);
                        }
                    }
                }
            }
        }

        public void Update()
        {
            if (_instance.State != MultiplayerGameState.Lobby)
                return;

            if (!_playerRegistry.TryGetPlayer(_configuration.ServerOwnerId, out var serverOwner) && _configuration.GameplayServerConfiguration.SongSelectionMode == SongSelectionMode.ManagerPicks)
                return;
            
            UpdateBeatmap(GetSelectedBeatmap(), GetSelectedModifiers());

            UpdatePlayersMissingEntitlementsMessages();

            if (_configuration.GameplayServerConfiguration.SongSelectionMode == SongSelectionMode.ManagerPicks)
            {
                if (_lastBeatmap != SelectedBeatmap || _LastCanEveryonePlayBeatmap != CanEveryonePlayBeatmap || _lastSpectatorState != AllPlayersNotWantToPlayNextLevel)
                {
                    _packetDispatcher.SendToPlayer(serverOwner!, new SetIsStartButtonEnabledPacket
                    {
                        Reason = GetCannotStartGameReason(serverOwner!, CanEveryonePlayBeatmap)
                    }, IgnoranceChannelTypes.Reliable);
                }
            }

            switch (_configuration.GameplayServerConfiguration.SongSelectionMode) //server modes
            {
                case SongSelectionMode.ManagerPicks:
                    CountingDown(serverOwner!.IsReady, !serverOwner.IsReady || AllPlayersNotWantToPlayNextLevel || !CanEveryonePlayBeatmap);
                    break;
                case SongSelectionMode.Vote:
                    CountingDown(AnyPlayersReady, NoPlayersReady || AllPlayersNotWantToPlayNextLevel || !CanEveryonePlayBeatmap);
                    break;
                case SongSelectionMode.RandomPlayerPicks:
                    CountingDown(AnyPlayersReady, NoPlayersReady || AllPlayersNotWantToPlayNextLevel || !CanEveryonePlayBeatmap);
                    break;
                case SongSelectionMode.ServerPicks:
                    CountingDown(true, false);
                    break;
            }

            _LastCanEveryonePlayBeatmap = CanEveryonePlayBeatmap;
            //_lastManagerId = _configuration.ServerOwnerId;
            _lastSpectatorState = AllPlayersNotWantToPlayNextLevel;
            _lastBeatmap = SelectedBeatmap;
        }

        private void CountingDown(bool isReady, bool NotStartable)
        {
            //_logger.Debug($"CountdownEndTime '{CountdownEndTime}' RunTime '{_instance.RunTime}' BeatMapStartCountdownTime '{_configuration.CountdownConfig.BeatMapStartCountdownTime}' CountdownTimePlayersReady '{_configuration.CountdownConfig.CountdownTimePlayersReady}'");  
            // If not already counting down
            if (CountDownState == CountdownState.NotCountingDown)
            {
                if (CountdownEndTime != 0 && CountdownEndTime <= _instance.RunTime)
                    CancelCountdown();
                if (isReady && !NotStartable)
                    SetCountdown(CountdownState.CountingDown, _configuration.CountdownConfig.CountdownTimePlayersReady); //Begin normal countdown
                else if (AllPlayersReady && !NotStartable)
                    SetCountdown(CountdownState.StartBeatmapCountdown, _configuration.CountdownConfig.BeatMapStartCountdownTime); //Lock in beatmap and being starting countdown
            }
            // If counting down
            if (CountDownState != CountdownState.NotCountingDown)
            {
                //_logger.Debug($"CountdownEndTime '{CountdownEndTime}' RunTime '{_instance.RunTime}'");
                //If the beatmap is not playable or the game is not startable
                if ( NotStartable )
                {
                    _logger.Debug($"Canceling countdown check  SelectedBeatmapNull={SelectedBeatmap == null}");
                    foreach (var p in _playerRegistry.Players.Where(p => (SelectedBeatmap != null && p.GetEntitlement(SelectedBeatmap.LevelId) is EntitlementStatus.NotOwned) && !p.IsSpectating && !p.IsBackgrounded && p.WantsToPlayNextLevel))
                    {
                        _logger.Debug($"Player causing cancel UserId={p.HashedUserId} Username={p.UserName} Entitlement={(SelectedBeatmap != null ? p.GetEntitlement(SelectedBeatmap.LevelId) : "SelectedBeatmap is null")} IsSpectating={p.IsSpectating} IsBackgrounded={p.IsBackgrounded} WantsToPlayNextLevel={p.WantsToPlayNextLevel}");
                    }
                    CancelCountdown();
                    return;
                }
                if (CountdownEndTime <= _instance.RunTime)
                {
                    _logger.Debug($"Countdown finished, sending map again and waiting for entitlement check");
                    // If countdown just finished, send map then pause lobby untill all players have map downloaded
                    if (CountDownState != CountdownState.WaitingForEntitlement)
                    {
                        SetCountdown(CountdownState.WaitingForEntitlement);
                        _packetDispatcher.SendToPlayers(_playerRegistry.Players.Where(p => p.GetEntitlement(SelectedBeatmap!.LevelId) == Messaging.Enums.EntitlementStatus.Unknown).ToArray(), new GetIsEntitledToLevelPacket
                        {
                            LevelId = SelectedBeatmap!.LevelId
                        }, IgnoranceChannelTypes.Reliable);
                    }
                    if (_playerRegistry.Players.All(p => (p.GetEntitlement(SelectedBeatmap!.LevelId) is EntitlementStatus.Ok) || p.IsSpectating || !p.WantsToPlayNextLevel || p.IsBackgrounded || p.ForceLateJoin))
                    {
                        _logger.Debug($"All players have entitlement or are not playing, starting map");
                        //starts beatmap
                        _gameplayManager.SetBeatmap(SelectedBeatmap!, SelectedModifiers);
                        SetCountdown(CountdownState.NotCountingDown);
                        Task.Run(() => _gameplayManager.StartSong(CancellationToken.None));
                        return;
                    }
                    if (CountdownEndTime + _configuration.SendPlayersWithoutEntitlementToSpectateTimeout <= _instance.RunTime) //If takes too long to start then players are sent to spectate by telling them the beatmap already started
                    {
                        _logger.Debug($"Took too long to start, kicking problem players");
                        IPlayer[] MissingEntitlement = _playerRegistry.Players.Where(p => p.GetEntitlement(SelectedBeatmap!.LevelId) is not EntitlementStatus.Ok &&  !p.IsSpectating && p.WantsToPlayNextLevel && !p.IsBackgrounded).ToArray();
                        foreach (IPlayer p in MissingEntitlement)
                        {
                            /* //Force the player to join late
                            p.ForceLateJoin = true;
                            _packetDispatcher.SendToPlayer(p, new CancelLevelStartPacket(), IgnoranceChannelTypes.Reliable);
                            _packetDispatcher.SendToPlayer(p, new SetIsReadyPacket() { IsReady = false }, IgnoranceChannelTypes.Reliable);*/
                            _instance.DisconnectPlayer(p);
                        }
                    }
                }
                if (CountDownState == CountdownState.CountingDown && (AllPlayersReady || (CountdownEndTime - _instance.RunTime) < _configuration.CountdownConfig.BeatMapStartCountdownTime))
                {
                    SetCountdown(CountdownState.StartBeatmapCountdown, _configuration.CountdownConfig.BeatMapStartCountdownTime);
                }
            }
        }

        private void UpdateBeatmap(BeatmapIdentifier? beatmap, GameplayModifiers modifiers)
        {
            if (SelectedBeatmap != beatmap)
            {
                SelectedBeatmap = beatmap;
                if (SelectedBeatmap != null)
                    _packetDispatcher.SendToNearbyPlayers(new SetSelectedBeatmap()
                    {
                        Beatmap = SelectedBeatmap
                    }, IgnoranceChannelTypes.Reliable); //TODO send custom mp packet details
                else
                    _packetDispatcher.SendToNearbyPlayers(new ClearSelectedBeatmap(), IgnoranceChannelTypes.Reliable);
            }
            if (SelectedModifiers != modifiers)
            {
                SelectedModifiers = modifiers;
                if (SelectedModifiers != EmptyModifiers)
                    _packetDispatcher.SendToNearbyPlayers(new SetSelectedGameplayModifiers()
                    {
                        Modifiers = SelectedModifiers
                    }, IgnoranceChannelTypes.Reliable);
                else
                    _packetDispatcher.SendToNearbyPlayers(new ClearSelectedGameplayModifiers(), IgnoranceChannelTypes.Reliable);
            }
        }

        private void UpdatePlayersMissingEntitlementsMessages()
        {
            foreach (IPlayer player in _playerRegistry.Players)
            {
                if (player.UpdateEntitlement || UpdateSpectatingPlayers)
                {
                    player.UpdateEntitlement = false;
                    if (player.BeatmapIdentifier != null)
                    {
                        _packetDispatcher.SendToPlayer(player, new SetPlayersMissingEntitlementsToLevelPacket
                        {
                            PlayersWithoutEntitlements = _playerRegistry.Players
                                .Where(p => (p.GetEntitlement(player.BeatmapIdentifier.LevelId) is EntitlementStatus.NotOwned) && !p.IsSpectating && p.WantsToPlayNextLevel && !p.IsBackgrounded)
                                .Select(p => p.HashedUserId).ToArray()
                        }, IgnoranceChannelTypes.Reliable);
                        //_logger.Debug("Sent missing entitlement packet");
                    }
                    else
                    {   //Send empty if no map is selected
                        _packetDispatcher.SendToPlayer(player, new SetPlayersMissingEntitlementsToLevelPacket
                        {
                            PlayersWithoutEntitlements = { }
                        }, IgnoranceChannelTypes.Reliable);
                    }
                }
            }
            UpdateSpectatingPlayers = false;
        }

        //TODO do something better than iterating, probs gonna be storing this server side anyway at some point soon
        public Dictionary<uint, string[]>? GetSelectedBeatmapDifficultiesRequirements()
        {
            if (!SelectedBeatmap!.LevelId.StartsWith("custom_level_"))
            {
				return null;
			}

            var selectedLevelHash = SelectedBeatmap!.LevelId.Substring(13);

            foreach (var player in _playerRegistry.Players)
            {
                _logger.Verbose($"GetDiffRequirements checking: SelectedHash {selectedLevelHash} Player MapHash {player.MapHash}");
                if(selectedLevelHash == player.MapHash)
                {
                    return player.BeatmapDifficultiesRequirements;
                }
            }
            _logger.Error($"Failed to find matching requirements searched SelectedHash {selectedLevelHash}");
            return null;
        }


        // Sets countdown and beatmap how the client would expect it to
        // If you want to cancel the countdown use CancelCountdown(), Not SetCountdown as CancelCountdown() ALSO informs the clients it has been canceled, whereas SetCountdown will not
        private void SetCountdown(CountdownState countdownState, long countdown = 0)
        {
            CountDownState = countdownState;
            switch (CountDownState)
            {
                case CountdownState.NotCountingDown:
                    CountdownEndTime = 0;
                    SelectedBeatmap = null;
                    SelectedModifiers = new();
                    break;
                case CountdownState.CountingDown:
                    if (countdown == 0)
                        countdown = 30000L;
                    CountdownEndTime = _instance.RunTime + countdown;
                    _packetDispatcher.SendToNearbyPlayers(new SetCountdownEndTimePacket
                    {
                        CountdownTime = CountdownEndTime
                    }, IgnoranceChannelTypes.Reliable);
                    break;
                case CountdownState.StartBeatmapCountdown:
                    if (countdown == 0)
                        countdown = 5000L;
                    CountdownEndTime = _instance.RunTime + countdown;
                    StartBeatmapPacket();
                    break;
                case CountdownState.WaitingForEntitlement:
                    StartBeatmapPacket();
                    break;
            }
            _logger.Debug($"CountdownEndTime final set to '{CountdownEndTime}' CountdownState '{CountDownState}' countdown is '{countdown}' RunTime is '{_instance.RunTime}'");
        }

        //Checks the lobby settings and sends the player the correct beatmap
        private void StartBeatmapPacket()
        {
            _logger.Debug("Sending start level packet");
            if (!_configuration.AllowPerPlayerModifiers && !_configuration.AllowPerPlayerDifficulties)
            {
                _packetDispatcher.SendToNearbyPlayers(new StartLevelPacket
                {
                    Beatmap = SelectedBeatmap!,
                    Modifiers = SelectedModifiers,
                    StartTime = CountdownEndTime
                }, IgnoranceChannelTypes.Reliable);
                return;
            }
            var diff = GetSelectedBeatmapDifficultiesRequirements();
            BeatmapIdentifier bm = SelectedBeatmap!;
            foreach (var player in _playerRegistry.Players)
            {
                // Check that PPD is enabled and that the difficulty the player has selected
                // exists on the level or if the player has the same map selected
                if (_configuration.AllowPerPlayerDifficulties && player.BeatmapIdentifier != null && (diff != null && diff.ContainsKey((uint)player.BeatmapIdentifier.Difficulty) || SelectedBeatmap.LevelId == player.BeatmapIdentifier.LevelId))
                    bm.Difficulty = player.BeatmapIdentifier.Difficulty;
                _logger.Debug($"Start level settings for player '{player.UserName}|{player.HashedUserId}'" +
                              $"(LevelId={bm.LevelId}, Difficulty={bm.Difficulty} Modifiers={(_configuration.AllowPerPlayerModifiers ? player.Modifiers : SelectedModifiers)}) " +
                              $"Checks: (AllowPerPlayerDifficulties={_configuration.AllowPerPlayerDifficulties}, " +
                              $"diff == null? {diff == null}, " +
                              $"diff.ContainsKey={(player.BeatmapIdentifier?.Difficulty != null && diff != null ? diff.ContainsKey((uint)player.BeatmapIdentifier.Difficulty) : "Player beatmap null or diff null")})");
                _packetDispatcher.SendToPlayer(player, new StartLevelPacket
                {
					Beatmap = bm!,
                    Modifiers = _configuration.AllowPerPlayerModifiers ?  player.Modifiers : SelectedModifiers,
                    StartTime = CountdownEndTime
                }, IgnoranceChannelTypes.Reliable);
            }
        }

        private void CancelCountdown()
        {
            switch (CountDownState)
            {
                case CountdownState.CountingDown or CountdownState.NotCountingDown:
                    _packetDispatcher.SendToNearbyPlayers(new CancelCountdownPacket(), IgnoranceChannelTypes.Reliable);
                    break;
                case CountdownState.StartBeatmapCountdown or CountdownState.WaitingForEntitlement:
                    foreach (IPlayer player in _playerRegistry.Players) //This stays because players dont send they are un-ready after the level is canceled causing bad client behaviour
                    {
                        player.IsReady = false;
                    }
                    _packetDispatcher.SendToNearbyPlayers(new CancelLevelStartPacket(), IgnoranceChannelTypes.Reliable);
                    _packetDispatcher.SendToNearbyPlayers(new SetIsReadyPacket() { IsReady = false }, IgnoranceChannelTypes.Reliable);
                    break;
                default:
                    _logger.Warning("Canceling countdown when there is no countdown to cancel");
                    break;
            }
            SetCountdown(CountdownState.NotCountingDown);
        }

        private bool PlayerMapCheck(IPlayer p)
        {
            if(p.BeatmapIdentifier == null) return false;
            //If no map hash then treat as base game map for compat reasons and while waiting for a packet
            var Passed = string.IsNullOrEmpty(p.MapHash);
            //If not passed, then we have difficulties, and if we have the diff we are looking for, then we can check it for requirements.
            if (!Passed && p.BeatmapDifficultiesRequirements.TryGetValue((uint)p.BeatmapIdentifier!.Difficulty, out string[]? Requirements))
                Passed = !(!_configuration.AllowChroma && Requirements.Contains("Chroma")) || !(!_configuration.AllowMappingExtensions && Requirements.Contains("Mapping Extensions")) || !(!_configuration.AllowNoodleExtensions && Requirements.Contains("Noodle Extensions"));
            return Passed;
        }

        private BeatmapIdentifier? GetSelectedBeatmap()
        {
            switch(_configuration.GameplayServerConfiguration.SongSelectionMode)
            {
                case SongSelectionMode.ManagerPicks:
                    {
                        if (_playerRegistry.TryGetPlayer(_configuration.ServerOwnerId, out var p) && p.BeatmapIdentifier != null)
                        {
                            if (PlayerMapCheck(p))
                                return p.BeatmapIdentifier;
                        }
                        return null;
                    }
                case SongSelectionMode.Vote:
                    Dictionary<BeatmapIdentifier, int> voteDictionary = new();
                    foreach (IPlayer player in _playerRegistry.Players.Where(p => PlayerMapCheck(p)))
                    {
                        if (voteDictionary.ContainsKey(player.BeatmapIdentifier!))
                            voteDictionary[player.BeatmapIdentifier!]++;
                        else
                            voteDictionary.Add(player.BeatmapIdentifier!, 1);
                    }
                    if (!voteDictionary.Any())
                    {
                        return null;
                    }
                    BeatmapIdentifier? Selected = null;
                    int Votes = 0;
                    foreach (var item in voteDictionary)
                    {
                        if (item.Value > Votes)
                        {
                            Selected = item.Key;
                            Votes = item.Value;
                        }
                    }
                    return Selected;
                case SongSelectionMode.RandomPlayerPicks:
                    if (CountDownState == CountdownState.CountingDown || CountDownState == CountdownState.NotCountingDown)
                    {
                        Random rand = new();
                        int selectedPlayer = rand.Next(_playerRegistry.GetPlayerCount() - 1);
                        RandomlyPickedPlayer = _playerRegistry.Players[selectedPlayer].HashedUserId;
                        return PlayerMapCheck(_playerRegistry.Players[selectedPlayer]) ? _playerRegistry.Players[selectedPlayer].BeatmapIdentifier : null;
                    }
                    return SelectedBeatmap;

                case SongSelectionMode.ServerPicks:
                    return SelectedBeatmap!;
            };
            return null;
        }

        string RandomlyPickedPlayer = string.Empty;

        private GameplayModifiers GetSelectedModifiers()
		{
            switch(_configuration.GameplayServerConfiguration.SongSelectionMode)
			{
                case SongSelectionMode.ManagerPicks:
                    if(_playerRegistry.TryGetPlayer(_configuration.ServerOwnerId, out var ServerOwner))
                        return ServerOwner.Modifiers;
                    return EmptyModifiers;
                case SongSelectionMode.Vote:
                    GameplayModifiers gameplayModifiers = new();
                    Dictionary<GameplayModifiers, int> voteDictionary = new();
                    foreach (IPlayer player in _playerRegistry.Players.Where(p => p.Modifiers != null))
                    {
                        if (voteDictionary.ContainsKey(player.Modifiers))
                            voteDictionary[player.Modifiers]++;
                        else
                            voteDictionary.Add(player.Modifiers!, 1);
                    }
                    if (!voteDictionary.Any())
                    {
                        int Votes = 0;
                        foreach (var item in voteDictionary)
                        {
                            if (item.Value > Votes)
                            {
                                gameplayModifiers = item.Key;
                                Votes = item.Value;
                            }
                        }
                    }
                    if(_configuration.ApplyNoFailModifier)
                        gameplayModifiers.NoFailOn0Energy = true;
                    return gameplayModifiers;
                case SongSelectionMode.RandomPlayerPicks:
                    if (RandomlyPickedPlayer == string.Empty)
                    {
                        GameplayModifiers Modifiers = new()
                        {
                            NoFailOn0Energy = _configuration.ApplyNoFailModifier
                        };
                        return Modifiers;
                    }
                    gameplayModifiers = new();
                    if (_playerRegistry.TryGetPlayer(RandomlyPickedPlayer, out var Randomplayer))
                        gameplayModifiers = Randomplayer.Modifiers;
                    if (_configuration.ApplyNoFailModifier)
                        gameplayModifiers.NoFailOn0Energy = true;
                    return gameplayModifiers;
                case SongSelectionMode.ServerPicks:
                    return SelectedModifiers;
            };
            return new GameplayModifiers();
		}

        public CannotStartGameReason GetCannotStartGameReason(IPlayer player, bool DoesEveryoneOwnBeatmap)
        {
            if (player.IsServerOwner && player.BeatmapIdentifier == null)
                return CannotStartGameReason.NoSongSelected;
            if (!AllPlayersAreInLobby)
                return CannotStartGameReason.AllPlayersNotInLobby;
            if (AllPlayersNotWantToPlayNextLevel)
                return CannotStartGameReason.AllPlayersSpectating;
            if (SelectedBeatmap != null && ((_configuration.ForceStartMode && player.GetEntitlement(SelectedBeatmap.LevelId) == EntitlementStatus.NotOwned) || (!_configuration.ForceStartMode && !DoesEveryoneOwnBeatmap)))
                return CannotStartGameReason.DoNotOwnSong;
            return CannotStartGameReason.None;
        }
    }
}
