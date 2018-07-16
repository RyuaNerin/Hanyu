using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace hanyu
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            this.RefreshList();
        }

        private void ctlRefresh_Click(object sender, EventArgs e)
        {
            this.RefreshList();
        }

        private void RefreshList()
        {
            this.ctlCerts.Enabled = this.ctlRefresh.Enabled = false;

            this.ctlCerts.Items.Clear();

            foreach (var cert in Cert.GetCerts())
            {
                var item = new ListViewItem
                {
                    Tag  = cert,
                    Text = cert.Name
                };

                item.SubItems.Add(cert.Type);
                item.SubItems.Add(cert.NotAfter .ToString("yy-MM-dd"));
                item.SubItems.Add(cert.Drive);
                item.SubItems.Add(cert.Ca);

                if (cert.NotAfter < DateTime.Now)
                    item.ForeColor = Color.Red;

                this.ctlCerts.Items.Add(item);
            }

            this.ctlCertsListDesc.Text = "설치된 인증서 : " + this.ctlCerts.Items.Count.ToString();

            this.ctlCerts.Enabled = this.ctlRefresh.Enabled = true;
        }

        private void ctlCerts_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var item = this.ctlCerts.GetItemAt(e.Location.X, e.Location.Y);
                if (item != null)
                    item.Selected = true;

                var cert = (Cert)item.Tag;

                this.ctlChangePassword.Enabled = this.ctlCertDelete.Enabled = cert.Editable;

                var pt = this.ctlCerts.PointToScreen(e.Location);
                this.ctlContextMenu.Show(pt);
            }
        }

        private void ctlCheckPassword_Click(object sender, EventArgs e)
        {
            if (this.ctlCerts.SelectedItems.Count != 1)
                return;

            using (var frm = new InputPassword())
            {
                if (frm.ShowDialog("비밀번호를 입력해주세요") != DialogResult.OK)
                    return;

                var cert = (Cert)this.ctlCerts.SelectedItems[0].Tag;

                var passwd = (string)frm.Tag;
                if (cert.CheckPassword(passwd))
                    MessageBox.Show(this, "비밀번호가 일치합니다!",        this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(this, "비밀번호가 일치하지 않습니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ctlChangePassword_Click(object sender, EventArgs e)
        {
            if (this.ctlCerts.SelectedItems.Count != 1)
                return;

            using (var frm = new InputPassword())
            {
                if (frm.ShowDialog("비밀번호를 입력해주세요") != DialogResult.OK)
                    return;

                var cert = (Cert)this.ctlCerts.SelectedItems[0].Tag;

                var oldPassword = (string)frm.Tag;
                if (!cert.CheckPassword(oldPassword))
                {
                    MessageBox.Show(this, "비밀번호가 일치하지 않습니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                if (frm.ShowDialog("새로운 비밀번호를 입력해주세요") != DialogResult.OK)
                    return;
                var newPassword = (string)frm.Tag;

                if (frm.ShowDialog("새로운 비밀번호를 다시 입력해주세요") != DialogResult.OK)
                    return;
                var newPassword2 = (string)frm.Tag;

                if (!newPassword.Equals(newPassword2, StringComparison.CurrentCulture))
                    MessageBox.Show(this, "두 비밀번호가 일치하지 않습니다!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                if (cert.ChangePassword(oldPassword, newPassword))
                    MessageBox.Show(this, "비밀번호를 변경하였습니다!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(this, "알 수 없는 오류가 발생하였습니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ctlCertMoveTo_Click(object sender, EventArgs e)
        {
            if (this.ctlCerts.SelectedItems.Count != 1)
                return;

            var cert = (Cert)this.ctlCerts.SelectedItems[0].Tag;

            using (var frm = new SelectDrive())
            {
                if (frm.ShowDialog(cert.Drive) != DialogResult.OK)
                    return;

                var newDrive = (string)frm.Tag;
                cert.MoveTo(newDrive);

                this.RefreshList();
            }
        }

        private void ctlCertCopyTo_Click(object sender, EventArgs e)
        {
            if (this.ctlCerts.SelectedItems.Count != 1)
                return;

            var cert = (Cert)this.ctlCerts.SelectedItems[0].Tag;

            using (var frm = new SelectDrive())
            {
                if (frm.ShowDialog(cert.Drive) != DialogResult.OK)
                    return;

                var newDrive = (string)frm.Tag;
                cert.CopyTo(newDrive);

                this.RefreshList();
            }
        }

        private void ctlCertDelete_Click(object sender, EventArgs e)
        {
            if (this.ctlCerts.SelectedItems.Count != 1)
                return;

            var cert = (Cert)this.ctlCerts.SelectedItems[0].Tag;

            if (MessageBox.Show(this, "정말 삭제하시겠습니까?\n삭제한 후에는 복구할 수 없습니다.", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            if (DateTime.Now < cert.NotAfter)
            {
                using (var frm = new InputPassword())
                {
                    if (frm.ShowDialog("비밀번호를 입력해주세요") != DialogResult.OK)
                        return;

                    var oldPassword = (string)frm.Tag;
                    if (!cert.CheckPassword(oldPassword))
                    {
                        MessageBox.Show(this, "비밀번호가 일치하지 않습니다.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                }
            }

            cert.Remove();
            this.RefreshList();
        }

        private void ctlCopyRight_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = "https://github.com/RyuaNerin/Hanyu", UseShellExecute = true }).Dispose();
                }
                catch
                {
                }
            }
        }
    }
}
