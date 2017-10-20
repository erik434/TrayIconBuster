namespace TrayIconBuster {
	partial class Form1 {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.btnTrayIconBuster = new System.Windows.Forms.Button();
			this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.runEvery5SecondsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.autoRunTimer = new System.Windows.Forms.Timer(this.components);
			this.runNowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnTrayIconBuster
			// 
			this.btnTrayIconBuster.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.066667F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnTrayIconBuster.Location = new System.Drawing.Point(36, 36);
			this.btnTrayIconBuster.Name = "btnTrayIconBuster";
			this.btnTrayIconBuster.Size = new System.Drawing.Size(313, 65);
			this.btnTrayIconBuster.TabIndex = 0;
			this.btnTrayIconBuster.Text = "Remove Obsolete Tray Icons";
			this.btnTrayIconBuster.UseVisualStyleBackColor = true;
			this.btnTrayIconBuster.Click += new System.EventHandler(this.btnTrayIconBuster_Click);
			// 
			// notifyIcon1
			// 
			this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
			this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
			this.notifyIcon1.Tag = "LP#TrayIconBuster V2.0";
			this.notifyIcon1.Text = "LP#TrayIconBuster V2.0";
			this.notifyIcon1.Visible = true;
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runEvery5SecondsToolStripMenuItem,
            this.runNowToolStripMenuItem,
            this.quitToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(278, 88);
			// 
			// runEvery5SecondsToolStripMenuItem
			// 
			this.runEvery5SecondsToolStripMenuItem.Name = "runEvery5SecondsToolStripMenuItem";
			this.runEvery5SecondsToolStripMenuItem.Size = new System.Drawing.Size(277, 28);
			this.runEvery5SecondsToolStripMenuItem.Text = "Run Every 5 Seconds";
			this.runEvery5SecondsToolStripMenuItem.Click += new System.EventHandler(this.runEvery5SecondsToolStripMenuItem_Click);
			// 
			// quitToolStripMenuItem
			// 
			this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
			this.quitToolStripMenuItem.Size = new System.Drawing.Size(277, 28);
			this.quitToolStripMenuItem.Text = "Quit";
			this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
			// 
			// autoRunTimer
			// 
			this.autoRunTimer.Interval = 5000;
			this.autoRunTimer.Tick += new System.EventHandler(this.autoRunTimer_Tick);
			// 
			// runNowToolStripMenuItem
			// 
			this.runNowToolStripMenuItem.Name = "runNowToolStripMenuItem";
			this.runNowToolStripMenuItem.Size = new System.Drawing.Size(277, 28);
			this.runNowToolStripMenuItem.Text = "Run Now";
			this.runNowToolStripMenuItem.Click += new System.EventHandler(this.runNowToolStripMenuItem_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(385, 143);
			this.Controls.Add(this.btnTrayIconBuster);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Form1";
			this.ShowInTaskbar = false;
			this.Text = "Form1";
			this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
			this.contextMenuStrip1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnTrayIconBuster;
		private System.Windows.Forms.NotifyIcon notifyIcon1;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem runEvery5SecondsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
		private System.Windows.Forms.Timer autoRunTimer;
		private System.Windows.Forms.ToolStripMenuItem runNowToolStripMenuItem;
	}
}

