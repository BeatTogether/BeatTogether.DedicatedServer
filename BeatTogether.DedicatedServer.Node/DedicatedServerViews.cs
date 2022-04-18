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

namespace WinFormsLibrary
{
    public partial class DedicatedServerViews : Form
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IInstanceRegistry _instanceRegistry;


        public DedicatedServerViews(IServiceProvider _serviceProvider, IInstanceRegistry _instanceRegistry)
        {
            InitializeComponent();
            this._serviceProvider = _serviceProvider;
            this._instanceRegistry = _instanceRegistry;
            DedicatedServerInstances.DataSource = ((InstanceRegistry)_instanceRegistry)._instances.ToList();
            Messenger.Default.Register<Boolean>(this, (action) => FormRecieveMsg());
            UpdateScreen();
            /*
            Task.Factory.StartNew(() => {
                Messenger.Default.Send<Boolean>(true);
                
                
                //<String>("Hello World");

            });
            */
        }

        public void FormRecieveMsg() {
            BeginInvoke((Action)delegate ()
            {
                UpdateScreen();
            });

        }

        public void UpdateScreen()
        {

            DedicatedServerInstances.DataSource = null;
            DedicatedServerInstances.DataSource = ((InstanceRegistry)_instanceRegistry)._instances.Keys.ToList();
            UpdatePlayerList();

        }


        private void UpdatePlayerList()
        {
            if(DedicatedServerInstances.SelectedValue != null)
            {
                DedicatedInstance selectedInstance = (DedicatedInstance)((InstanceRegistry)_instanceRegistry).GetInstance(DedicatedServerInstances.SelectedValue.ToString()!);
                PlayersDataGrid.Hide();
                PlayersDataGrid.DataSource = selectedInstance.GetPlayerRegistry().Players;
                PlayersDataGrid.Columns["Random"].Visible = false;
                PlayersDataGrid.Columns["PublicEncryptionKey"].Visible = false;
                PlayersDataGrid.Columns["UserName"].DisplayIndex = 0;
                PlayersDataGrid.Show();
                if (selectedInstance.GetPlayerRegistry().Players.Count > 0)
                {
                    string managerId = selectedInstance.Configuration.ManagerId;
                    if (managerId == "ziuMSceapEuNN7wRGQXrZg") LobbyManager.Text = "QUICKPLAY";
                    else LobbyManager.Text = selectedInstance.GetPlayerRegistry().GetPlayer(managerId).UserName;
                }
            }

        }
    }
}
