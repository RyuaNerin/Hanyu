using System.Windows.Forms;

namespace hanyu
{
    public partial class InputPassword : Form
	{
		public InputPassword()
		{
			InitializeComponent();
        }

        public DialogResult ShowDialog(string desc)
        {
            this.ctlDescription.Text = desc;
            return this.ShowDialog();
        }

        private void InputPassword_Shown(object sender, System.EventArgs e)
        {
            this.ctlPassword.Text = string.Empty;
        }

        private void InputPassword_Activated(object sender, System.EventArgs e)
        {
            this.ctlPassword.Focus();
        }

        private void ctlPassword_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
                this.Tag = this.ctlPassword.Text;
				this.DialogResult = this.ctlPassword.TextLength > 0 ? DialogResult.OK : DialogResult.None;
				this.Close();
			}
		}

		private void ctlPassword_TextChanged(object sender, System.EventArgs e)
		{
			this.ctlPasswordLen.Text = this.ctlPassword.TextLength.ToString();
		}
    }
}
