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
            this.buttonReserved = new System.Windows.Forms.Button();
            this.buttonFitAll = new System.Windows.Forms.Button();
            this.buttonDraw = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.panelFaceType = new System.Windows.Forms.Panel();
            this.comboBoxNodeType = new System.Windows.Forms.ComboBox();
            this.labelKind = new System.Windows.Forms.Label();
            this.textBoxPartName = new System.Windows.Forms.TextBox();
            this.labelPartName = new System.Windows.Forms.Label();
            this.comboBoxFaceType = new System.Windows.Forms.ComboBox();
            this.labelFaceType = new System.Windows.Forms.Label();
            this.panelPointControl = new System.Windows.Forms.Panel();
            this.labelX = new System.Windows.Forms.Label();
            this.labelArc = new System.Windows.Forms.Label();
            this.labelNo = new System.Windows.Forms.Label();
            this.labelDirection = new System.Windows.Forms.Label();
            this.labelY = new System.Windows.Forms.Label();
            this.labelDelete = new System.Windows.Forms.Label();
            this.labelAdd = new System.Windows.Forms.Label();
            this.panelTitle = new System.Windows.Forms.Panel();
            this.panelBasePoint = new System.Windows.Forms.Panel();
            this.textBoxBaseX = new System.Windows.Forms.TextBox();
            this.textBoxBaseY = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panelBotton.SuspendLayout();
            this.panelFaceType.SuspendLayout();
            this.panelTitle.SuspendLayout();
            this.panelBasePoint.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBotton
            // 
            this.panelBotton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBotton.BackColor = System.Drawing.SystemColors.Control;
            this.panelBotton.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBotton.Controls.Add(this.buttonReserved);
            this.panelBotton.Controls.Add(this.buttonFitAll);
            this.panelBotton.Controls.Add(this.buttonDraw);
            this.panelBotton.Location = new System.Drawing.Point(7, 467);
            this.panelBotton.Name = "panelBotton";
            this.panelBotton.Size = new System.Drawing.Size(350, 50);
            this.panelBotton.TabIndex = 3;
            // 
            // buttonReserved
            // 
            this.buttonReserved.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReserved.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(98)))), ((int)(((byte)(118)))), ((int)(((byte)(145)))));
            this.buttonReserved.Enabled = false;
            this.buttonReserved.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(188)))), ((int)(((byte)(215)))));
            this.buttonReserved.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(188)))), ((int)(((byte)(215)))));
            this.buttonReserved.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(148)))), ((int)(((byte)(175)))));
            this.buttonReserved.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonReserved.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.buttonReserved.Location = new System.Drawing.Point(245, 7);
            this.buttonReserved.Name = "buttonReserved";
            this.buttonReserved.Size = new System.Drawing.Size(95, 35);
            this.buttonReserved.TabIndex = 2;
            this.buttonReserved.Text = "Reserved";
            this.buttonReserved.UseVisualStyleBackColor = false;
            // 
            // buttonFitAll
            // 
            this.buttonFitAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFitAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(98)))), ((int)(((byte)(118)))), ((int)(((byte)(145)))));
            this.buttonFitAll.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(188)))), ((int)(((byte)(215)))));
            this.buttonFitAll.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(188)))), ((int)(((byte)(215)))));
            this.buttonFitAll.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(148)))), ((int)(((byte)(175)))));
            this.buttonFitAll.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonFitAll.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.buttonFitAll.Location = new System.Drawing.Point(128, 7);
            this.buttonFitAll.Name = "buttonFitAll";
            this.buttonFitAll.Size = new System.Drawing.Size(95, 35);
            this.buttonFitAll.TabIndex = 1;
            this.buttonFitAll.Text = "Fit All";
            this.buttonFitAll.UseVisualStyleBackColor = false;
            this.buttonFitAll.Click += new System.EventHandler(this.buttonFitAll_Click);
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
            this.buttonDraw.TabIndex = 0;
            this.buttonDraw.Text = "Redraw";
            this.buttonDraw.UseVisualStyleBackColor = false;
            this.buttonDraw.Click += new System.EventHandler(this.buttonDraw_Click);
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
            this.buttonCancel.Location = new System.Drawing.Point(262, 531);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(95, 35);
            this.buttonCancel.TabIndex = 5;
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
            this.buttonOK.Location = new System.Drawing.Point(161, 531);
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
            this.panelFaceType.Controls.Add(this.comboBoxNodeType);
            this.panelFaceType.Controls.Add(this.labelKind);
            this.panelFaceType.Controls.Add(this.textBoxPartName);
            this.panelFaceType.Controls.Add(this.labelPartName);
            this.panelFaceType.Controls.Add(this.comboBoxFaceType);
            this.panelFaceType.Controls.Add(this.labelFaceType);
            this.panelFaceType.Location = new System.Drawing.Point(7, 7);
            this.panelFaceType.Name = "panelFaceType";
            this.panelFaceType.Size = new System.Drawing.Size(350, 105);
            this.panelFaceType.TabIndex = 0;
            // 
            // comboBoxNodeType
            // 
            this.comboBoxNodeType.FormattingEnabled = true;
            this.comboBoxNodeType.Items.AddRange(new object[] {
            "COIL",
            "MAGNET",
            "STEEL"});
            this.comboBoxNodeType.Location = new System.Drawing.Point(118, 42);
            this.comboBoxNodeType.Name = "comboBoxNodeType";
            this.comboBoxNodeType.Size = new System.Drawing.Size(167, 20);
            this.comboBoxNodeType.TabIndex = 1;
            this.comboBoxNodeType.SelectedIndexChanged += new System.EventHandler(this.comboBoxPartType_SelectedIndexChanged);
            // 
            // labelKind
            // 
            this.labelKind.AutoSize = true;
            this.labelKind.Location = new System.Drawing.Point(25, 45);
            this.labelKind.Name = "labelKind";
            this.labelKind.Size = new System.Drawing.Size(68, 12);
            this.labelKind.TabIndex = 17;
            this.labelKind.Text = "Part Type :";
            // 
            // textBoxPartName
            // 
            this.textBoxPartName.Location = new System.Drawing.Point(118, 10);
            this.textBoxPartName.Name = "textBoxPartName";
            this.textBoxPartName.Size = new System.Drawing.Size(167, 21);
            this.textBoxPartName.TabIndex = 0;
            // 
            // labelPartName
            // 
            this.labelPartName.AutoSize = true;
            this.labelPartName.Location = new System.Drawing.Point(25, 14);
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
            this.comboBoxFaceType.Location = new System.Drawing.Point(118, 73);
            this.comboBoxFaceType.Name = "comboBoxFaceType";
            this.comboBoxFaceType.Size = new System.Drawing.Size(167, 20);
            this.comboBoxFaceType.TabIndex = 2;
            this.comboBoxFaceType.SelectedIndexChanged += new System.EventHandler(this.comboBoxFaceType_SelectedIndexChanged);
            // 
            // labelFaceType
            // 
            this.labelFaceType.AutoSize = true;
            this.labelFaceType.Location = new System.Drawing.Point(25, 76);
            this.labelFaceType.Name = "labelFaceType";
            this.labelFaceType.Size = new System.Drawing.Size(74, 12);
            this.labelFaceType.TabIndex = 12;
            this.labelFaceType.Text = "Face Type :";
            // 
            // panelPointControl
            // 
            this.panelPointControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPointControl.AutoScroll = true;
            this.panelPointControl.BackColor = System.Drawing.SystemColors.Control;
            this.panelPointControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPointControl.Location = new System.Drawing.Point(7, 207);
            this.panelPointControl.Name = "panelPointControl";
            this.panelPointControl.Size = new System.Drawing.Size(350, 254);
            this.panelPointControl.TabIndex = 2;
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
            this.panelTitle.Location = new System.Drawing.Point(7, 166);
            this.panelTitle.Name = "panelTitle";
            this.panelTitle.Size = new System.Drawing.Size(350, 35);
            this.panelTitle.TabIndex = 13;
            // 
            // panelBasePoint
            // 
            this.panelBasePoint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBasePoint.BackColor = System.Drawing.SystemColors.Control;
            this.panelBasePoint.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBasePoint.Controls.Add(this.textBoxBaseX);
            this.panelBasePoint.Controls.Add(this.textBoxBaseY);
            this.panelBasePoint.Controls.Add(this.label1);
            this.panelBasePoint.Controls.Add(this.label2);
            this.panelBasePoint.Location = new System.Drawing.Point(7, 119);
            this.panelBasePoint.Name = "panelBasePoint";
            this.panelBasePoint.Size = new System.Drawing.Size(350, 41);
            this.panelBasePoint.TabIndex = 1;
            // 
            // textBoxBaseX
            // 
            this.textBoxBaseX.Location = new System.Drawing.Point(82, 11);
            this.textBoxBaseX.Name = "textBoxBaseX";
            this.textBoxBaseX.Size = new System.Drawing.Size(80, 21);
            this.textBoxBaseX.TabIndex = 0;
            this.textBoxBaseX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxBaseX.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxBaseX_KeyPress);
            this.textBoxBaseX.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxBaseX_KeyUp);
            this.textBoxBaseX.Leave += new System.EventHandler(this.textBoxBaseX_Leave);
            // 
            // textBoxBaseY
            // 
            this.textBoxBaseY.Location = new System.Drawing.Point(243, 11);
            this.textBoxBaseY.Name = "textBoxBaseY";
            this.textBoxBaseY.Size = new System.Drawing.Size(80, 21);
            this.textBoxBaseY.TabIndex = 1;
            this.textBoxBaseY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxBaseY.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxBaseY_KeyPress);
            this.textBoxBaseY.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxBaseY_KeyUp);
            this.textBoxBaseY.Leave += new System.EventHandler(this.textBoxBaseY_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(179, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 12);
            this.label1.TabIndex = 13;
            this.label1.Text = "Base_Y :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 12);
            this.label2.TabIndex = 14;
            this.label2.Text = "Base_X :";
            // 
            // PopupShape
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(364, 572);
            this.Controls.Add(this.panelBasePoint);
            this.Controls.Add(this.panelPointControl);
            this.Controls.Add(this.panelTitle);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.panelFaceType);
            this.Controls.Add(this.panelBotton);
            this.Name = "PopupShape";
            this.Padding = new System.Windows.Forms.Padding(4);
            this.ShowIcon = false;
            this.Text = "형상 입력";
            this.panelBotton.ResumeLayout(false);
            this.panelFaceType.ResumeLayout(false);
            this.panelFaceType.PerformLayout();
            this.panelTitle.ResumeLayout(false);
            this.panelTitle.PerformLayout();
            this.panelBasePoint.ResumeLayout(false);
            this.panelBasePoint.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelBotton;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Panel panelFaceType;
        private System.Windows.Forms.Panel panelPointControl;
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
        private System.Windows.Forms.Button buttonReserved;
        private System.Windows.Forms.Button buttonFitAll;
        private System.Windows.Forms.Panel panelBasePoint;
        private System.Windows.Forms.TextBox textBoxBaseX;
        private System.Windows.Forms.TextBox textBoxBaseY;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxNodeType;
        private System.Windows.Forms.Label labelKind;
    }
}