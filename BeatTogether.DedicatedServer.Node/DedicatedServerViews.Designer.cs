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
            this.LobbyManager = new System.Windows.Forms.Label();
            this.PlayersDataGrid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.PlayersDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // DedicatedServerInstances
            // 
            this.DedicatedServerInstances.FormattingEnabled = true;
            this.DedicatedServerInstances.ItemHeight = 15;
            this.DedicatedServerInstances.Location = new System.Drawing.Point(12, 12);
            this.DedicatedServerInstances.Name = "DedicatedServerInstances";
            this.DedicatedServerInstances.Size = new System.Drawing.Size(274, 319);
            this.DedicatedServerInstances.TabIndex = 0;
            // 
            // LobbyManager
            // 
            this.LobbyManager.AutoSize = true;
            this.LobbyManager.Location = new System.Drawing.Point(316, 12);
            this.LobbyManager.Name = "LobbyManager";
            this.LobbyManager.Size = new System.Drawing.Size(87, 15);
            this.LobbyManager.TabIndex = 1;
            this.LobbyManager.Text = "LobbyManager";
            // 
            // PlayersDataGrid
            // 
            this.PlayersDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.PlayersDataGrid.Location = new System.Drawing.Point(316, 30);
            this.PlayersDataGrid.Name = "PlayersDataGrid";
            this.PlayersDataGrid.RowTemplate.Height = 25;
            this.PlayersDataGrid.Size = new System.Drawing.Size(596, 301);
            this.PlayersDataGrid.TabIndex = 4;
            // 
            // DedicatedServerViews
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(932, 343);
            this.Controls.Add(this.PlayersDataGrid);
            this.Controls.Add(this.LobbyManager);
            this.Controls.Add(this.DedicatedServerInstances);
            this.Name = "DedicatedServerViews";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.PlayersDataGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox DedicatedServerInstances;
        private System.Windows.Forms.Label LobbyManager;
        private System.Windows.Forms.DataGridView PlayersDataGrid;
    }
}