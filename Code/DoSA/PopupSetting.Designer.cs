namespace DoSA
{
    partial class PopupSetting
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
            this.groupBoxSetting = new System.Windows.Forms.GroupBox();
            this.textBoxFemmPath = new System.Windows.Forms.TextBox();
            this.buttonSelectFemmPath = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxWorkingDirectory = new System.Windows.Forms.TextBox();
            this.buttonSelectWorkingDirectory = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSettingOK = new System.Windows.Forms.Button();
            this.buttonSettingCancel = new System.Windows.Forms.Button();
            this.groupBoxSetting.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxSetting
            // 
            this.groupBoxSetting.Controls.Add(this.textBoxFemmPath);
            this.groupBoxSetting.Controls.Add(this.buttonSelectFemmPath);
            this.groupBoxSetting.Controls.Add(this.label3);
            this.groupBoxSetting.Controls.Add(this.textBoxWorkingDirectory);
            this.groupBoxSetting.Controls.Add(this.buttonSelectWorkingDirectory);
            this.groupBoxSetting.Controls.Add(this.label1);
            this.groupBoxSetting.Location = new System.Drawing.Point(12, 12);
            this.groupBoxSetting.Name = "groupBoxSetting";
            this.groupBoxSetting.Size = new System.Drawing.Size(506, 105);
            this.groupBoxSetting.TabIndex = 10;
            this.groupBoxSetting.TabStop = false;
            this.groupBoxSetting.Text = "Directories";
            // 
            // textBoxFemmPath
            // 
            this.textBoxFemmPath.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxFemmPath.Location = new System.Drawing.Point(165, 67);
            this.textBoxFemmPath.Name = "textBoxFemmPath";
            this.textBoxFemmPath.Size = new System.Drawing.Size(284, 21);
            this.textBoxFemmPath.TabIndex = 16;
            // 
            // buttonSelectFemmPath
            // 
            this.buttonSelectFemmPath.Location = new System.Drawing.Point(455, 65);
            this.buttonSelectFemmPath.Name = "buttonSelectFemmPath";
            this.buttonSelectFemmPath.Size = new System.Drawing.Size(27, 23);
            this.buttonSelectFemmPath.TabIndex = 17;
            this.buttonSelectFemmPath.Tag = "CFD-Post.exe Full Path";
            this.buttonSelectFemmPath.Text = "...";
            this.buttonSelectFemmPath.UseVisualStyleBackColor = true;
            this.buttonSelectFemmPath.Click += new System.EventHandler(this.buttonSelectFemmPath_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(24, 70);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 15, 3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(104, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "FEMM.exe Path :";
            // 
            // textBoxWorkingDirectory
            // 
            this.textBoxWorkingDirectory.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxWorkingDirectory.Location = new System.Drawing.Point(165, 30);
            this.textBoxWorkingDirectory.Name = "textBoxWorkingDirectory";
            this.textBoxWorkingDirectory.Size = new System.Drawing.Size(284, 21);
            this.textBoxWorkingDirectory.TabIndex = 3;
            // 
            // buttonSelectWorkingDirectory
            // 
            this.buttonSelectWorkingDirectory.Location = new System.Drawing.Point(455, 28);
            this.buttonSelectWorkingDirectory.Name = "buttonSelectWorkingDirectory";
            this.buttonSelectWorkingDirectory.Size = new System.Drawing.Size(27, 23);
            this.buttonSelectWorkingDirectory.TabIndex = 4;
            this.buttonSelectWorkingDirectory.Tag = "Client Base Directory";
            this.buttonSelectWorkingDirectory.Text = "...";
            this.buttonSelectWorkingDirectory.UseVisualStyleBackColor = true;
            this.buttonSelectWorkingDirectory.Click += new System.EventHandler(this.buttonSelectWorkingDirectory_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "Working Directory :";
            // 
            // buttonSettingOK
            // 
            this.buttonSettingOK.Location = new System.Drawing.Point(284, 153);
            this.buttonSettingOK.Name = "buttonSettingOK";
            this.buttonSettingOK.Size = new System.Drawing.Size(114, 36);
            this.buttonSettingOK.TabIndex = 12;
            this.buttonSettingOK.Text = "OK";
            this.buttonSettingOK.UseVisualStyleBackColor = true;
            this.buttonSettingOK.Click += new System.EventHandler(this.buttonSettingOK_Click);
            // 
            // buttonSettingCancel
            // 
            this.buttonSettingCancel.Location = new System.Drawing.Point(404, 153);
            this.buttonSettingCancel.Name = "buttonSettingCancel";
            this.buttonSettingCancel.Size = new System.Drawing.Size(114, 36);
            this.buttonSettingCancel.TabIndex = 11;
            this.buttonSettingCancel.Text = "Cancel";
            this.buttonSettingCancel.UseVisualStyleBackColor = true;
            this.buttonSettingCancel.Click += new System.EventHandler(this.buttonSettingCancel_Click);
            // 
            // PopupSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(528, 198);
            this.Controls.Add(this.groupBoxSetting);
            this.Controls.Add(this.buttonSettingOK);
            this.Controls.Add(this.buttonSettingCancel);
            this.Name = "PopupSetting";
            this.ShowIcon = false;
            this.Text = "Setting";
            this.groupBoxSetting.ResumeLayout(false);
            this.groupBoxSetting.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxSetting;
        private System.Windows.Forms.TextBox textBoxFemmPath;
        private System.Windows.Forms.Button buttonSelectFemmPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxWorkingDirectory;
        private System.Windows.Forms.Button buttonSelectWorkingDirectory;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonSettingOK;
        private System.Windows.Forms.Button buttonSettingCancel;
    }
}