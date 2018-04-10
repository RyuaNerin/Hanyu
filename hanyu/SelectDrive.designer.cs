namespace hanyu
{
	partial class SelectDrive
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
            this.ctlDescription = new System.Windows.Forms.Label();
            this.ctlSelect = new System.Windows.Forms.Button();
            this.ctlDrive = new System.Windows.Forms.ComboBox();
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
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(533, 138);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.ctlDescription, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.ctlSelect, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.ctlDrive, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(3);
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(220, 49);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // ctlDescription
            // 
            this.ctlDescription.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.ctlDescription, 2);
            this.ctlDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlDescription.Location = new System.Drawing.Point(3, 3);
            this.ctlDescription.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.ctlDescription.Name = "ctlDescription";
            this.ctlDescription.Size = new System.Drawing.Size(214, 15);
            this.ctlDescription.TabIndex = 6;
            this.ctlDescription.Text = "어디로 복사할까요?";
            this.ctlDescription.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ctlSelect
            // 
            this.ctlSelect.AutoSize = true;
            this.ctlSelect.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ctlSelect.Dock = System.Windows.Forms.DockStyle.Left;
            this.ctlSelect.Location = new System.Drawing.Point(156, 21);
            this.ctlSelect.Margin = new System.Windows.Forms.Padding(0);
            this.ctlSelect.Name = "ctlSelect";
            this.ctlSelect.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.ctlSelect.Size = new System.Drawing.Size(61, 25);
            this.ctlSelect.TabIndex = 7;
            this.ctlSelect.Text = "선택";
            this.ctlSelect.UseVisualStyleBackColor = true;
            this.ctlSelect.Click += new System.EventHandler(this.ctlSelect_Click);
            // 
            // ctlDrive
            // 
            this.ctlDrive.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlDrive.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ctlDrive.FormattingEnabled = true;
            this.ctlDrive.Location = new System.Drawing.Point(3, 22);
            this.ctlDrive.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            this.ctlDrive.MinimumSize = new System.Drawing.Size(150, 0);
            this.ctlDrive.Name = "ctlDrive";
            this.ctlDrive.Size = new System.Drawing.Size(150, 23);
            this.ctlDrive.TabIndex = 8;
            // 
            // SelectDrive
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
            this.Name = "SelectDrive";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Activated += new System.EventHandler(this.InputPassword_Activated);
            this.Load += new System.EventHandler(this.SelectDrive_Load);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label ctlDescription;
        private System.Windows.Forms.Button ctlSelect;
        private System.Windows.Forms.ComboBox ctlDrive;
    }
}