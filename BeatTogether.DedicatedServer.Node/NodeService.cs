using Autobus;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Interface.Events;
using BeatTogether.DedicatedServer.Interface.Requests;
using BeatTogether.DedicatedServer.Interface.Responses;
using BeatTogether.DedicatedServer.Kernel.Encryption;
using BeatTogether.DedicatedServer.Node.Abstractions;
using BeatTogether.DedicatedServer.Node.Configuration;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Net;
using BeatTogether.DedicatedServer.Messaging.Models;

namespace BeatTogether.DedicatedServer.Node
{
    public sealed class NodeService : IMatchmakingService
    {
        private readonly NodeConfiguration _configuration;
        private readonly IInstanceFactory _instanceFactory;
        private readonly PacketEncryptionLayer _packetEncryptionLayer;
        private readonly IAutobus _autobus;
        private readonly ILogger _logger = Log.ForContext<NodeService>();

        public NodeService(
            NodeConfiguration configuration,
            IInstanceFactory instanceFactory,
            PacketEncryptionLayer packetEncryptionLayer,
            IAutobus autobus)
        {
            _configuration = configuration;
            _instanceFactory = instanceFactory;
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
                request.ServerName,
                request.resultScreenTime,
                request.BeatmapStartTime,
                request.PlayersReadyCountdownTime ,
                request.AllowPerPlayerModifiers,
                request.AllowPerPlayerDifficulties,
                request.AllowChroma,
                request.AllowME,
                request.AllowNE
                );
            if (matchmakingServer is null)
                return new CreateMatchmakingServerResponse(CreateMatchmakingServerError.NoAvailableSlots, string.Empty, Array.Empty<byte>(), Array.Empty<byte>());

            matchmakingServer.PlayerConnectedEvent += HandleUpdatePlayerEvent;
            matchmakingServer.PlayerDisconnectedEvent += HandlePlayerDisconnectEvent;
            matchmakingServer.PlayerDisconnectBeforeJoining += HandlePlayerLeaveBeforeJoining;
            matchmakingServer.StopEvent += HandleStopEvent;
            matchmakingServer.GameIsInLobby += HandleGameInLobbyEvent;
            matchmakingServer.UpdateInstanceEvent += HandleConfigChangeEvent;

            await matchmakingServer.Start();
            return new CreateMatchmakingServerResponse(
                CreateMatchmakingServerError.None,
                $"{_configuration.HostName}:{matchmakingServer.Port}",
                _packetEncryptionLayer.Random,
                _packetEncryptionLayer.KeyPair.PublicKey
            );
        }


        #region EventHandlers

        private void HandleGameInLobbyEvent(string secret, bool state)
        {
            _autobus.Publish(new ServerInGameplayEvent(secret, !state, string.Empty));
        }
        public static Interface.Models.GameplayModifiers GameplayCast(GameplayModifiers v)
        {
            return new Interface.Models.GameplayModifiers((Interface.Models.EnergyType)v.Energy, v.NoFailOn0Energy, v.DemoNoFail, v.InstaFail, v.FailOnSaberClash, (Interface.Models.EnabledObstacleType)v.EnabledObstacle, v.DemoNoObstacles, v.FastNotes, v.StrictAngles, v.DisappearingArrows, v.GhostNotes, v.NoBombs, (Interface.Models.SongSpeed)v.Speed, v.NoArrows, v.ProMode, v.ZenMode, v.SmallCubes);
        }
        public static Interface.Models.LevelCompletionResults LevelCompletionCast(LevelCompletionResults y)
        {
            return new(GameplayCast(y.GameplayModifiers), y.ModifiedScore, y.MultipliedScore, (Interface.Models.Rank)y.Rank, y.FullCombo, y.LeftSaberMovementDistance, y.RightSaberMovementDistance, y.LeftHandMovementDistance, y.RightHandMovementDistance, (Interface.Models.LevelEndStateType)y.LevelEndStateType, (Interface.Models.LevelEndAction)y.LevelEndAction, y.Energy, y.GoodCutsCount, y.BadCutsCount, y.MissedCount, y.NotGoodCount, y.OkCount, y.MaxCutScore, y.TotalCutScore, y.GoodCutsCountForNotesWithFullScoreScoringType, y.AverageCenterDistanceCutScoreForNotesWithFullScoreScoringType, y.AverageCutScoreForNotesWithFullScoreScoringType, y.MaxCombo, y.EndSongTime);
        }
        public static Interface.Models.AvatarData AvatarCast(AvatarData v)
        {
            return new(
                v.HeadTopId,
                v.HeadTopPrimaryColor,
                v.HeadTopSecondaryColor,
                v.GlassesId,
                v.GlassesColor,
                v.FacialHairId,
                v.FacialHairColor,
                v.HandsId,
                v.HandsColor,
                v.ClothesId,
                v.ClothesPrimaryColor,
                v.ClothesSecondaryColor,
                v.ClothesDetailColor,
                v.SkinColorId,
                v.EyesId,
                v.MouthId);
        }


        private void HandleConfigChangeEvent(IDedicatedInstance inst)
        {
            _autobus.Publish(new UpdateInstanceConfigEvent(
                inst._configuration.Secret,
                inst._configuration.ServerName,
                new Interface.Models.GameplayServerConfiguration(
                    inst._configuration.MaxPlayerCount,
                    (Interface.Enums.DiscoveryPolicy)inst._configuration.DiscoveryPolicy,
                    (Interface.Enums.InvitePolicy)inst._configuration.InvitePolicy,
                    (Interface.Enums.GameplayServerMode)inst._configuration.GameplayServerMode,
                    (Interface.Enums.SongSelectionMode)inst._configuration.SongSelectionMode,
                    (Interface.Enums.GameplayServerControlSettings)inst._configuration.GameplayServerControlSettings
                    )
                ));
        }

        private void HandleStopEvent(IDedicatedInstance inst)
        {
            _autobus.Publish(new MatchmakingServerStoppedEvent(inst._configuration.Secret));
        }
        private void HandleUpdatePlayerEvent(IPlayer player)
        {
            _autobus.Publish(new PlayerJoinEvent(player.Secret, ((IPEndPoint)player.Endpoint).ToString(), player.UserId));
        }
        private void HandlePlayerDisconnectEvent(IPlayer player)
        {
            _packetEncryptionLayer.RemoveEncryptedEndPoint((IPEndPoint)player.Endpoint);
            _autobus.Publish(new PlayerLeaveServerEvent(player.Secret, player.UserId, ((IPEndPoint)player.Endpoint).ToString()));
        }
        private void HandlePlayerLeaveBeforeJoining(string Secret, EndPoint endPoint, string[] Players)
        {
            _packetEncryptionLayer.RemoveEncryptedEndPoint((IPEndPoint)endPoint);
            _autobus.Publish(new UpdatePlayersEvent(Secret, Players));
        }
        #endregion
    }
}