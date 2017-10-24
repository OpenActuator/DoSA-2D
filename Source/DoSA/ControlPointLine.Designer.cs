namespace Shapes
{
    partial class ControlPointLine
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelNo = new System.Windows.Forms.Label();
            this.checkBoxKind = new System.Windows.Forms.CheckBox();
            this.checkBoxArcDirection = new System.Windows.Forms.CheckBox();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.textBoxX = new System.Windows.Forms.TextBox();
            this.textBoxY = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // labelNo
            // 
            this.labelNo.AutoSize = true;
            this.labelNo.Location = new System.Drawing.Point(15, 10);
            this.labelNo.Name = "labelNo";
            this.labelNo.Size = new System.Drawing.Size(11, 12);
            this.labelNo.TabIndex = 0;
            this.labelNo.Text = "1";
            // 
            // checkBoxKind
            // 
            this.checkBoxKind.AutoSize = true;
            this.checkBoxKind.Location = new System.Drawing.Point(201, 9);
            this.checkBoxKind.Name = "checkBoxKind";
            this.checkBoxKind.Size = new System.Drawing.Size(15, 14);
            this.checkBoxKind.TabIndex = 1;
            this.checkBoxKind.TabStop = false;
            this.checkBoxKind.UseVisualStyleBackColor = true;
            this.checkBoxKind.Click += new System.EventHandler(this.checkBoxKind_Click);
            // 
            // checkBoxArcDirection
            // 
            this.checkBoxArcDirection.AutoSize = true;
            this.checkBoxArcDirection.Location = new System.Drawing.Point(232, 9);
            this.checkBoxArcDirection.Name = "checkBoxArcDirection";
            this.checkBoxArcDirection.Size = new System.Drawing.Size(15, 14);
            this.checkBoxArcDirection.TabIndex = 3;
            this.checkBoxArcDirection.TabStop = false;
            this.checkBoxArcDirection.UseVisualStyleBackColor = true;
            this.checkBoxArcDirection.Click += new System.EventHandler(this.checkBoxArcDirection_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonAdd.Location = new System.Drawing.Point(299, 6);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(20, 20);
            this.buttonAdd.TabIndex = 5;
            this.buttonAdd.TabStop = false;
            this.buttonAdd.Text = "A";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonDelete
            // 
            this.buttonDelete.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonDelete.Location = new System.Drawing.Point(263, 6);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(20, 20);
            this.buttonDelete.TabIndex = 4;
            this.buttonDelete.TabStop = false;
            this.buttonDelete.Text = "D";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDel_Click);
            // 
            // textBoxX
            // 
            this.textBoxX.Location = new System.Drawing.Point(39, 6);
            this.textBoxX.Name = "textBoxX";
            this.textBoxX.Size = new System.Drawing.Size(70, 21);
            this.textBoxX.TabIndex = 0;
            this.textBoxX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxX.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxX_KeyPress);
            this.textBoxX.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxX_KeyUp);
            // 
            // textBoxY
            // 
            this.textBoxY.Location = new System.Drawing.Point(115, 6);
            this.textBoxY.Name = "textBoxY";
            this.textBoxY.Size = new System.Drawing.Size(70, 21);
            this.textBoxY.TabIndex = 1;
            this.textBoxY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxY.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxY_KeyPress);
            this.textBoxY.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxY_KeyUp);
            // 
            // ControlPointLine
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.textBoxY);
            this.Controls.Add(this.textBoxX);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.checkBoxArcDirection);
            this.Controls.Add(this.checkBoxKind);
            this.Controls.Add(this.labelNo);
            this.Name = "ControlPointLine";
            this.Size = new System.Drawing.Size(336, 28);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelNo;
        private System.Windows.Forms.CheckBox checkBoxKind;
        private System.Windows.Forms.CheckBox checkBoxArcDirection;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.TextBox textBoxX;
        private System.Windows.Forms.TextBox textBoxY;
    }
}
