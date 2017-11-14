using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Shapes
{
    public partial class CPointControl : UserControl
    {
        /// <summary>
        /// 좌표 제거 이벤트 핸들러
        /// </summary>
        public delegate void CoordinatesRemoveEventHandler(object sender, PointControlRemoveEventArgs e);

        /// <summary>
        /// 좌표 제거 이벤트
        /// </summary>
        public event CoordinatesRemoveEventHandler RemoveCoordinates;

        /// <summary>
        /// 좌표 제거 완료 이벤트
        /// </summary>
        public event EventHandler RemoveComplete;
      
        /// <summary>
        /// 좌표 추가 이벤트
        /// </summary>
        public event EventHandler AddCoordinates;

        /// <summary>
        /// 좌표 값
        /// </summary>
        public string StrCoordX
        {
            get{ return textBoxX.Text; }
            set { this.textBoxX.Text = value.ToString(); }
        }

        /// <summary>
        /// 좌표 값
        /// </summary>
        public string StrCoordY
        {
            get { return textBoxY.Text; }
            set { this.textBoxY.Text = value.ToString(); }
        }

        /// <summary>
        /// 순번
        /// </summary>
        public int Sequence
        {
            get { return int.Parse(this.labelNo.Text); }
            set { this.labelNo.Text = value.ToString(); }
        }

        /// <summary>
        /// 곡선 여부
        /// </summary>
        public bool IsArc 
        { 
            get { return this.checkBoxKind.Checked; }
            set { this.checkBoxKind.Checked = value; }
        }

        /// <summary>
        /// 방향 여부
        /// </summary>
        public bool IsArcDirection 
        { 
            get { return this.checkBoxArcDirection.Checked; }
            set { this.checkBoxArcDirection.Checked = value; }        
        }

        /// <summary>
        /// 설정과 동시에 보이기 유무 처리됨
        /// </summary>
        public bool IsRectangle
        {
            get { return this.buttonDelete.Enabled; }
            set { 
                this.buttonDelete.Enabled = value;
                this.buttonAdd.Enabled = value;
                this.checkBoxArcDirection.Enabled = value;
                this.checkBoxKind.Enabled = value;
            }
        }

        #region--------------------------- 생성자 ---------------------------

        public CPointControl()
        {
            InitializeComponent();
        }             

        public CPointControl(CPoint point)
        {
            InitializeComponent();

            StrCoordX = point.m_dX.ToString();
            StrCoordY = point.m_dY.ToString();

            IsArc = (point.m_emLineKind == EMLineKind.ARC) ? true : false;
            IsArcDirection = (point.m_emDirectionArc == EMDirectionArc.BACKWARD) ? true : false;            
        }

        #endregion

        #region--------------------------- Button Event -------------------------

        private void buttonDel_Click(object sender, EventArgs e)
        {
            // 버튼이 사라지면서 Parent 를 잊어버리기 때문에 백업 후에 사용한다.
            PopupShape backupPopupShape = (PopupShape)this.Parent.Parent;

            if (RemoveCoordinates != null)
            {
                PointControlRemoveEventArgs args = new PointControlRemoveEventArgs();
                RemoveCoordinates(this, args);

                if (args.Cancel) return;
            }

            this.Parent.Controls.Remove(this);

            if (RemoveComplete != null)
            {
                RemoveComplete(this, new EventArgs());
            }

            // 백업한 PopupShape 를 사용한다.
            backupPopupShape.drawTemporaryFace();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (AddCoordinates != null)
            {
                AddCoordinates(this, new EventArgs());
            }
        }

        #endregion

        #region---------------------- 형상정보 입력과 동시에 형상 그리기 ------------------

        // 형상 그리기 조건
        //
        // 1. Y 값 입력후 엔터키
        // 2. Y 값 입력후 칸을 벗어날 때
        // 3. Line Type 변경
        // 4. Arc 방향 변경
        //
        private void textBoxY_KeyUp(object sender, KeyEventArgs e)
        {
            /// Enter 에서만 동작한다.
            if (e.KeyCode == Keys.Enter)
            {
                ((PopupShape)this.Parent.Parent).drawTemporaryFace();
            }
        }

        private void textBoxY_Leave(object sender, EventArgs e)
        {
            ((PopupShape)this.Parent.Parent).drawTemporaryFace();
        }

        private void textBoxX_KeyUp(object sender, KeyEventArgs e)
        {
            /// Enter 에서만 동작한다.
            if (e.KeyCode == Keys.Enter)
            {
                ((PopupShape)this.Parent.Parent).drawTemporaryFace();
            }
        }

        private void textBoxX_Leave(object sender, EventArgs e)
        {
            ((PopupShape)this.Parent.Parent).drawTemporaryFace();
        }

        private void checkBoxKind_Click(object sender, EventArgs e)
        {
            ((PopupShape)this.Parent.Parent).drawTemporaryFace();
        }

        private void checkBoxArcDirection_Click(object sender, EventArgs e)
        {
            ((PopupShape)this.Parent.Parent).drawTemporaryFace();
        }

        #endregion

        #region-------------------------- 동작 제한 기능 -----------------------------

        private void textBoxX_KeyPress(object sender, KeyPressEventArgs e)
        {
            /// 숫자, 소수점, Back, 엔터만 입력 가능하도록함
            if (    !(Char.IsDigit(e.KeyChar)) && e.KeyChar != '.' && e.KeyChar != '-' && 
                    e.KeyChar != Convert.ToChar(Keys.Back) && e.KeyChar != Convert.ToChar(Keys.Enter))
            {
                e.Handled = true;
            }
        }

        private void textBoxY_KeyPress(object sender, KeyPressEventArgs e)
        {
            /// 숫자, 소수점, Back, 엔터만 입력 가능하도록함
            if (    !(Char.IsDigit(e.KeyChar)) && e.KeyChar != '.' && e.KeyChar != '-' && 
                    e.KeyChar != Convert.ToChar(Keys.Back) && e.KeyChar != Convert.ToChar(Keys.Enter))
            {
                e.Handled = true;
            }
        }

        internal void hideAddButton()
        {
            buttonAdd.Visible = false;
        }

        internal void showAddButton()
        {
            buttonAdd.Visible = true;
        }

        #endregion
        
    }

    /// <summary>
    /// 좌표 제거 이벤트 인자
    /// </summary>
    public class PointControlRemoveEventArgs
    {
        /// <summary>
        /// 삭제 취소 여부
        /// </summary>
        public bool Cancel { get; set; }
    }
}