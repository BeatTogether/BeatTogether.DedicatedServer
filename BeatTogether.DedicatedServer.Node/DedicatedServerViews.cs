using System;
using BeatTogether.DedicatedServer.Node.Abstractions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BeatTogether.DedicatedServer.Node;
using BeatTogether.DedicatedServer.Kernel;
using GalaSoft.MvvmLight.Messaging;
using WinFormsLibrary.Messages;
using BeatTogether.DedicatedServer.Kernel.Configuration;
using BeatTogether.DedicatedServer.Interface.Models;
using BeatTogether.DedicatedServer.Interface.Requests;
using Microsoft.Extensions.DependencyInjection;
using BeatTogether.DedicatedServer.Interface.Enums;
using BeatTogether.DedicatedServer.Interface;

namespace WinFormsLibrary
{
    public partial class DedicatedServerViews : Form
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMatchmakingService _nodeService;
        private readonly IInstanceRegistry _instanceRegistry;
        private readonly IInstanceFactory instanceFactory;

        public DedicatedServerViews(IServiceProvider _serviceProvider, IInstanceRegistry _instanceRegistry, IMatchmakingService _nodeService, IInstanceFactory instanceFactory)
        {
            InitializeComponent();
            this._serviceProvider = _serviceProvider;
            this._instanceRegistry = _instanceRegistry;
            this._nodeService = _nodeService;
            this.instanceFactory = instanceFactory;
            DedicatedServerInstances.DataSource = ((InstanceRegistry)_instanceRegistry)._instances.ToList();
            Messenger.Default.Register<UpdateForm>(this, (action) => FormRecieveMsg());
            Messenger.Default.Register<LobbyUpdateMessage>(this, (action) => FormRecieveLobbyMsg(action));

            UpdateScreen();
        }

        public void FormRecieveMsg() {
            BeginInvoke((Action)delegate ()
            {
                UpdateScreen();
            });
        }

        public void FormRecieveLobbyMsg(LobbyUpdateMessage lobbyUpdateMessage)
        {
            BeginInvoke((Action)delegate ()
            {
                UpdatePlayerList();
                UpdateInstanceStats(lobbyUpdateMessage.GetCountdownEndTime());
            });
        }

        public void UpdateScreen()
        {
            DedicatedServerInstances.DataSource = null;
            DedicatedServerInstances.DataSource = ((InstanceRegistry)_instanceRegistry)._instances.Keys.ToList();
            UpdatePlayerList();
            UpdateInstanceStats(0);
        }


        private void UpdateInstanceStats(float countdownendtime)
        {
            if (DedicatedServerInstances.SelectedValue != null)
            {
                if (((InstanceRegistry)_instanceRegistry).TryGetInstance(DedicatedServerInstances.SelectedValue.ToString()!, out var instance))
                {
                    var selectedInstance = (DedicatedInstance)instance; MaxPlayers.Text = selectedInstance.Configuration.MaxPlayerCount.ToString() + " Max players";
                    PlayerCount.Text = selectedInstance.GetPlayerRegistry().Players.Count.ToString() + " Players";
                    Port.Text = selectedInstance.Configuration.Port.ToString();
                    RunTime.Text = "Running for: " + selectedInstance.RunTime.ToString() + " Seconds";
                    GameState.Text = selectedInstance.State.ToString();
                    if (countdownendtime != 0)
                        Countdown.Text = (selectedInstance.RunTime - countdownendtime).ToString();
                    else
                        Countdown.Text = ("Not counting down");
                }
                else
                {
                    PlayerCount.Text = "NaN";
                    Port.Text = "NAN";
                    RunTime.Text = "NAN";
                    GameState.Text = "NAN";
                    Countdown.Text = "NAN";
                }
            }
        }

        private void UpdatePlayerList()
        {
            if(DedicatedServerInstances.SelectedValue != null)
            {
                if( ((InstanceRegistry)_instanceRegistry).TryGetInstance(DedicatedServerInstances.SelectedValue.ToString()!, out var instance))
                {
                    var selectedInstance = (DedicatedInstance)instance;

                    PlayersDataGrid.DataSource = selectedInstance.GetPlayerRegistry().Players;
                    PlayersDataGrid.Columns["Random"].Visible = false;
                    PlayersDataGrid.Columns["Instance"].Visible = false;
                    PlayersDataGrid.Columns["RemoteConnectionId"].Visible = false;
                    PlayersDataGrid.Columns["ConnectionId"].Visible = false;
                    PlayersDataGrid.Columns["PublicEncryptionKey"].Visible = false;
                    PlayersDataGrid.Columns["Latency"].Visible = false;
                    PlayersDataGrid.Columns["SyncTime"].Visible = false;
                    PlayersDataGrid.Columns["SortIndex"].Visible = false;
                    PlayersDataGrid.Columns["Avatar"].Visible = false;
                    PlayersDataGrid.Columns["BeatmapIdentifier"].Visible = false;
                    PlayersDataGrid.Columns["Modifiers"].Visible = false;
                    PlayersDataGrid.Columns["State"].Visible = false;
                    PlayersDataGrid.Columns["IsManager"].Visible = false;
                    PlayersDataGrid.Columns["CanRecommendBeatmaps"].Visible = false;
                    PlayersDataGrid.Columns["CanRecommendModifiers"].Visible = false;
                    PlayersDataGrid.Columns["CanKickVote"].Visible = false;
                    PlayersDataGrid.Columns["CanInvite"].Visible = false;
                    PlayersDataGrid.Columns["IsPlayer"].Visible = false;
                    PlayersDataGrid.Columns["IsBackgrounded"].Visible = false;
                    PlayersDataGrid.Columns["IsActive"].Visible = false;
                    PlayersDataGrid.Columns["InMenu"].Visible = false;
                    PlayersDataGrid.Columns["IsModded"].Visible = false;

                    PlayersDataGrid.Columns["UserName"].DisplayIndex = 0;
                    PlayersDataGrid.Columns["InLobby"].DisplayIndex = 1;
                    PlayersDataGrid.Columns["InLobby"].Width = 15;
                    PlayersDataGrid.Columns["IsReady"].DisplayIndex = 2;
                    PlayersDataGrid.Columns["IsReady"].Width = 15;
                    PlayersDataGrid.Columns["UserID"].DisplayIndex = 3;
                    PlayersDataGrid.Columns["IsSpectating"].DisplayIndex = 4;
                    PlayersDataGrid.Columns["IsSpectating"].Width = 15;
                    PlayersDataGrid.Columns["InGameplay"].DisplayIndex = 5;
                    PlayersDataGrid.Columns["InGameplay"].Width = 15;
                    PlayersDataGrid.Columns["WasActiveAtLevelStart"].DisplayIndex = 6;
                    PlayersDataGrid.Columns["WasActiveAtLevelStart"].Width = 15;



                    if (selectedInstance.GetPlayerRegistry().TryGetPlayer(selectedInstance.Configuration.ManagerId, out var player))
                        InstanceManager.Text = "Custom instance, Manager is: " + player.UserName;
                    else if (selectedInstance.Configuration.ManagerId == "ziuMSceapEuNN7wRGQXrZg")
                        InstanceManager.Text = "QUICKPLAY instance";
                    else InstanceManager.Text = "Creating Instance";
                }
                else
                {
                    PlayersDataGrid.DataSource = null;
                }
            }
            else
            {
                PlayersDataGrid.DataSource = null;
            }

        }

        private void AddInstanceButton_Click(object sender, EventArgs e)
        {
            var scope = _serviceProvider.CreateScope();


            GameplayServerConfiguration configuration = new GameplayServerConfiguration(10, DiscoveryPolicy.Public, InvitePolicy.AnyoneCanInvite, GameplayServerMode.Countdown, SongSelectionMode.Vote, GameplayServerControlSettings.All);
            CreateMatchmakingServerRequest request = new CreateMatchmakingServerRequest("SpecialServer", "ziuMSceapEuNN7wRGQXrZg", configuration);


            _ = ((NodeService)_nodeService).CreateMatchmakingServer(request);
        }

        private void StopSelectedInstance_Click(object sender, EventArgs e)
        {
            if (DedicatedServerInstances.SelectedValue != null)
            {
                if (((InstanceRegistry)_instanceRegistry).TryGetInstance(DedicatedServerInstances.SelectedValue.ToString()!, out var instance))
                ((DedicatedInstance)instance).StopDedicatedInstance();
            }

        }
    }
}
