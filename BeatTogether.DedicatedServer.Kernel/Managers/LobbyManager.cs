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

        private BeatmapIdentifier? _lastBeatmap;
        private bool _lastSpectatorState;
        private bool _lastAllOwnMap;          
        private string _lastManagerId = null!;
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
                //Sends players stuck in the lobby to spectate the ongoing game, prevents a rare quest issue with loss of tracking causing the game to pause on map start
                if (_playerRegistry.Players.Any(p => p.InLobby) && _instance.State == MultiplayerGameState.Game && _gameplayManager.State == GameplayManagerState.Gameplay)
                {
                    var InLobby = _playerRegistry.Players.FindAll(p => p.InLobby);
                    foreach (var p in InLobby)
                    {
                        if (p.InLobby && _gameplayManager.CurrentBeatmap != null)
                        {
                            _packetDispatcher.SendToPlayer(p, new StartLevelPacket
                            {
                                Beatmap = _gameplayManager.CurrentBeatmap!,
                                Modifiers = _gameplayManager.CurrentModifiers!,
                                StartTime = _instance.CountdownEndTime
                            }, DeliveryMethod.ReliableOrdered);
                            _packetDispatcher.SendToPlayer(p, new SetPlayersMissingEntitlementsToLevelPacket
                            {
                                PlayersWithoutEntitlements = _playerRegistry.Players
                                    .Where(p => p.GetEntitlement(_gameplayManager.CurrentBeatmap!.LevelId) is EntitlementStatus.NotOwned or EntitlementStatus.NotDownloaded)
                                    .Select(p => p.UserId).ToList()
                            }, DeliveryMethod.ReliableOrdered);
                            LevelFinishedPacket packet = new LevelFinishedPacket();
                            packet.Results.PlayerLevelEndState = MultiplayerPlayerLevelEndState.NotStarted;
                            packet.Results.LevelCompletionResults = new LevelCompletionResults();
                            packet.Results.PlayerLevelEndReason = MultiplayerPlayerLevelEndReason.StartupFailed;
                            _gameplayManager.HandleLevelFinished(p, packet);
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
                if (_lastBeatmap != beatmap || _lastAllOwnMap != allPlayersOwnBeatmap || _lastSpectatorState != AllPlayersSpectating || _lastManagerId != _configuration.ManagerId)
                {
                    // If not all players have beatmap
                    if (!allPlayersOwnBeatmap)
                    {
                        // Set players missing entitlements
                        _packetDispatcher.SendToNearbyPlayers(new SetPlayersMissingEntitlementsToLevelPacket
                        {
                            PlayersWithoutEntitlements = _playerRegistry.Players
                                .Where(p => p.GetEntitlement(beatmap.LevelId) is EntitlementStatus.NotOwned /*or EntitlementStatus.NotDownloaded*/)
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
                        CountingDown(manager!.IsReady, CountdownTimeManagerReady, !manager!.IsReady, allPlayersOwnBeatmap, beatmap, modifiers);
                        break;
                    case SongSelectionMode.Vote:
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
            if (_instance.CountdownEndTime == 0)
            {
                _instance.UpdateBeatmap(beatmap, modifiers);
                if ((AllPlayersReady && !AllPlayersSpectating && allPlayersOwnBeatmap))
                    _instance.SetCountdown(CountdownState.StartBeatmapCountdown);
                else if (isReady && allPlayersOwnBeatmap)
                    _instance.SetCountdown(CountdownState.CountingDown, CountDownTime);
            }
            // If counting down
            else
            {
                // If beatmap or modifiers changed, update them
                _instance.UpdateBeatmap(beatmap, modifiers);
                if (_instance.CountdownEndTime <= _instance.RunTime)
                {
                    // If countdown just finished, send map one last time then pause lobby untill all players have map downloaded
                    if (_instance.CountDownState != CountdownState.WaitingForEntitlement)
                        _instance.SetCountdown(CountdownState.WaitingForEntitlement);
                    if (_playerRegistry.Players.All(p => p.GetEntitlement(_instance.SelectedBeatmap!.LevelId) is EntitlementStatus.Ok))
                    {
                        // sends entitlements to players
                        _packetDispatcher.SendToNearbyPlayers(new SetPlayersMissingEntitlementsToLevelPacket
                        {
                            PlayersWithoutEntitlements = _playerRegistry.Players
                                .Where(p => p.GetEntitlement(_instance.SelectedBeatmap!.LevelId) is EntitlementStatus.NotOwned or EntitlementStatus.NotDownloaded)
                                .Select(p => p.UserId).ToList()
                        }, DeliveryMethod.ReliableOrdered);
                        // Starts beatmap
                        _gameplayManager.StartSong(_instance.SelectedBeatmap!, _instance.SelectedModifiers, CancellationToken.None);
                        //resets countdown
                        _instance.SetCountdown(CountdownState.NotCountingDown);
                        return;
                    }
                }
                // If manager/all players are no longer ready or not all players own beatmap
                if (NotStartable || !allPlayersOwnBeatmap)
                    _instance.CancelCountdown();
                else if (AllPlayersReady && (_instance.CountdownEndTime - _instance.RunTime) > CountdownTimeEveryoneReady)
                        _instance.SetCountdown(CountdownState.StartBeatmapCountdown);
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
