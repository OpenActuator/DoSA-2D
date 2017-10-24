namespace Shapes
{
    partial class PopupShape
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
            this.panelBotton = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.panelFaceType = new System.Windows.Forms.Panel();
            this.textBoxPartName = new System.Windows.Forms.TextBox();
            this.labelPartName = new System.Windows.Forms.Label();
            this.comboBoxFaceType = new System.Windows.Forms.ComboBox();
            this.labelFaceType = new System.Windows.Forms.Label();
            this.panelPointLine = new System.Windows.Forms.Panel();
            this.labelX = new System.Windows.Forms.Label();
            this.labelArc = new System.Windows.Forms.Label();
            this.labelNo = new System.Windows.Forms.Label();
            this.labelDirection = new System.Windows.Forms.Label();
            this.labelY = new System.Windows.Forms.Label();
            this.labelDelete = new System.Windows.Forms.Label();
            this.labelAdd = new System.Windows.Forms.Label();
            this.panelTitle = new System.Windows.Forms.Panel();
            this.buttonDraw = new System.Windows.Forms.Button();
            this.panelBotton.SuspendLayout();
            this.panelFaceType.SuspendLayout();
            this.panelTitle.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBotton
            // 
            this.panelBotton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBotton.BackColor = System.Drawing.SystemColors.Control;
            this.panelBotton.Controls.Add(this.buttonDraw);
            this.panelBotton.Controls.Add(this.buttonCancel);
            this.panelBotton.Controls.Add(this.buttonOK);
            this.panelBotton.Location = new System.Drawing.Point(7, 441);
            this.panelBotton.Name = "panelBotton";
            this.panelBotton.Size = new System.Drawing.Size(330, 49);
            this.panelBotton.TabIndex = 1;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(98)))), ((int)(((byte)(118)))), ((int)(((byte)(145)))));
            this.buttonCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(188)))), ((int)(((byte)(215)))));
            this.buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(188)))), ((int)(((byte)(215)))));
            this.buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(148)))), ((int)(((byte)(175)))));
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.buttonCancel.Location = new System.Drawing.Point(225, 7);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(95, 35);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = false;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(98)))), ((int)(((byte)(118)))), ((int)(((byte)(145)))));
            this.buttonOK.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(188)))), ((int)(((byte)(215)))));
            this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(188)))), ((int)(((byte)(215)))));
            this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(148)))), ((int)(((byte)(175)))));
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.buttonOK.Location = new System.Drawing.Point(124, 7);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(95, 35);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = false;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // panelFaceType
            // 
            this.panelFaceType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelFaceType.BackColor = System.Drawing.SystemColors.Control;
            this.panelFaceType.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFaceType.Controls.Add(this.textBoxPartName);
            this.panelFaceType.Controls.Add(this.labelPartName);
            this.panelFaceType.Controls.Add(this.comboBoxFaceType);
            this.panelFaceType.Controls.Add(this.labelFaceType);
            this.panelFaceType.Location = new System.Drawing.Point(7, 7);
            this.panelFaceType.Name = "panelFaceType";
            this.panelFaceType.Size = new System.Drawing.Size(330, 75);
            this.panelFaceType.TabIndex = 12;
            // 
            // textBoxPartName
            // 
            this.textBoxPartName.Location = new System.Drawing.Point(102, 9);
            this.textBoxPartName.Name = "textBoxPartName";
            this.textBoxPartName.Size = new System.Drawing.Size(150, 21);
            this.textBoxPartName.TabIndex = 14;
            // 
            // labelPartName
            // 
            this.labelPartName.AutoSize = true;
            this.labelPartName.Location = new System.Drawing.Point(9, 13);
            this.labelPartName.Name = "labelPartName";
            this.labelPartName.Size = new System.Drawing.Size(73, 12);
            this.labelPartName.TabIndex = 13;
            this.labelPartName.Text = "Part Name :";
            // 
            // comboBoxFaceType
            // 
            this.comboBoxFaceType.FormattingEnabled = true;
            this.comboBoxFaceType.Items.AddRange(new object[] {
            "RECTANGLE",
            "POLYGON"});
            this.comboBoxFaceType.Location = new System.Drawing.Point(102, 42);
            this.comboBoxFaceType.Name = "comboBoxFaceType";
            this.comboBoxFaceType.Size = new System.Drawing.Size(150, 20);
            this.comboBoxFaceType.TabIndex = 4;
            this.comboBoxFaceType.SelectedIndexChanged += new System.EventHandler(this.comboBoxdrawType_SelectedIndexChanged);
            // 
            // labelFaceType
            // 
            this.labelFaceType.AutoSize = true;
            this.labelFaceType.Location = new System.Drawing.Point(8, 46);
            this.labelFaceType.Name = "labelFaceType";
            this.labelFaceType.Size = new System.Drawing.Size(74, 12);
            this.labelFaceType.TabIndex = 12;
            this.labelFaceType.Text = "Face Type :";
            // 
            // panelPointLine
            // 
            this.panelPointLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPointLine.AutoScroll = true;
            this.panelPointLine.BackColor = System.Drawing.SystemColors.Control;
            this.panelPointLine.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPointLine.Location = new System.Drawing.Point(7, 129);
            this.panelPointLine.Name = "panelPointLine";
            this.panelPointLine.Size = new System.Drawing.Size(330, 306);
            this.panelPointLine.TabIndex = 14;
            // 
            // labelX
            // 
            this.labelX.AutoSize = true;
            this.labelX.Location = new System.Drawing.Point(67, 11);
            this.labelX.Name = "labelX";
            this.labelX.Size = new System.Drawing.Size(13, 12);
            this.labelX.TabIndex = 12;
            this.labelX.Text = "X";
            // 
            // labelArc
            // 
            this.labelArc.AutoSize = true;
            this.labelArc.Location = new System.Drawing.Point(197, 11);
            this.labelArc.Name = "labelArc";
            this.labelArc.Size = new System.Drawing.Size(24, 12);
            this.labelArc.TabIndex = 12;
            this.labelArc.Text = "Arc";
            // 
            // labelNo
            // 
            this.labelNo.AutoSize = true;
            this.labelNo.Location = new System.Drawing.Point(9, 11);
            this.labelNo.Name = "labelNo";
            this.labelNo.Size = new System.Drawing.Size(25, 12);
            this.labelNo.TabIndex = 12;
            this.labelNo.Text = "No.";
            // 
            // labelDirection
            // 
            this.labelDirection.AutoSize = true;
            this.labelDirection.Location = new System.Drawing.Point(228, 11);
            this.labelDirection.Name = "labelDirection";
            this.labelDirection.Size = new System.Drawing.Size(24, 12);
            this.labelDirection.TabIndex = 12;
            this.labelDirection.Text = "Dir.";
            // 
            // labelY
            // 
            this.labelY.AutoSize = true;
            this.labelY.Location = new System.Drawing.Point(140, 11);
            this.labelY.Name = "labelY";
            this.labelY.Size = new System.Drawing.Size(13, 12);
            this.labelY.TabIndex = 12;
            this.labelY.Text = "Y";
            // 
            // labelDelete
            // 
            this.labelDelete.AutoSize = true;
            this.labelDelete.Location = new System.Drawing.Point(259, 11);
            this.labelDelete.Name = "labelDelete";
            this.labelDelete.Size = new System.Drawing.Size(27, 12);
            this.labelDelete.TabIndex = 12;
            this.labelDelete.Text = "Del.";
            // 
            // labelAdd
            // 
            this.labelAdd.AutoSize = true;
            this.labelAdd.Location = new System.Drawing.Point(293, 11);
            this.labelAdd.Name = "labelAdd";
            this.labelAdd.Size = new System.Drawing.Size(27, 12);
            this.labelAdd.TabIndex = 12;
            this.labelAdd.Text = "Add";
            // 
            // panelTitle
            // 
            this.panelTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTitle.BackColor = System.Drawing.SystemColors.Control;
            this.panelTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTitle.Controls.Add(this.labelAdd);
            this.panelTitle.Controls.Add(this.labelDelete);
            this.panelTitle.Controls.Add(this.labelY);
            this.panelTitle.Controls.Add(this.labelDirection);
            this.panelTitle.Controls.Add(this.labelNo);
            this.panelTitle.Controls.Add(this.labelArc);
            this.panelTitle.Controls.Add(this.labelX);
            this.panelTitle.Location = new System.Drawing.Point(7, 88);
            this.panelTitle.Name = "panelTitle";
            this.panelTitle.Size = new System.Drawing.Size(330, 35);
            this.panelTitle.TabIndex = 13;
            // 
            // buttonDraw
            // 
            this.buttonDraw.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDraw.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(98)))), ((int)(((byte)(118)))), ((int)(((byte)(145)))));
            this.buttonDraw.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(188)))), ((int)(((byte)(215)))));
            this.buttonDraw.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(188)))), ((int)(((byte)(215)))));
            this.buttonDraw.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(148)))), ((int)(((byte)(175)))));
            this.buttonDraw.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonDraw.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.buttonDraw.Location = new System.Drawing.Point(11, 7);
            this.buttonDraw.Name = "buttonDraw";
            this.buttonDraw.Size = new System.Drawing.Size(95, 35);
            this.buttonDraw.TabIndex = 5;
            this.buttonDraw.Text = "Draw";
            this.buttonDraw.UseVisualStyleBackColor = false;
            this.buttonDraw.Click += new System.EventHandler(this.buttonDraw_Click);
            // 
            // PopupShape
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(344, 497);
            this.Controls.Add(this.panelPointLine);
            this.Controls.Add(this.panelTitle);
            this.Controls.Add(this.panelFaceType);
            this.Controls.Add(this.panelBotton);
            this.Name = "PopupShape";
            this.Padding = new System.Windows.Forms.Padding(4);
            this.Text = "형상 입력";
            this.panelBotton.ResumeLayout(false);
            this.panelFaceType.ResumeLayout(false);
            this.panelFaceType.PerformLayout();
            this.panelTitle.ResumeLayout(false);
            this.panelTitle.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelBotton;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Panel panelFaceType;
        private System.Windows.Forms.Panel panelPointLine;
        private System.Windows.Forms.ComboBox comboBoxFaceType;
        private System.Windows.Forms.Label labelX;
        private System.Windows.Forms.Label labelArc;
        private System.Windows.Forms.Label labelNo;
        private System.Windows.Forms.Label labelDirection;
        private System.Windows.Forms.Label labelY;
        private System.Windows.Forms.Label labelDelete;
        private System.Windows.Forms.Label labelAdd;
        private System.Windows.Forms.Panel panelTitle;
        private System.Windows.Forms.Label labelFaceType;
        private System.Windows.Forms.TextBox textBoxPartName;
        private System.Windows.Forms.Label labelPartName;
        private System.Windows.Forms.Button buttonDraw;
    }
}