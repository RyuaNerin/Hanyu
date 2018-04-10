using System.Windows.Forms;

namespace hanyu
{
	public partial class SelectDrive : Form
	{
        private struct DriveInfo
        {
            public string Name;
            public string Drive;

            public override string ToString()
            {
                return this.Name;
            }
        }

		public SelectDrive()
		{
			InitializeComponent();
        }

        private string m_oldDirve;
        public DialogResult ShowDialog(string oldDrive)
        {
            this.m_oldDirve = oldDrive;
            return this.ShowDialog();
        }

        private void RefreshDriveList()
        {
            this.ctlDrive.Items.Clear();
            foreach (var drive in System.IO.DriveInfo.GetDrives())
            {
                if (!drive.IsReady || drive.Name == this.m_oldDirve)
                    continue;

                try
                {
                    var st = new DriveInfo
                    {
                        Drive = drive.Name,
                        Name = string.IsNullOrWhiteSpace(drive.VolumeLabel) ? drive.Name : $"{drive.VolumeLabel} ({drive.Name})"
                    };

                    this.ctlDrive.Items.Add(st);
                }
                catch
                {
                }
            }
        }

        private void SelectDrive_Load(object sender, System.EventArgs e)
        {
            this.RefreshDriveList();
        }

        private void InputPassword_Activated(object sender, System.EventArgs e)
        {
            this.ctlDrive.Focus();
        }

        private const int WM_DEVICECHANGE = 0x0219;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_DEVICECHANGE)
                this.RefreshDriveList();

            base.WndProc(ref m);
        }

        private void ctlSelect_Click(object sender, System.EventArgs e)
        {
            if (this.ctlDrive.SelectedIndex >= 0)
            {
                this.Tag = ((DriveInfo)this.ctlDrive.SelectedItem).Drive;
                this.DialogResult = DialogResult.OK;
            }
            else
                this.DialogResult = DialogResult.Cancel;

            this.Close();
        }
    }
}
