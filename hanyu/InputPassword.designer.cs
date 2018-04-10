namespace hanyu
{
	partial class InputPassword
    {
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.ctlPassword = new System.Windows.Forms.TextBox();
            this.ctlDescription = new System.Windows.Forms.Label();
            this.ctlPasswordLen = new System.Windows.Forms.Label();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.tableLayoutPanel1);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(533, 138);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.ctlPassword, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.ctlDescription, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.ctlPasswordLen, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(3);
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(131, 47);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // ctlPassword
            // 
            this.ctlPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlPassword.Location = new System.Drawing.Point(28, 21);
            this.ctlPassword.Margin = new System.Windows.Forms.Padding(0);
            this.ctlPassword.MaxLength = 32;
            this.ctlPassword.MinimumSize = new System.Drawing.Size(100, 4);
            this.ctlPassword.Name = "ctlPassword";
            this.ctlPassword.PasswordChar = '*';
            this.ctlPassword.Size = new System.Drawing.Size(100, 23);
            this.ctlPassword.TabIndex = 0;
            this.ctlPassword.TextChanged += new System.EventHandler(this.ctlPassword_TextChanged);
            this.ctlPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ctlPassword_KeyDown);
            // 
            // ctlDescription
            // 
            this.ctlDescription.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.ctlDescription, 2);
            this.ctlDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlDescription.Location = new System.Drawing.Point(3, 3);
            this.ctlDescription.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.ctlDescription.Name = "ctlDescription";
            this.ctlDescription.Size = new System.Drawing.Size(125, 15);
            this.ctlDescription.TabIndex = 6;
            this.ctlDescription.Text = "설명충";
            this.ctlDescription.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ctlPasswordLen
            // 
            this.ctlPasswordLen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlPasswordLen.Location = new System.Drawing.Point(3, 21);
            this.ctlPasswordLen.Margin = new System.Windows.Forms.Padding(0);
            this.ctlPasswordLen.Name = "ctlPasswordLen";
            this.ctlPasswordLen.Size = new System.Drawing.Size(25, 23);
            this.ctlPasswordLen.TabIndex = 1;
            this.ctlPasswordLen.Text = "0";
            this.ctlPasswordLen.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // InputPassword
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(533, 138);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "InputPassword";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Activated += new System.EventHandler(this.InputPassword_Activated);
            this.Shown += new System.EventHandler(this.InputPassword_Shown);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox ctlPassword;
        private System.Windows.Forms.Label ctlPasswordLen;
        private System.Windows.Forms.Label ctlDescription;
    }
}