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
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using BeatTogether.LiteNetLib.Enums;
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
        public bool AllPlayersReady => _playerRegistry.Players.All(p => p.IsReady || !p.WantsToPlayNextLevel); //If all are ready or not playing
        public bool SomePlayersReady => _playerRegistry.Players.Any(p => p.IsReady); //If anyone is readied
        public bool NoPlayersReady => _playerRegistry.Players.All(p => !p.IsReady || !p.WantsToPlayNextLevel); //players not ready or are going to spectate
        public bool AllPlayersNotWantToPlayNextLevel => _playerRegistry.Players.All(p => !p.WantsToPlayNextLevel);//if all are going to be spectating
        public bool AllPlayersAreInLobby => _playerRegistry.Players.All(p => p.InMenu);//if all are going to be spectating
        public bool DoesEveryoneOwnBeatmap => SelectedBeatmap != null && !_playerRegistry.Players.Any(p => (p.GetEntitlement(SelectedBeatmap.LevelId) is EntitlementStatus.NotOwned or EntitlementStatus.Unknown) && !p.IsSpectating && p.WantsToPlayNextLevel);
        public bool SpectatingPlayersUpdated { get; set; } = false;
        public bool ForceStartSelectedBeatmap { get; set; } = false;

        public BeatmapIdentifier? SelectedBeatmap { get; private set; } = null;
        public GameplayModifiers SelectedModifiers { get; private set; } = new();
        public CountdownState CountDownState { get; private set; } = CountdownState.NotCountingDown;
        public long CountdownEndTime { get; private set; } = 0;

        private BeatmapIdentifier? _lastBeatmap = null;
        private bool _lastSpectatorState;
        private bool _AllOwnMap;
        private string _lastManagerId = null!;
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
                if (_instance.ConnectDisconnectSemaphore.CurrentCount > 0)
                {
                    _instance.ConnectDisconnectSemaphore.Wait();
                    Update();
                    _instance.ConnectDisconnectSemaphore.Release();
                }
                try
                {
                    await Task.Delay(LoopTime, cancellationToken);
                }
                catch (TaskCanceledException) { continue; }
                catch (OperationCanceledException) { continue; }
            }
        }

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
                    if (_playerRegistry.Players.All(p => (p.GetEntitlement(SelectedBeatmap!.LevelId) is EntitlementStatus.Ok) || p.IsSpectating || !p.WantsToPlayNextLevel || p.ForceLateJoin))
                    {
                        foreach (IPlayer p in _playerRegistry.Players)
                        {
                            if (p.GetEntitlement(SelectedBeatmap.LevelId) is not EntitlementStatus.Ok || p.IsSpectating || !p.WantsToPlayNextLevel || p.ForceLateJoin)
                            {
                                _packetDispatcher.SendFromPlayer(p, new SetIsEntitledToLevelPacket()
                                {
                                    LevelId = SelectedBeatmap.LevelId,
                                    Entitlement = EntitlementStatus.Ok
                                }, DeliveryMethod.ReliableOrdered);
                            }
                        }
                        //The clients need to be sent that all the payers have OK entitlement
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
                        foreach (IPlayer p in _playerRegistry.Players)
                        {
                            if (p.GetEntitlement(SelectedBeatmap.LevelId) is not EntitlementStatus.Ok)
                            {
                                _packetDispatcher.SendFromPlayer(p, new SetIsEntitledToLevelPacket()
                                {
                                    LevelId = SelectedBeatmap.LevelId,
                                    Entitlement = EntitlementStatus.Ok
                                }, DeliveryMethod.ReliableOrdered);
                                p.ForceLateJoin = true;
                            }
                        }
                    }
                }
            }
        }

        public void Update()
        {
            if (_instance.State != MultiplayerGameState.Lobby)
                return;

            if (!_playerRegistry.TryGetPlayer(_configuration.ServerOwnerId, out var serverOwner) && _configuration.SongSelectionMode == SongSelectionMode.ServerOwnerPicks)
                return;
            
            UpdateBeatmap(GetSelectedBeatmap(), GetSelectedModifiers());

            if (_lastManagerId != null && _lastManagerId != _configuration.ServerOwnerId && _playerRegistry.TryGetPlayer(_lastManagerId, out var OldManager))
                _packetDispatcher.SendToPlayer(OldManager, new SetIsStartButtonEnabledPacket
                {
                    Reason = CannotStartGameReason.None
                }, DeliveryMethod.ReliableOrdered);

            if (ForceStartSelectedBeatmap)
            {
                ForceStartBeatmapUpdate();
                return;
            }

            foreach (IPlayer player in _playerRegistry.Players)
            {
                if (player.UpdateEntitlement || SpectatingPlayersUpdated)
                {
                    if (player.BeatmapIdentifier != null)
                    {
                        _packetDispatcher.SendToPlayer(player, new SetPlayersMissingEntitlementsToLevelPacket
                        {
                            PlayersWithoutEntitlements = _playerRegistry.Players
                                .Where(p => (p.GetEntitlement(player.BeatmapIdentifier.LevelId) is EntitlementStatus.NotOwned or EntitlementStatus.Unknown) && !p.IsSpectating && p.WantsToPlayNextLevel)
                                .Select(p => p.UserId).ToArray()
                        }, DeliveryMethod.ReliableOrdered);
                        _logger.Debug("Sent missing entitlement packet");
                    }
                    player.UpdateEntitlement = false;
                }
            }
            SpectatingPlayersUpdated = false;
            bool allPlayersOwnBeatmap = DoesEveryoneOwnBeatmap;

            if (_configuration.SongSelectionMode == SongSelectionMode.ServerOwnerPicks)
            {
                if (_lastBeatmap != SelectedBeatmap || _AllOwnMap != allPlayersOwnBeatmap || _lastSpectatorState != AllPlayersNotWantToPlayNextLevel)
                {
                    _packetDispatcher.SendToPlayer(serverOwner!, new SetIsStartButtonEnabledPacket
                    {
                        Reason = GetCannotStartGameReason(serverOwner!, allPlayersOwnBeatmap)
                    }, DeliveryMethod.ReliableOrdered);
                }
            }
	        _AllOwnMap = allPlayersOwnBeatmap;
            if (SelectedBeatmap != null)
            {
                switch (_configuration.SongSelectionMode) //server modes
                {
                    case SongSelectionMode.ServerOwnerPicks:
                        CountingDown(serverOwner!.IsReady, !serverOwner.IsReady);
                        break;
                    case SongSelectionMode.Vote:
                        CountingDown(SomePlayersReady, NoPlayersReady);
                        break;
                    case SongSelectionMode.RandomPlayerPicks:
                        CountingDown(SomePlayersReady, NoPlayersReady);
                        break;
                    case SongSelectionMode.ServerPicks:
                        CountingDown(true, false);
                        break;
                }
            }
            else
            {
                //Send stop countdown packet if the beatmap is somehow set to null (serrver owner may disconnect, or if tournament server setting the beatmap to null should stop the countdown)
                if (CountDownState != CountdownState.NotCountingDown)
                    CancelCountdown();
            }

            _lastManagerId = _configuration.ServerOwnerId;
            _lastSpectatorState = AllPlayersNotWantToPlayNextLevel;
            _lastBeatmap = SelectedBeatmap;
        }

        private void CountingDown(bool isReady, bool NotStartable)
        {
            _logger.Debug($"CountdownEndTime '{CountdownEndTime}' RunTime '{_instance.RunTime}' BeatMapStartCountdownTime '{_configuration.CountdownConfig.BeatMapStartCountdownTime}' CountdownTimePlayersReady '{_configuration.CountdownConfig.CountdownTimePlayersReady}'");  
            // If not already counting down
            if (CountDownState == CountdownState.NotCountingDown)
            {
                if (CountdownEndTime != 0 && CountdownEndTime <= _instance.RunTime)
                    CancelCountdown();
                if ((AllPlayersReady && !AllPlayersNotWantToPlayNextLevel && _AllOwnMap))
                    SetCountdown(CountdownState.StartBeatmapCountdown, _configuration.CountdownConfig.BeatMapStartCountdownTime);
                else if (isReady && _AllOwnMap)
                    SetCountdown(CountdownState.CountingDown, _configuration.CountdownConfig.CountdownTimePlayersReady);
            }
            // If counting down
            if (CountDownState != CountdownState.NotCountingDown)
            {
                _logger.Debug($"CountdownEndTime '{CountdownEndTime}' RunTime '{_instance.RunTime}'");
                if(CountdownEndTime <= _instance.RunTime)
                {
                    _logger.Debug($"Countdown finished, sending map");
                    // If countdown just finished, send map then pause lobby untill all players have map downloaded
                    if (CountDownState != CountdownState.WaitingForEntitlement)
                    {
                        SetCountdown(CountdownState.WaitingForEntitlement);
                    }
                    if (_playerRegistry.Players.All(p => (p.GetEntitlement(SelectedBeatmap!.LevelId) is EntitlementStatus.Ok) || p.IsSpectating || !p.WantsToPlayNextLevel || p.ForceLateJoin))
                    {
                        foreach(IPlayer p in _playerRegistry.Players)
                        {
                            if (p.IsSpectating || !p.WantsToPlayNextLevel || p.ForceLateJoin)
                            {
                                _packetDispatcher.SendFromPlayer(p, new SetIsEntitledToLevelPacket()
                                {
                                    LevelId = SelectedBeatmap!.LevelId,
                                    Entitlement = EntitlementStatus.Ok
                                }, DeliveryMethod.ReliableOrdered);
                            }
                        }
                        _logger.Debug($"All players have entitlement, starting map");
                        //starts beatmap
                        _gameplayManager.SetBeatmap(SelectedBeatmap!, SelectedModifiers);
                        Task.Run(() => _gameplayManager.StartSong(CancellationToken.None));
                        //stops countdown
                        SetCountdown(CountdownState.NotCountingDown);
                        return;
                    }
                    if (CountdownEndTime + _configuration.SendPlayersWithoutEntitlementToSpectateTimeout <= _instance.RunTime) //If takes too long to start then players are sent to spectate by telling them the beatmap already started
                    {
                        _logger.Debug($"Took too long to start, sending players to spectate");
                        IPlayer[] MissingEntitlement = _playerRegistry.Players.Where(p => p.GetEntitlement(SelectedBeatmap!.LevelId) is not EntitlementStatus.Ok).ToArray();
                        foreach (IPlayer p in MissingEntitlement)
                        {
                            //Force the player to join late
                            p.ForceLateJoin = true;
                            _packetDispatcher.SendToPlayer(p, new CancelLevelStartPacket(), DeliveryMethod.ReliableOrdered);
                            _packetDispatcher.SendToPlayer(p, new SetIsReadyPacket() { IsReady = false }, DeliveryMethod.ReliableOrdered);
                        }
                    }
                }
                // If server owner/all players are no longer ready or not all players own beatmap or beatmap is null
                if (NotStartable || !_AllOwnMap || AllPlayersNotWantToPlayNextLevel || ((CountdownEndTime - _instance.RunTime) < _configuration.CountdownConfig.BeatMapStartCountdownTime && SelectedBeatmap == null))
                    CancelCountdown();
                else if (CountDownState == CountdownState.CountingDown && (AllPlayersReady || (CountdownEndTime - _instance.RunTime) < _configuration.CountdownConfig.BeatMapStartCountdownTime))
                    SetCountdown(CountdownState.StartBeatmapCountdown, _configuration.CountdownConfig.BeatMapStartCountdownTime);
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
                    }, DeliveryMethod.ReliableOrdered);
                else
                    _packetDispatcher.SendToNearbyPlayers(new ClearSelectedBeatmap(), DeliveryMethod.ReliableOrdered);
            }
            if (SelectedModifiers != modifiers)
            {
                SelectedModifiers = modifiers;
                if (SelectedModifiers != EmptyModifiers)
                    _packetDispatcher.SendToNearbyPlayers(new SetSelectedGameplayModifiers()
                    {
                        Modifiers = SelectedModifiers
                    }, DeliveryMethod.ReliableOrdered);
                else
                    _packetDispatcher.SendToNearbyPlayers(new ClearSelectedGameplayModifiers(), DeliveryMethod.ReliableOrdered);
            }
        }

        public BeatmapDifficulty[] GetSelectedBeatmapDifficulties()
        {
            if (!SelectedBeatmap!.LevelId.StartsWith("custom_level_"))
            {
                return Array.Empty<BeatmapDifficulty>();
            }
            foreach (var player in _playerRegistry.Players)
            {
                if(SelectedBeatmap!.LevelId == player.MapHash)
                {
                    return player.BeatmapDifficulties;
                }
            }
            return Array.Empty<BeatmapDifficulty>();
        }


        // Sets countdown and beatmap how the client would expect it to
        // If you want to cancel the countdown use CancelCountdown(), Not SetCountdown as CancelCountdown() ALSO informs the clients it has been canceled, whereas SetCountdown will not
        private void SetCountdown(CountdownState countdownState, long countdown = 0)
        {
            _logger.Error($"CountdownEndTime currently is '{CountdownEndTime}' countdown is set to '{countdown}' state will be set to '{countdownState}' BeatmapStartTime is '{_configuration.CountdownConfig.BeatMapStartCountdownTime}'");
            _logger.Error($"Check should start Beatmap {(CountdownEndTime - _instance.RunTime) < _configuration.CountdownConfig.BeatMapStartCountdownTime} EndTime '{CountdownEndTime - _instance.RunTime}' RunTime '{_instance.RunTime}'");
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
                    }, DeliveryMethod.ReliableOrdered);
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
            _logger.Error($"CountdownEndTime final set to '{CountdownEndTime}' CountdownState '{CountDownState}' countdown is '{countdown}' RunTime is '{_instance.RunTime}' BeatmapStartTime is '{_configuration.CountdownConfig.BeatMapStartCountdownTime}'");
        }

        //Checks the lobby settings and sends the player the correct beatmap
        private void StartBeatmapPacket()
        {
            if (!_configuration.AllowPerPlayerModifiers && !_configuration.AllowPerPlayerDifficulties)
            {
                _packetDispatcher.SendToNearbyPlayers(new StartLevelPacket
                {
                    Beatmap = SelectedBeatmap!,
                    Modifiers = SelectedModifiers,
                    StartTime = CountdownEndTime
                }, DeliveryMethod.ReliableOrdered);
                return;
            }
            BeatmapDifficulty[] diff = GetSelectedBeatmapDifficulties();
            BeatmapIdentifier bm = SelectedBeatmap!;
            foreach (var player in _playerRegistry.Players)
            {
                if (_configuration.AllowPerPlayerDifficulties && player.BeatmapIdentifier != null && diff.Contains(player.BeatmapIdentifier.Difficulty))
                    bm.Difficulty = player.BeatmapIdentifier.Difficulty;
                _packetDispatcher.SendToPlayer(player, new StartLevelPacket
                {
                    Beatmap = bm!,
                    Modifiers = _configuration.AllowPerPlayerModifiers ?  player.Modifiers : SelectedModifiers,
                    StartTime = CountdownEndTime
                }, DeliveryMethod.ReliableOrdered);
            }
        }

        private void CancelCountdown()
        {
            switch (CountDownState)
            {
                case CountdownState.CountingDown or CountdownState.NotCountingDown:
                    _packetDispatcher.SendToNearbyPlayers(new CancelCountdownPacket(), DeliveryMethod.ReliableOrdered);
                    break;
                case CountdownState.StartBeatmapCountdown or CountdownState.WaitingForEntitlement:
                    foreach (IPlayer player in _playerRegistry.Players) //This stays because players dont send they are un-ready after the level is canceled causing bad client behaviour
                    {
                        player.IsReady = false;
                    }
                    _packetDispatcher.SendToNearbyPlayers(new CancelLevelStartPacket(), DeliveryMethod.ReliableOrdered);
                    _packetDispatcher.SendToNearbyPlayers(new SetIsReadyPacket() { IsReady = false }, DeliveryMethod.ReliableOrdered);
                    break;
                default:
                    _logger.Warning("Canceling countdown when there is no countdown to cancel");
                    break;
            }
            SetCountdown(CountdownState.NotCountingDown);
        }

        private BeatmapIdentifier? GetSelectedBeatmap()
        {
            switch(_configuration.SongSelectionMode)
            {
                case SongSelectionMode.ServerOwnerPicks:
                    {
                        if(_playerRegistry.TryGetPlayer(_configuration.ServerOwnerId, out var p))
                            if(p.BeatmapIdentifier != null)
                            {
                                bool passed = ((!(p.Chroma && !_configuration.AllowChroma) || !(p.MappingExtensions && !_configuration.AllowMappingExtensions) || !(p.NoodleExtensions && !_configuration.AllowNoodleExtensions)) && p.MapHash == p.BeatmapIdentifier!.LevelId) || p.MapHash != p.BeatmapIdentifier!.LevelId;
                                if (passed)
                                    return p.BeatmapIdentifier;
                            }
                        return null;
                    }
                case SongSelectionMode.Vote:
                    Dictionary<BeatmapIdentifier, int> voteDictionary = new();
                    foreach (IPlayer player in _playerRegistry.Players.Where(p => p.BeatmapIdentifier != null&& (((!(p.Chroma && !_configuration.AllowChroma) || !(p.MappingExtensions && !_configuration.AllowMappingExtensions) || !(p.NoodleExtensions && !_configuration.AllowNoodleExtensions)) && p.MapHash == p.BeatmapIdentifier!.LevelId) || p.MapHash != p.BeatmapIdentifier!.LevelId)))
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
                        RandomlyPickedPlayer = _playerRegistry.Players[selectedPlayer].UserId;
                        return _playerRegistry.Players[selectedPlayer].BeatmapIdentifier;
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
            switch(_configuration.SongSelectionMode)
			{
                case SongSelectionMode.ServerOwnerPicks:
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
