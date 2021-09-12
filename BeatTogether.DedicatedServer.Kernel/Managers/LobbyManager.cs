using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;
using LiteNetLib;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.Managers
{
    public sealed class LobbyManager : ILobbyManager
    {
        private const float CountdownTimeForeverAlone = 5f;
        private const float CountdownTimeSomeReady = 15.0f;
        private const float CountdownTimeEveryoneReady = 5.0f;
        private const float CountdownAfterGameplayCooldown = 5f;

        public bool AllPlayersReady => _playerRegistry.Players.All(p => p.IsReady || p.IsSpectating);
        public bool SomePlayersReady => _playerRegistry.Players.Any(p => p.IsReady);
        public bool NoPlayersReady => _playerRegistry.Players.All(p => !p.IsReady || p.IsSpectating);
        public bool AllPlayersSpectating => _playerRegistry.Players.All(p => p.IsSpectating);

        private BeatmapIdentifier? _startedBeatmap;
        private BeatmapIdentifier? _lastBeatmap;
        private GameplayModifiers _startedModifiers = new();
        private float _countdownEndTime;

        private bool _lastSpectatorState;
        private bool _lastEntitlementState;

        private IMatchmakingServer _server;
        private IPlayerRegistry _playerRegistry;
        private IPacketDispatcher _packetDispatcher;
        private IEntitlementManager _entitlementManager;
        private IGameplayManager _gameplayManager;
        private ILogger _logger = Log.ForContext<LobbyManager>();

        public LobbyManager(
            IMatchmakingServer server,
            IPlayerRegistry playerRegistry,
            IPacketDispatcher packetDispatcher,
            IEntitlementManager entitlementManager,
            IGameplayManager gameplayManager)
        {
            _server = server;
            _playerRegistry = playerRegistry;
            _packetDispatcher = packetDispatcher;
            _entitlementManager = entitlementManager;
            _gameplayManager = gameplayManager;
        }

        public void Update()
        {
            if (_server.State != MultiplayerGameState.Lobby)
                return;

            if (!_playerRegistry.TryGetPlayer(_server.ManagerId, out var manager))
                return;
            
            BeatmapIdentifier? beatmap = GetSelectedBeatmap();
            GameplayModifiers modifiers = GetSelectedModifiers();

            if (beatmap != null)
            {
                bool allPlayersOwnBeatmap = _entitlementManager.AllPlayersOwnBeatmap(beatmap.LevelId);

                // If new beatmap selected
                if (_lastBeatmap != beatmap || _lastEntitlementState != allPlayersOwnBeatmap)
                {
                    // If not all players have beatmap
                    if (!allPlayersOwnBeatmap)
                    {
                        // Set players missing entitlements
                        _packetDispatcher.SendToNearbyPlayers(new SetPlayersMissingEntitlementsToLevelPacket
                        {
                            PlayersWithoutEntitlements = _entitlementManager.GetPlayersWithoutBeatmap(beatmap.LevelId)
                        }, DeliveryMethod.ReliableOrdered);

                        // Cannot start song because losers dont have your map
                        _packetDispatcher.SendToNearbyPlayers(new SetIsStartButtonEnabledPacket
                        {
                            Reason =CannotStartGameReason.DoNotOwnSong
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

                        // Allow start map if all players aren't spectating
                        if (!AllPlayersSpectating)
                            _packetDispatcher.SendToNearbyPlayers(new SetIsStartButtonEnabledPacket
                            {
                                Reason = CannotStartGameReason.None
                            }, DeliveryMethod.ReliableOrdered);
                    }
                }

                _lastEntitlementState = allPlayersOwnBeatmap;

                // If counting down and countdown finished
                if (_countdownEndTime != 0 && _countdownEndTime < _server.RunTime)
				{
                    // If countdown just finished
                    if (_countdownEndTime != 1)
					{
                        // Set start level
                        _packetDispatcher.SendToNearbyPlayers(new StartLevelPacket
                        {
                            Beatmap = _startedBeatmap!,
                            Modifiers = _startedModifiers,
                            StartTime = _countdownEndTime
                        }, DeliveryMethod.ReliableOrdered);

                        _countdownEndTime = 1;
                    }

                    // If all players have map
                    if (_entitlementManager.AllPlayersHaveBeatmap(beatmap.LevelId))
					{
                        // Reset
                        _countdownEndTime = 0;
                        _startedBeatmap = null;
                        _startedModifiers = new();

                        // Start map
                        _gameplayManager.StartSong(beatmap, modifiers, CancellationToken.None);
                        return;
                    }
				}

                // Figure out if should be counting down and for how long
                switch (_server.Configuration.SongSelectionMode)
                {
                    case SongSelectionMode.OwnerPicks:
                        bool isManagerReady = manager.IsReady;

                        // If not already counting down
                        if (_countdownEndTime == 0)
                        {
                            if (AllPlayersReady && !AllPlayersSpectating)
                            {
                                _countdownEndTime = _server.RunTime + CountdownTimeEveryoneReady;
                            }
                            else if (isManagerReady)
                            {
                                _countdownEndTime = _server.RunTime + CountdownTimeSomeReady;
                            }

                            // If should be counting down, tell players
                            if ((AllPlayersReady && !AllPlayersSpectating) || isManagerReady)
                            {
                                _startedBeatmap = beatmap;
                                _startedModifiers = modifiers;
                                
                                // Set countdown end time
                                _packetDispatcher.SendToNearbyPlayers(new SetCountdownEndTimePacket
                                {
                                    CountdownTime = _countdownEndTime
                                }, DeliveryMethod.ReliableOrdered);
                            }
                        }

                        // If counting down
                        else
                        {
                            // If manager is no longer ready
                            if (!isManagerReady)
                            {
                                // Reset and stop counting down
                                _countdownEndTime = 0;
                                _packetDispatcher.SendToNearbyPlayers(new CancelCountdownPacket(), DeliveryMethod.ReliableOrdered);
                                _packetDispatcher.SendToNearbyPlayers(new CancelLevelStartPacket(), DeliveryMethod.ReliableOrdered);
							}

                            // If manager is still ready
							else
							{
                                // If all players are ready and countdown is too long
                                if (AllPlayersReady && _countdownEndTime - _server.RunTime > CountdownTimeEveryoneReady)
                                {
                                    // Shorten countdown time
                                    _countdownEndTime = _server.RunTime + CountdownTimeEveryoneReady;

                                    // Cancel countdown (bc of stupid client garbage)
                                    _packetDispatcher.SendToNearbyPlayers(new CancelCountdownPacket(), DeliveryMethod.ReliableOrdered);
                                    
                                    // Set countdown end time
                                    _packetDispatcher.SendToNearbyPlayers(new SetCountdownEndTimePacket
                                    {
                                        CountdownTime = _countdownEndTime
                                    }, DeliveryMethod.ReliableOrdered);
                                }
                            }
                        }
                        break;
                    case SongSelectionMode.Vote:
                        if (_countdownEndTime == 0)
                        {
                            if (AllPlayersReady && !AllPlayersSpectating)
                                _countdownEndTime = _server.RunTime + CountdownTimeEveryoneReady;
                            if (SomePlayersReady)
                                _countdownEndTime = _server.RunTime + CountdownTimeSomeReady;

                            if ((AllPlayersReady && !AllPlayersSpectating) || SomePlayersReady)
                            {
                                _startedBeatmap = beatmap;
                                _startedModifiers = manager.Modifiers;

                                var countdownEndTimePacket = new SetCountdownEndTimePacket
                                {
                                    CountdownTime = _countdownEndTime
                                };

                                var startLevelPacket = new StartLevelPacket
                                {
                                    Beatmap = _startedBeatmap,
                                    Modifiers = _startedModifiers,
                                    StartTime = _countdownEndTime
                                };

                                _packetDispatcher.SendToNearbyPlayers(countdownEndTimePacket, DeliveryMethod.ReliableOrdered);
                                _packetDispatcher.SendToNearbyPlayers(startLevelPacket, DeliveryMethod.ReliableOrdered);
                            }
                        }
                        else
                        {
                            if (NoPlayersReady)
                            {
                                _countdownEndTime = 0;

                                var cancelCountdownPacket = new CancelCountdownPacket();
                                var cancelLevelStartPacket = new CancelLevelStartPacket();
                                _packetDispatcher.SendToNearbyPlayers(cancelCountdownPacket, DeliveryMethod.ReliableOrdered);
                                _packetDispatcher.SendToNearbyPlayers(cancelLevelStartPacket, DeliveryMethod.ReliableOrdered);
                            }
                        }
                        break;
                }
            }
            
            // If beatmap is null and it wasn't previously
            else if (_lastBeatmap != beatmap)
			{
                // Cannot select song because no song is selected
                _packetDispatcher.SendToNearbyPlayers(new SetIsStartButtonEnabledPacket
                {
                    Reason = CannotStartGameReason.NoSongSelected
                }, DeliveryMethod.ReliableOrdered);
            }

            // If AllPlayersSpectating changed
            if (_lastSpectatorState != AllPlayersSpectating)
			{
                // Cannot start song because all players are spectating
                if (AllPlayersSpectating)
                    _packetDispatcher.SendToNearbyPlayers(new SetIsStartButtonEnabledPacket
                    {
                        Reason = CannotStartGameReason.AllPlayersSpectating
                    }, DeliveryMethod.ReliableOrdered);
            }

            _lastSpectatorState = AllPlayersSpectating;
            _lastBeatmap = beatmap;
        }

        public BeatmapIdentifier? GetSelectedBeatmap()
        {
            switch(_server.Configuration.SongSelectionMode)
            {
                case SongSelectionMode.OwnerPicks: return _playerRegistry.GetPlayer(_server.ManagerId).BeatmapIdentifier;
                case SongSelectionMode.Vote:
                    Dictionary<BeatmapIdentifier, int> voteDictionary = new();
                    _playerRegistry.Players.ForEach(p =>
                    {
                        if (p.BeatmapIdentifier != null)
                        {
                            if (voteDictionary.ContainsKey(p.BeatmapIdentifier))
                                voteDictionary[p.BeatmapIdentifier]++;
                            else
                                voteDictionary[p.BeatmapIdentifier] = 1;
                        }
                    });

                    var topBeatmap = voteDictionary.First();
                    voteDictionary.ToList().ForEach(beatmap =>
                    {
                        if (beatmap.Value > topBeatmap.Value)
                            topBeatmap = beatmap;
                    });
                    return topBeatmap.Key;
            };
            return null;
        }

        public GameplayModifiers GetSelectedModifiers()
		{
            switch(_server.Configuration.SongSelectionMode)
			{
                case SongSelectionMode.OwnerPicks: return _playerRegistry.GetPlayer(_server.ManagerId).Modifiers;
                case SongSelectionMode.Vote:
                    Dictionary<GameplayModifiers, int> voteDictionary = new();
                    _playerRegistry.Players.ForEach(p =>
                    {
                        if (p.Modifiers != null)
                        {
                            if (voteDictionary.ContainsKey(p.Modifiers))
                                voteDictionary[p.Modifiers]++;
                            else
                                voteDictionary[p.Modifiers] = 1;
                        }
                    });

                    var topModifiers = voteDictionary.First();
                    voteDictionary.ToList().ForEach(modifiers =>
                    {
                        if (modifiers.Value > topModifiers.Value)
                            topModifiers = modifiers;
                    });
                    return topModifiers.Key;
            };
            return new GameplayModifiers();
		}
    }
}
