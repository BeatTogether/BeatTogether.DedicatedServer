using BeatTogether.DedicatedServer.Kernel.Enums;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using BeatTogether.DedicatedServer.Messaging.Models;
using System.Linq;
using System.Collections.Generic;
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
        private GameplayModifiers _startedModifiers = new();
        private float _countdownEndTime;

        private IMatchmakingServer _server;
        private IPlayerRegistry _playerRegistry;
        private IPacketDispatcher _packetDispatcher;

        public LobbyManager(
            IMatchmakingServer server,
            IPlayerRegistry playerRegistry,
            IPacketDispatcher packetDispatcher)
        {
            _server = server;
            _playerRegistry = playerRegistry;
            _packetDispatcher = packetDispatcher;
        }

        public void Update()
        {
            IPlayer manager = _playerRegistry.GetPlayer(_server.ManagerId);
            BeatmapIdentifierNetSerializable? beatmap = GetSelectedBeatmap();
            
            if (beatmap != null && beatmap != _startedBeatmap)
            {
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
