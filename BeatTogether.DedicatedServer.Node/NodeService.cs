using Autobus;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Interface.Events;
using BeatTogether.DedicatedServer.Interface.Requests;
using BeatTogether.DedicatedServer.Interface.Responses;
using BeatTogether.DedicatedServer.Kernel.Encryption;
using BeatTogether.DedicatedServer.Node.Abstractions;
using BeatTogether.DedicatedServer.Node.Configuration;
using BeatTogether.DedicatedServer.Interface.Models;
using BeatTogether.DedicatedServer.Interface.Enums;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using Serilog;
using System;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Kernel.Managers.Abstractions;

namespace BeatTogether.DedicatedServer.Node
{
    public sealed class NodeService : IMatchmakingService
    {
        private readonly NodeConfiguration _configuration;
        private readonly IInstanceFactory _instanceFactory;
        private readonly IInstanceRegistry _instanceRegistry;
        private readonly PacketEncryptionLayer _packetEncryptionLayer;
        private readonly IAutobus _autobus;
        private readonly ILogger _logger = Log.ForContext<NodeService>();

        public NodeService(
            NodeConfiguration configuration,
            IInstanceFactory instanceFactory,
            IInstanceRegistry instanceRegistry,
            PacketEncryptionLayer packetEncryptionLayer,
            IAutobus autobus)
        {
            _configuration = configuration;
            _instanceFactory = instanceFactory;
            _instanceRegistry = instanceRegistry;
            _packetEncryptionLayer = packetEncryptionLayer;
            _autobus = autobus;
        }

        public async Task<CreateMatchmakingServerResponse> CreateMatchmakingServer(CreateMatchmakingServerRequest request)
        {
            _logger.Debug($"Received request to create matchmaking server. " +
                $"(Secret={request.Secret}, " +
                $"ManagerId={request.ManagerId}, " +
                $"MaxPlayerCount={request.Configuration.MaxPlayerCount}, " +
                $"DiscoveryPolicy={request.Configuration.DiscoveryPolicy}, " +
                $"InvitePolicy={request.Configuration.InvitePolicy}, " +
                $"GameplayServerMode={request.Configuration.GameplayServerMode}, " +
                $"SongSelectionMode={request.Configuration.SongSelectionMode}, " +
                $"GameplayServerControlSettings={request.Configuration.GameplayServerControlSettings})");

            var matchmakingServer = _instanceFactory.CreateInstance(
                request.Secret,
                request.ManagerId,
                request.Configuration,
                request.PermanentManager,
                request.Timeout,
                request.ServerName
            );
            if (matchmakingServer is null) // TODO: can also be no available slots
                return new CreateMatchmakingServerResponse(CreateMatchmakingServerError.InvalidSecret, string.Empty, Array.Empty<byte>(), Array.Empty<byte>());

            await matchmakingServer.Start();
            //_autobus.Publish(new MatchmakingServerStartedEvent(request.Secret, request.ManagerId, request.Configuration));//Tells the master server to add a server, NOT USED
            matchmakingServer.StopEvent += () => _autobus.Publish(new MatchmakingServerStoppedEvent(request.Secret));//Tells the master server when the newly added server has stopped

            return new CreateMatchmakingServerResponse(
                CreateMatchmakingServerError.None,
                $"{_configuration.HostName}:{matchmakingServer.Port}",
                _packetEncryptionLayer.Random,
                _packetEncryptionLayer.KeyPair.PublicKey
            );
        }

        public async Task<StopMatchmakingServerResponse> StopMatchmakingServer(StopMatchmakingServerRequest request)
        {
            if (_instanceRegistry.TryGetInstance(request.Secret, out var instance))
            {
                await instance.Stop();
                _autobus.Publish(new MatchmakingServerStoppedEvent(request.Secret));
                return new StopMatchmakingServerResponse(true);
            }
            return new StopMatchmakingServerResponse(false);
        }

        public Task<SimplePlayersListResponce> GetSimplePlayerList(GetPlayersSimpleRequest request)
        {
            if (_instanceRegistry.TryGetInstance(request.Secret, out var instance))
            {
                SimplePlayer[] simplePlayers = new SimplePlayer[instance.GetPlayerRegistry().Players.Count - 1];
                for (int i = 0; i < instance.GetPlayerRegistry().Players.Count - 1; i++)
                {
                    simplePlayers[i] = new SimplePlayer(instance.GetPlayerRegistry().Players[i].UserName, instance.GetPlayerRegistry().Players[i].UserId);
                }
                return Task.FromResult(new SimplePlayersListResponce(simplePlayers));
            }
            return Task.FromResult(new SimplePlayersListResponce(null));
        }

        public Task<AdvancedPlayersListResponce> GetAdvancedPlayerList(GetPlayersAdvancedRequest request)
        {
            if (_instanceRegistry.TryGetInstance(request.Secret, out var instance))
            {
                AdvancedPlayer[] advancedPlayers = new AdvancedPlayer[instance.GetPlayerRegistry().Players.Count - 1];
                for (int i = 0; i < instance.GetPlayerRegistry().Players.Count - 1; i++)
                {
                    IPlayer player = instance.GetPlayerRegistry().Players[i];
                    Beatmap beatmap;
                    if (player.BeatmapIdentifier != null)
                    {
                        beatmap = new(
                            player.BeatmapIdentifier.LevelId,
                            player.BeatmapIdentifier.Characteristic,
                            (BeatmapDifficulty)player.BeatmapIdentifier.Difficulty);
                    }
                    else
                    {
                        beatmap = new(
                            "NULL",
                            "NULL",
                            BeatmapDifficulty.Normal);
                    }

                    advancedPlayers[i] = new(
                        new SimplePlayer(
                            player.UserName,
                            player.UserId),
                        player.ConnectionId,
                        player.IsManager,
                        player.IsPlayer,
                        player.IsSpectating,
                        player.WantsToPlayNextLevel,
                        player.IsBackgrounded,
                        player.InGameplay,
                        player.WasActiveAtLevelStart,
                        player.IsActive,
                        player.FinishedLevel,
                        player.InMenu,
                        player.IsModded,
                        player.InLobby,
                        beatmap,
                        new GameplayModifiers((EnergyType)player.Modifiers.Energy,
                            player.Modifiers.NoFailOn0Energy,
                            player.Modifiers.DemoNoFail,
                            player.Modifiers.InstaFail,
                            player.Modifiers.FailOnSaberClash,
                            (EnabledObstacleType)player.Modifiers.EnabledObstacle,
                            player.Modifiers.DemoNoObstacles,
                            player.Modifiers.FastNotes,
                            player.Modifiers.StrictAngles,
                            player.Modifiers.DisappearingArrows,
                            player.Modifiers.GhostNotes,
                            player.Modifiers.NoBombs,
                            (SongSpeed)player.Modifiers.Speed,
                            player.Modifiers.NoArrows,
                            player.Modifiers.ProMode,
                            player.Modifiers.ZenMode,
                            player.Modifiers.SmallCubes),
                        player.CanRecommendBeatmaps,
                        player.CanRecommendModifiers,
                        player.CanKickVote,
                        player.CanInvite
                        );
                }
                return Task.FromResult(new AdvancedPlayersListResponce(advancedPlayers));
            }
            return Task.FromResult(new AdvancedPlayersListResponce(null));
        }

        public Task<AdvancedPlayerResponce> GetAdvancedPlayer(GetPlayerAdvancedRequest request)
        {
            if (_instanceRegistry.TryGetInstance(request.Secret, out var instance))
            {

                IPlayer player = instance.GetPlayerRegistry().GetPlayer(request.UserId);

                Beatmap beatmap;
                if (player.BeatmapIdentifier != null)
                {
                    beatmap = new(
                        player.BeatmapIdentifier.LevelId,
                        player.BeatmapIdentifier.Characteristic,
                        (BeatmapDifficulty)player.BeatmapIdentifier.Difficulty);
                }
                else
                {
                    beatmap = new(
                        "NULL",
                        "NULL",
                        BeatmapDifficulty.Normal);
                }

                AdvancedPlayer AdvancedPlayer = new(
                    new SimplePlayer(
                        player.UserName,
                        player.UserId),
                    player.ConnectionId,
                    player.IsManager,
                    player.IsPlayer,
                    player.IsSpectating,
                    player.WantsToPlayNextLevel,
                    player.IsBackgrounded,
                    player.InGameplay,
                    player.WasActiveAtLevelStart,
                    player.IsActive,
                    player.FinishedLevel,
                    player.InMenu,
                    player.IsModded,
                    player.InLobby,
                    beatmap,
                    new GameplayModifiers((EnergyType)player.Modifiers.Energy,
                        player.Modifiers.NoFailOn0Energy,
                        player.Modifiers.DemoNoFail,
                        player.Modifiers.InstaFail,
                        player.Modifiers.FailOnSaberClash,
                        (EnabledObstacleType)player.Modifiers.EnabledObstacle,
                        player.Modifiers.DemoNoObstacles,
                        player.Modifiers.FastNotes,
                        player.Modifiers.StrictAngles,
                        player.Modifiers.DisappearingArrows,
                        player.Modifiers.GhostNotes,
                        player.Modifiers.NoBombs,
                        (SongSpeed)player.Modifiers.Speed,
                        player.Modifiers.NoArrows,
                        player.Modifiers.ProMode,
                        player.Modifiers.ZenMode,
                        player.Modifiers.SmallCubes),
                    player.CanRecommendBeatmaps,
                    player.CanRecommendModifiers,
                    player.CanKickVote,
                    player.CanInvite
                    );
                return Task.FromResult(new AdvancedPlayerResponce(AdvancedPlayer));
            }
            return Task.FromResult(new AdvancedPlayerResponce(null));
        }

        public Task<KickPlayerResponse> KickPlayer(KickPlayerRequest request)
        {
            if (_instanceRegistry.TryGetInstance(request.Secret, out var instance))
            {
                instance.DisconnectPlayer(instance.GetPlayerRegistry().GetPlayer(request.UserId));                
                return Task.FromResult(new KickPlayerResponse(true));
            }
            return Task.FromResult(new KickPlayerResponse(false));
        }

        public Task<AdvancedInstanceResponce> GetAdvancedInstance(GetAdvancedInstanceRequest request)
        {
            if (_instanceRegistry.TryGetInstance(request.Secret, out var instance))
            {
                ILobbyManager lobby = (ILobbyManager)instance.GetServiceProvider().GetService(typeof(ILobbyManager))!;
                IGameplayManager GameplayManager = (IGameplayManager)instance.GetServiceProvider().GetService(typeof(IGameplayManager))!;

                GameplayServerConfiguration config = new(
                    instance.Configuration.MaxPlayerCount,
                    (DiscoveryPolicy)instance.Configuration.DiscoveryPolicy,
                    (InvitePolicy)instance.Configuration.InvitePolicy,
                    (GameplayServerMode)instance.Configuration.GameplayServerMode,
                    (SongSelectionMode)instance.Configuration.SongSelectionMode,
                    (GameplayServerControlSettings)instance.Configuration.GameplayServerControlSettings
                    );

                AdvancedInstance advancedInstance = new(
                    config,
                    instance.GetPlayerRegistry().Players.Count,
                    instance.IsRunning,
                    instance.RunTime,
                    instance.Port,
                    instance.UserId,
                    instance.UserName,
                    (MultiplayerGameState)instance.State,
                    (GameplayManagerState)GameplayManager.State,
                    instance.NoPlayersTime,
                    instance.DestroyInstanceTimeout,
                    instance.SetManagerFromUserId,
                    lobby.CountdownEndTime,
                    (CountdownState)lobby.CountDownState,
                    GetInstanceModifiers(instance, lobby, GameplayManager),
                    GetInstanceBeatmap(instance, lobby, GameplayManager));

                return Task.FromResult(new AdvancedInstanceResponce(advancedInstance));
            }
            return Task.FromResult(new AdvancedInstanceResponce(null));
        }

        private Beatmap GetInstanceBeatmap(IDedicatedInstance instance, ILobbyManager lobby, IGameplayManager GameplayManager)
        {
            Beatmap beatmap;
            switch (instance.State)
            {
                case Messaging.Enums.MultiplayerGameState.Lobby:
                    if (lobby.SelectedBeatmap != null)
                    {
                        beatmap = new(
                            lobby.SelectedBeatmap.LevelId,
                            lobby.SelectedBeatmap.Characteristic,
                            (BeatmapDifficulty)lobby.SelectedBeatmap.Difficulty);
                    }
                    else
                    {
                        beatmap = new(
                            "NULL",
                            "NULL",
                            BeatmapDifficulty.Normal);
                    }
                    break;
                case Messaging.Enums.MultiplayerGameState.Game:
                    if (GameplayManager.CurrentBeatmap != null)
                    {
                        beatmap = new(
                            GameplayManager.CurrentBeatmap.LevelId,
                            GameplayManager.CurrentBeatmap.Characteristic,
                            (BeatmapDifficulty)GameplayManager.CurrentBeatmap.Difficulty);
                    }
                    else
                    {
                        beatmap = new(
                            "NULL",
                            "NULL",
                            BeatmapDifficulty.Normal);
                    }
                    break;
                default:
                    beatmap = new(
                        "NULL",
                        "NULL",
                        BeatmapDifficulty.Normal);
                    break;
            }
            return beatmap;
        } //Not a request

        private GameplayModifiers GetInstanceModifiers(IDedicatedInstance instance, ILobbyManager lobby, IGameplayManager GameplayManager)
        {
            GameplayModifiers modifiers;
            switch (instance.State)
            {
                case Messaging.Enums.MultiplayerGameState.Lobby or Messaging.Enums.MultiplayerGameState.None:
                    modifiers = new((EnergyType)lobby.SelectedModifiers.Energy,
                        lobby.SelectedModifiers.NoFailOn0Energy,
                        lobby.SelectedModifiers.DemoNoFail,
                        lobby.SelectedModifiers.InstaFail,
                        lobby.SelectedModifiers.FailOnSaberClash,
                        (EnabledObstacleType)lobby.SelectedModifiers.EnabledObstacle,
                        lobby.SelectedModifiers.DemoNoObstacles,
                        lobby.SelectedModifiers.FastNotes,
                        lobby.SelectedModifiers.StrictAngles,
                        lobby.SelectedModifiers.DisappearingArrows,
                        lobby.SelectedModifiers.GhostNotes,
                        lobby.SelectedModifiers.NoBombs,
                        (SongSpeed)lobby.SelectedModifiers.Speed,
                        lobby.SelectedModifiers.NoArrows,
                        lobby.SelectedModifiers.ProMode,
                        lobby.SelectedModifiers.ZenMode,
                        lobby.SelectedModifiers.SmallCubes);
                    break;
                case Messaging.Enums.MultiplayerGameState.Game:
                    modifiers = new((EnergyType)GameplayManager.CurrentModifiers.Energy,
                        GameplayManager.CurrentModifiers.NoFailOn0Energy,
                        GameplayManager.CurrentModifiers.DemoNoFail,
                        GameplayManager.CurrentModifiers.InstaFail,
                        GameplayManager.CurrentModifiers.FailOnSaberClash,
                        (EnabledObstacleType)GameplayManager.CurrentModifiers.EnabledObstacle,
                        GameplayManager.CurrentModifiers.DemoNoObstacles,
                        GameplayManager.CurrentModifiers.FastNotes,
                        GameplayManager.CurrentModifiers.StrictAngles,
                        GameplayManager.CurrentModifiers.DisappearingArrows,
                        GameplayManager.CurrentModifiers.GhostNotes,
                        GameplayManager.CurrentModifiers.NoBombs,
                        (SongSpeed)GameplayManager.CurrentModifiers.Speed,
                        GameplayManager.CurrentModifiers.NoArrows,
                        GameplayManager.CurrentModifiers.ProMode,
                        GameplayManager.CurrentModifiers.ZenMode,
                        GameplayManager.CurrentModifiers.SmallCubes);
                    break;
                default:
                    modifiers = new((EnergyType)lobby.SelectedModifiers.Energy,
                        lobby.SelectedModifiers.NoFailOn0Energy,
                        lobby.SelectedModifiers.DemoNoFail,
                        lobby.SelectedModifiers.InstaFail,
                        lobby.SelectedModifiers.FailOnSaberClash,
                        (EnabledObstacleType)lobby.SelectedModifiers.EnabledObstacle,
                        lobby.SelectedModifiers.DemoNoObstacles,
                        lobby.SelectedModifiers.FastNotes,
                        lobby.SelectedModifiers.StrictAngles,
                        lobby.SelectedModifiers.DisappearingArrows,
                        lobby.SelectedModifiers.GhostNotes,
                        lobby.SelectedModifiers.NoBombs,
                        (SongSpeed)lobby.SelectedModifiers.Speed,
                        lobby.SelectedModifiers.NoArrows,
                        lobby.SelectedModifiers.ProMode,
                        lobby.SelectedModifiers.ZenMode,
                        lobby.SelectedModifiers.SmallCubes);
                    break;
            }
            return modifiers;
        } //Not a rrquest

        public Task<SetInstanceBeatmapResponse> SetInstanceBeatmap(SetInstanceBeatmapRequest request)
        {
            if (_instanceRegistry.TryGetInstance(request.Secret, out var instance))
            {
                ILobbyManager lobby = (ILobbyManager)instance.GetServiceProvider().GetService(typeof(ILobbyManager))!;

                if (request.countdownState != CountdownState.NotCountingDown  && request.countdownState != CountdownState.WaitingForEntitlement)
                {
                    Messaging.Models.BeatmapIdentifier beatmap = new();
                    beatmap.LevelId = request.beatmap.LevelId;
                    beatmap.Characteristic = request.beatmap.Characteristic;
                    beatmap.Difficulty = (Messaging.Models.BeatmapDifficulty)request.beatmap.Difficulty;
                    Messaging.Models.GameplayModifiers gameplayModifiers = new();
                    gameplayModifiers.Energy = (Messaging.Models.GameplayModifiers.EnergyType)request.modifiers.Energy;
                    gameplayModifiers.NoFailOn0Energy = request.modifiers.NoFailOn0Energy;
                    gameplayModifiers.DemoNoFail = request.modifiers.DemoNoFail;
                    gameplayModifiers.InstaFail = request.modifiers.InstaFail;
                    gameplayModifiers.FailOnSaberClash = request.modifiers.FailOnSaberClash;
                    gameplayModifiers.EnabledObstacle = (Messaging.Models.GameplayModifiers.EnabledObstacleType)request.modifiers.EnabledObstacle;
                    gameplayModifiers.DemoNoObstacles = request.modifiers.DemoNoObstacles;
                    gameplayModifiers.FastNotes = request.modifiers.FastNotes;
                    gameplayModifiers.StrictAngles = request.modifiers.StrictAngles;
                    gameplayModifiers.DisappearingArrows = request.modifiers.DisappearingArrows;
                    gameplayModifiers.GhostNotes = request.modifiers.GhostNotes;
                    gameplayModifiers.NoBombs = request.modifiers.NoBombs;
                    gameplayModifiers.Speed = (Messaging.Models.GameplayModifiers.SongSpeed)request.modifiers.Speed;
                    gameplayModifiers.NoArrows = request.modifiers.NoArrows;
                    gameplayModifiers.ProMode = request.modifiers.ProMode;
                    gameplayModifiers.ZenMode = request.modifiers.ZenMode;
                    gameplayModifiers.SmallCubes = request.modifiers.SmallCubes;
                    lobby.UpdateBeatmap(beatmap, gameplayModifiers);
                    lobby.SetCountdown((Kernel.Enums.CountdownState)request.countdownState, request.countdown);
                }
                else
                {
                    lobby.UpdateBeatmap(null, new());
                    lobby.SetCountdown(Kernel.Enums.CountdownState.NotCountingDown, 0);
                }
                if(request.countdownState == CountdownState.WaitingForEntitlement)
                {
                    return Task.FromResult(new SetInstanceBeatmapResponse(false));
                }
                return Task.FromResult(new SetInstanceBeatmapResponse(true));
            }
            return Task.FromResult(new SetInstanceBeatmapResponse(false));
        }
    }
}