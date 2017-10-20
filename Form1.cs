using System;
using System.ComponentModel;
using System.Drawing;						// Image
using System.IO;							// Path
using System.Windows.Forms;

//
// tested only on 32-bit XP


namespace TrayIconBuster {
	public partial class Form1 : Form {
		private uint runs;

		public Form1() {
			InitializeComponent();
			//ImageToIcon("..\\..\\TrayIconBuster.jpg");
			toggleAutoRun();	// start autorun
			Visible=false;		// exclude from task switching (WIN-TAB)
		}

		/// <summary>
		/// Converts an image file (e.g. 32*32 pixel JPEG) to an icon file
		/// </summary>
		/// <param name="filename"></param>
		public void ImageToIcon(string filename) {
			Bitmap bm=new Bitmap(filename);
			Icon icon=Icon.FromHandle(bm.GetHicon());
			filename=Path.ChangeExtension(filename, ".ico");
			Stream stream=new FileStream(filename, FileMode.Create);
			icon.Save(stream);
		}

		private void runEvery5SecondsToolStripMenuItem_Click(object sender, EventArgs e) {
			toggleAutoRun();
		}

		private void toggleAutoRun() {
			bool autoRunning=!runEvery5SecondsToolStripMenuItem.Checked;
			runEvery5SecondsToolStripMenuItem.Checked=autoRunning;
			if(autoRunning) autoRunTimer.Start();
			else autoRunTimer.Stop();
		}

		private void quitToolStripMenuItem_Click(object sender, EventArgs e) {
			Application.Exit();
		}

		private void runNowToolStripMenuItem_Click(object sender, EventArgs e) {
			RemovePhantomIcons();
		}

		/// <summary>
		/// The form's button calls the trayIconBuster (normally the main form
		/// is invisble)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnTrayIconBuster_Click(object sender, EventArgs e) {
			RemovePhantomIcons();
		}

		private void autoRunTimer_Tick(object sender, EventArgs e) {
			RemovePhantomIcons();
		}

		private void RemovePhantomIcons() {
			TrayIconBuster.RemovePhantomIcons();
			runs++;
			notifyIcon1.Text=(string)notifyIcon1.Tag+" ("+runs+" runs)";
		}
	}

}