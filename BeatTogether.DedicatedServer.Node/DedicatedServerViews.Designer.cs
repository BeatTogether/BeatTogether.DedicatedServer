namespace WinFormsLibrary
{
    partial class DedicatedServerViews
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.DedicatedServerInstances = new System.Windows.Forms.ListBox();
            this.InstanceManager = new System.Windows.Forms.Label();
            this.PlayersDataGrid = new System.Windows.Forms.DataGridView();
            this.InstanceInfo = new System.Windows.Forms.Label();
            this.MaxPlayers = new System.Windows.Forms.Label();
            this.Port = new System.Windows.Forms.Label();
            this.PlayerCount = new System.Windows.Forms.Label();
            this.RunTime = new System.Windows.Forms.Label();
            this.GameState = new System.Windows.Forms.Label();
            this.Countdown = new System.Windows.Forms.Label();
            this.AddInstanceButton = new System.Windows.Forms.Button();
            this.StopSelectedInstance = new System.Windows.Forms.Button();
            this.KickPlayerButton = new System.Windows.Forms.Button();
            this.RoomCodeTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.PlayersDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // DedicatedServerInstances
            // 
            this.DedicatedServerInstances.FormattingEnabled = true;
            this.DedicatedServerInstances.ItemHeight = 15;
            this.DedicatedServerInstances.Location = new System.Drawing.Point(12, 57);
            this.DedicatedServerInstances.Name = "DedicatedServerInstances";
            this.DedicatedServerInstances.Size = new System.Drawing.Size(274, 274);
            this.DedicatedServerInstances.TabIndex = 0;
            this.DedicatedServerInstances.SelectedIndexChanged += new System.EventHandler(this.DedicatedServerInstances_SelectedIndexChanged);
            // 
            // InstanceManager
            // 
            this.InstanceManager.AutoSize = true;
            this.InstanceManager.Location = new System.Drawing.Point(292, 12);
            this.InstanceManager.Name = "InstanceManager";
            this.InstanceManager.Size = new System.Drawing.Size(98, 15);
            this.InstanceManager.TabIndex = 1;
            this.InstanceManager.Text = "InstanceManager";
            // 
            // PlayersDataGrid
            // 
            this.PlayersDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.PlayersDataGrid.Location = new System.Drawing.Point(292, 30);
            this.PlayersDataGrid.Name = "PlayersDataGrid";
            this.PlayersDataGrid.RowTemplate.Height = 25;
            this.PlayersDataGrid.Size = new System.Drawing.Size(396, 301);
            this.PlayersDataGrid.TabIndex = 4;
            // 
            // InstanceInfo
            // 
            this.InstanceInfo.AutoSize = true;
            this.InstanceInfo.Location = new System.Drawing.Point(694, 30);
            this.InstanceInfo.Name = "InstanceInfo";
            this.InstanceInfo.Size = new System.Drawing.Size(75, 15);
            this.InstanceInfo.TabIndex = 5;
            this.InstanceInfo.Text = "Instance Info";
            // 
            // MaxPlayers
            // 
            this.MaxPlayers.AutoSize = true;
            this.MaxPlayers.Location = new System.Drawing.Point(694, 45);
            this.MaxPlayers.Name = "MaxPlayers";
            this.MaxPlayers.Size = new System.Drawing.Size(70, 15);
            this.MaxPlayers.TabIndex = 6;
            this.MaxPlayers.Text = "Max players";
            // 
            // Port
            // 
            this.Port.AutoSize = true;
            this.Port.Location = new System.Drawing.Point(694, 75);
            this.Port.Name = "Port";
            this.Port.Size = new System.Drawing.Size(29, 15);
            this.Port.TabIndex = 7;
            this.Port.Text = "Port";
            // 
            // PlayerCount
            // 
            this.PlayerCount.AutoSize = true;
            this.PlayerCount.Location = new System.Drawing.Point(694, 60);
            this.PlayerCount.Name = "PlayerCount";
            this.PlayerCount.Size = new System.Drawing.Size(44, 15);
            this.PlayerCount.TabIndex = 8;
            this.PlayerCount.Text = "Players";
            // 
            // RunTime
            // 
            this.RunTime.AutoSize = true;
            this.RunTime.Location = new System.Drawing.Point(694, 90);
            this.RunTime.Name = "RunTime";
            this.RunTime.Size = new System.Drawing.Size(124, 15);
            this.RunTime.TabIndex = 9;
            this.RunTime.Text = "Running for _ seconds";
            // 
            // GameState
            // 
            this.GameState.AutoSize = true;
            this.GameState.Location = new System.Drawing.Point(694, 105);
            this.GameState.Name = "GameState";
            this.GameState.Size = new System.Drawing.Size(64, 15);
            this.GameState.TabIndex = 10;
            this.GameState.Text = "GameState";
            // 
            // Countdown
            // 
            this.Countdown.AutoSize = true;
            this.Countdown.Location = new System.Drawing.Point(694, 120);
            this.Countdown.Name = "Countdown";
            this.Countdown.Size = new System.Drawing.Size(124, 15);
            this.Countdown.TabIndex = 11;
            this.Countdown.Text = "Countdown _ seconds";
            // 
            // AddInstanceButton
            // 
            this.AddInstanceButton.Location = new System.Drawing.Point(12, 12);
            this.AddInstanceButton.Name = "AddInstanceButton";
            this.AddInstanceButton.Size = new System.Drawing.Size(131, 40);
            this.AddInstanceButton.TabIndex = 12;
            this.AddInstanceButton.Text = "Add instance";
            this.AddInstanceButton.UseVisualStyleBackColor = true;
            this.AddInstanceButton.Click += new System.EventHandler(this.AddInstanceButton_Click);
            // 
            // StopSelectedInstance
            // 
            this.StopSelectedInstance.Location = new System.Drawing.Point(694, 291);
            this.StopSelectedInstance.Name = "StopSelectedInstance";
            this.StopSelectedInstance.Size = new System.Drawing.Size(226, 40);
            this.StopSelectedInstance.TabIndex = 13;
            this.StopSelectedInstance.Text = "Stop instance";
            this.StopSelectedInstance.UseVisualStyleBackColor = true;
            this.StopSelectedInstance.Click += new System.EventHandler(this.StopSelectedInstance_Click);
            // 
            // KickPlayerButton
            // 
            this.KickPlayerButton.Location = new System.Drawing.Point(694, 245);
            this.KickPlayerButton.Name = "KickPlayerButton";
            this.KickPlayerButton.Size = new System.Drawing.Size(226, 40);
            this.KickPlayerButton.TabIndex = 14;
            this.KickPlayerButton.Text = "Kick selected player";
            this.KickPlayerButton.UseVisualStyleBackColor = true;
            this.KickPlayerButton.Click += new System.EventHandler(this.KickPlayerButton_Click);
            // 
            // RoomCodeTextBox
            // 
            this.RoomCodeTextBox.Location = new System.Drawing.Point(149, 22);
            this.RoomCodeTextBox.MaxLength = 5;
            this.RoomCodeTextBox.Name = "RoomCodeTextBox";
            this.RoomCodeTextBox.Size = new System.Drawing.Size(137, 23);
            this.RoomCodeTextBox.TabIndex = 15;
            // 
            // DedicatedServerViews
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(932, 343);
            this.Controls.Add(this.RoomCodeTextBox);
            this.Controls.Add(this.KickPlayerButton);
            this.Controls.Add(this.StopSelectedInstance);
            this.Controls.Add(this.AddInstanceButton);
            this.Controls.Add(this.Countdown);
            this.Controls.Add(this.GameState);
            this.Controls.Add(this.RunTime);
            this.Controls.Add(this.PlayerCount);
            this.Controls.Add(this.Port);
            this.Controls.Add(this.MaxPlayers);
            this.Controls.Add(this.InstanceInfo);
            this.Controls.Add(this.PlayersDataGrid);
            this.Controls.Add(this.InstanceManager);
            this.Controls.Add(this.DedicatedServerInstances);
            this.Name = "DedicatedServerViews";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.PlayersDataGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox DedicatedServerInstances;
        private System.Windows.Forms.Label InstanceManager;
        private System.Windows.Forms.DataGridView PlayersDataGrid;
        private System.Windows.Forms.Label InstanceInfo;
        private System.Windows.Forms.Label MaxPlayers;
        private System.Windows.Forms.Label Port;
        private System.Windows.Forms.Label PlayerCount;
        private System.Windows.Forms.Label RunTime;
        private System.Windows.Forms.Label GameState;
        private System.Windows.Forms.Label Countdown;
        private System.Windows.Forms.Button AddInstanceButton;
        private System.Windows.Forms.Button StopSelectedInstance;
        private System.Windows.Forms.Button KickPlayerButton;
        private System.Windows.Forms.TextBox RoomCodeTextBox;
    }
}