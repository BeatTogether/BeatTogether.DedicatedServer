using System.Collections.Generic;
using System.Linq;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Messaging.Enums;
using BeatTogether.DedicatedServer.Messaging.Models;
using BeatTogether.DedicatedServer.Messaging.Packets.MultiplayerSession.MenuRpc;

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

        private BeatmapIdentifierNetSerializable? _startedBeatmap;
        private BeatmapIdentifierNetSerializable? _lastBeatmap;
        private GameplayModifiers _startedModifiers = new();
        private float _countdownEndTime;

        private IMatchmakingServer _server;
        private IPlayerRegistry _playerRegistry;
        private IPacketDispatcher _packetDispatcher;
        private IEntitlementManager _entitlementManager;

        public LobbyManager(
            IMatchmakingServer server,
            IPlayerRegistry playerRegistry,
            IPacketDispatcher packetDispatcher,
            IEntitlementManager entitlementManager)
        {
            _server = server;
            _playerRegistry = playerRegistry;
            _packetDispatcher = packetDispatcher;
            _entitlementManager = entitlementManager;
        }

        public void Update()
        {
            if (_server.State != MultiplayerGameState.Lobby)
                return;

            if (!_playerRegistry.TryGetPlayer(_server.ManagerId, out var manager))
                return;
            
            BeatmapIdentifierNetSerializable? beatmap = GetSelectedBeatmap();
            
            if (beatmap != null && beatmap != _startedBeatmap)
            {
                if (_lastBeatmap != beatmap)
                {
                    if (!_entitlementManager.AllPlayersHaveBeatmap(beatmap.LevelId))
                    {
                        var setPlayersMissingEntitlementsToLevelPacket = new SetPlayersMissingEntitlementsToLevelPacket
                        {
                            PlayersWithoutEntitlements = _entitlementManager.GetPlayersWithoutBeatmap(beatmap.LevelId)
                        };

                        var setIsStartButtonEnabledPacket = new SetIsStartButtonEnabledPacket
                        {
                            Reason = Messaging.Enums.CannotStartGameReason.DoNotOwnSong
                        };

                        _packetDispatcher.SendToNearbyPlayers(manager, setPlayersMissingEntitlementsToLevelPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                        _packetDispatcher.SendToNearbyPlayers(manager, setIsStartButtonEnabledPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);

                        _lastBeatmap = beatmap;
                        return;
                    }
                    else
                    {
                        var setPlayersMissingEntitlementsToLevelPacket = new SetPlayersMissingEntitlementsToLevelPacket
                        {
                            PlayersWithoutEntitlements = new List<string>()
                        };

                        var setIsStartButtonEnabledPacket = new SetIsStartButtonEnabledPacket
                        {
                            Reason = Messaging.Enums.CannotStartGameReason.None
                        };

                        _packetDispatcher.SendToNearbyPlayers(manager, setPlayersMissingEntitlementsToLevelPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                        _packetDispatcher.SendToNearbyPlayers(manager, setIsStartButtonEnabledPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                    }
                }

                if (_countdownEndTime != 0 && _countdownEndTime > _server.RunTime && _entitlementManager.AllPlayersHaveBeatmap(beatmap.LevelId))
                {
                    _server.State = MultiplayerGameState.Game;
                    return;
                }

                switch (_server.Configuration.SongSelectionMode)
                {
                    case SongSelectionMode.OwnerPicks:
                        bool isManagerReady = manager.IsReady;
                        if (_countdownEndTime == 0)
                        {
                            if (AllPlayersReady)
                            {
                                _countdownEndTime = _server.RunTime + CountdownTimeEveryoneReady;
                            }
                            else if (isManagerReady)
                            {
                                _countdownEndTime = _server.RunTime + CountdownTimeSomeReady;
                            }

                            if (AllPlayersReady || isManagerReady)
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

                                _packetDispatcher.SendToNearbyPlayers(manager, countdownEndTimePacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                                _packetDispatcher.SendToNearbyPlayers(manager, startLevelPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                            }
                        }
                        else
                        {
                            if (!isManagerReady)
                            {
                                _countdownEndTime = 0;

                                var cancelCountdownPacket = new CancelCountdownPacket();
                                var cancelLevelStartPacket = new CancelLevelStartPacket();
                                _packetDispatcher.SendToNearbyPlayers(manager, cancelCountdownPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                                _packetDispatcher.SendToNearbyPlayers(manager, cancelLevelStartPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                            }
                        }
                        break;
                    case SongSelectionMode.Vote:
                        if (_countdownEndTime == 0)
                        {
                            if (AllPlayersReady)
                                _countdownEndTime = _server.RunTime + CountdownTimeEveryoneReady;
                            if (SomePlayersReady)
                                _countdownEndTime = _server.RunTime + CountdownTimeSomeReady;

                            if (AllPlayersReady || SomePlayersReady)
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

                                _packetDispatcher.SendToNearbyPlayers(manager, countdownEndTimePacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                                _packetDispatcher.SendToNearbyPlayers(manager, startLevelPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                            }
                        }
                        else
                        {
                            if (NoPlayersReady)
                            {
                                _countdownEndTime = 0;

                                var cancelCountdownPacket = new CancelCountdownPacket();
                                var cancelLevelStartPacket = new CancelLevelStartPacket();
                                _packetDispatcher.SendToNearbyPlayers(manager, cancelCountdownPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                                _packetDispatcher.SendToNearbyPlayers(manager, cancelLevelStartPacket, LiteNetLib.DeliveryMethod.ReliableOrdered);
                            }
                        }
                        break;
                }
            }
            _lastBeatmap = beatmap;
        }

        public BeatmapIdentifierNetSerializable? GetSelectedBeatmap()
        {
            switch(_server.Configuration.SongSelectionMode)
            {
                case SongSelectionMode.OwnerPicks: return _playerRegistry.GetPlayer(_server.ManagerId).BeatmapIdentifier;
                case SongSelectionMode.Vote:
                    Dictionary<BeatmapIdentifierNetSerializable, int> voteDictionary = new();
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
    }
}
