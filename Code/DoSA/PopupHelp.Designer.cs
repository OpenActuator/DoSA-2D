namespace DoSA
{
    partial class PopupHelp
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonExampleDirectory = new System.Windows.Forms.Button();
            this.buttonSolenoidGuide = new System.Windows.Forms.Button();
            this.buttonVCMGuide = new System.Windows.Forms.Button();
            this.buttonDoSAUserGuide = new System.Windows.Forms.Button();
            this.buttonHelpClose = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonExampleDirectory);
            this.groupBox1.Controls.Add(this.buttonSolenoidGuide);
            this.groupBox1.Controls.Add(this.buttonVCMGuide);
            this.groupBox1.Controls.Add(this.buttonDoSAUserGuide);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(363, 215);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Help Lists";
            // 
            // buttonExampleDirectory
            // 
            this.buttonExampleDirectory.Location = new System.Drawing.Point(57, 162);
            this.buttonExampleDirectory.Name = "buttonExampleDirectory";
            this.buttonExampleDirectory.Size = new System.Drawing.Size(250, 31);
            this.buttonExampleDirectory.TabIndex = 4;
            this.buttonExampleDirectory.Text = "따라하기 예제 디렉토리";
            this.buttonExampleDirectory.UseVisualStyleBackColor = true;
            this.buttonExampleDirectory.Click += new System.EventHandler(this.buttonExampleDirectory_Click);
            // 
            // buttonSolenoidGuide
            // 
            this.buttonSolenoidGuide.Location = new System.Drawing.Point(57, 116);
            this.buttonSolenoidGuide.Name = "buttonSolenoidGuide";
            this.buttonSolenoidGuide.Size = new System.Drawing.Size(250, 31);
            this.buttonSolenoidGuide.TabIndex = 3;
            this.buttonSolenoidGuide.Text = "Solenoid 예제 따라하기";
            this.buttonSolenoidGuide.UseVisualStyleBackColor = true;
            this.buttonSolenoidGuide.Click += new System.EventHandler(this.buttonSolenoidGuide_Click);
            // 
            // buttonVCMGuide
            // 
            this.buttonVCMGuide.Location = new System.Drawing.Point(57, 70);
            this.buttonVCMGuide.Name = "buttonVCMGuide";
            this.buttonVCMGuide.Size = new System.Drawing.Size(250, 31);
            this.buttonVCMGuide.TabIndex = 2;
            this.buttonVCMGuide.Text = "VCM 예제 따라하기";
            this.buttonVCMGuide.UseVisualStyleBackColor = true;
            this.buttonVCMGuide.Click += new System.EventHandler(this.buttonVCMGuide_Click);
            // 
            // buttonDoSAUserGuide
            // 
            this.buttonDoSAUserGuide.Location = new System.Drawing.Point(57, 24);
            this.buttonDoSAUserGuide.Name = "buttonDoSAUserGuide";
            this.buttonDoSAUserGuide.Size = new System.Drawing.Size(250, 31);
            this.buttonDoSAUserGuide.TabIndex = 1;
            this.buttonDoSAUserGuide.Text = "DoSA 사용 설명서";
            this.buttonDoSAUserGuide.UseVisualStyleBackColor = true;
            this.buttonDoSAUserGuide.Click += new System.EventHandler(this.buttonDoSAUserGuide_Click);
            // 
            // buttonHelpClose
            // 
            this.buttonHelpClose.Location = new System.Drawing.Point(261, 233);
            this.buttonHelpClose.Name = "buttonHelpClose";
            this.buttonHelpClose.Size = new System.Drawing.Size(114, 36);
            this.buttonHelpClose.TabIndex = 13;
            this.buttonHelpClose.Text = "Close";
            this.buttonHelpClose.UseVisualStyleBackColor = true;
            this.buttonHelpClose.Click += new System.EventHandler(this.buttonHelpClose_Click);
            // 
            // PopupHelp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 276);
            this.Controls.Add(this.buttonHelpClose);
            this.Controls.Add(this.groupBox1);
            this.Name = "PopupHelp";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Help";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonHelpClose;
        private System.Windows.Forms.Button buttonDoSAUserGuide;
        private System.Windows.Forms.Button buttonSolenoidGuide;
        private System.Windows.Forms.Button buttonVCMGuide;
        private System.Windows.Forms.Button buttonExampleDirectory;
    }
}