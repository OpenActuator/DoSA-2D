using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Scripts;
using gtLibrary;
using Nodes;
using DoSA;
using Parts;

namespace Shapes
{
    public partial class PopupShape : Form
    {
        const int POPUP_SIZE_X = 380;
        const int POPUP_SIZE_Y = 620;

        const int MIN_POLYGON_CONTROL_COUNT = 4;
        const int RECTANGLE_CONTROL_COUNT = 2;

        private EMFaceType m_faceType = EMFaceType.RECTANGLE;
        private EMKind m_partType = EMKind.NON_KIND;

        private List<CPointUI> m_listPointUI = null;

        private string m_strPartName;

        private bool m_bCreatePopupWindow;
        public EMKind PartType
        {
            get { return m_partType; }
            set { m_partType = value; }
        }
        public string PartName
        {
            get { return m_strPartName; }
            set { m_strPartName = value; }
        }

        /// <summary>
        /// PopupShape 의 FaceType 변경만으로 Popup 창의 형태와 초기화가 이루어진다.
        /// </summary>
        public EMFaceType FaceType
        {
            get { return m_faceType; }
            set
            {
                this.m_faceType = value;

                switch (this.m_faceType)
                {
                    case EMFaceType.RECTANGLE:
                        this.panelPointControl.Controls.Clear();
                        this.m_faceType = EMFaceType.RECTANGLE;

                        this.addRectanglePointControl(this.panelPointControl);
                        this.Size = new Size(POPUP_SIZE_X, POPUP_SIZE_Y);

                        break;

                    case EMFaceType.POLYGON:
                        this.panelPointControl.Controls.Clear();
                        this.m_faceType = EMFaceType.POLYGON;

                        this.addPolylinePointControl(this.panelPointControl);
                        this.Size = new Size(POPUP_SIZE_X, POPUP_SIZE_Y);
                        break;

                    default:
                        CNotice.printLogID("AFTE");

                        // 해당사항이 없는 항목이 넘어 왔기 때문에 바로 retrun 해서 아래의 동작을 하지 않는다.
                        return;
                }
            }
        }

        /// <summary>
        /// 좌표정보 리스트
        /// </summary>
        public List<CPointUI> ListPointUI
        {
            get
            {
                m_listPointUI = new List<CPointUI>();

                try
                { 

                    for (int i = this.panelPointControl.Controls.Count - 1; i >= 0; i--)
                    {
                        m_listPointUI.Add((CPointUI)this.panelPointControl.Controls[i]);
                    }

                    return m_listPointUI;
                }
                catch (Exception ex)
                {
                    CNotice.printLog(ex.Message);

                    // null 를 리턴하지 않고 항목이 들어있지 않는 List 를 송부한다.
                    return m_listPointUI;
                }  
            }
        }

        /// <summary>
        /// Face 의 형상 정보가 없는 경우 호출되는 생성자로
        /// 툴바의 파트 생성 버튼을 사용할 때 호출되는 생성자이다.
        /// </summary>
        public PopupShape(EMFaceType drawType, EMKind emKind)
        {
            InitializeComponent();

            m_strPartName = string.Empty;
            m_partType = emKind;

            m_bCreatePopupWindow = true;
            
            textBoxBaseX.Text = "0.0";
            textBoxBaseY.Text = "0.0";

            switch (m_partType)
            {
                case EMKind.COIL:
                    labelPartName.Text = "Coil Name :";
                    this.Text = "Add Coil";
                    break;
                
                case EMKind.MAGNET:
                    labelPartName.Text = "Magnet Name :";
                    this.Text = "Add Magnet";
                    break;
             
                case EMKind.STEEL:
                    labelPartName.Text = "Steel Name :";
                    this.Text = "Add Steel";
                    break;

                default:
                    CNotice.printLogID("TPTI");

                    // 해당사항이 없는 항목이 넘어 왔기 때문에 바로 retrun 해서 아래의 동작을 하지 않는다.
                    return;
            }

            // 콤보박스 데이터는 파라메터로 넘어오는 대로 강제로 지정한다.
            comboBoxFaceType.SelectedItem = drawType.ToString();
            comboBoxNodeType.SelectedItem = m_partType.ToString();

            /// 코일의 경우는 코일계산 때문에 Rectangle 로 고정을 해야 한다.
            if (emKind == EMKind.COIL)
                comboBoxFaceType.Enabled = false;

            comboBoxNodeType.Enabled = false;
        }

        /// <summary>
        /// Face 의 형상 정보가 있는 경우 호출되는 생성자로
        /// TreeView 에서 파트를 더블클릭할때 호출되는 생성자이다.
        /// </summary>
        public PopupShape(string partName, CFace face, EMKind emKind)
        {
            InitializeComponent();

            m_strPartName = partName;
            m_partType = emKind;

            m_bCreatePopupWindow = false;

            try
            {
                switch (m_partType)
                {
                    case EMKind.COIL:
                        labelPartName.Text = "Coil Name :";
                        this.Text = "Change Coil";
                        break;

                    case EMKind.MAGNET:
                        labelPartName.Text = "Magnet Name :";
                        this.Text = "Change Magnet";
                        break;

                    case EMKind.STEEL:
                        labelPartName.Text = "Steel Name :";
                        this.Text = "Change Steel";
                        break;

                    case EMKind.NON_KIND:
                        labelPartName.Text = "Part Name";
                        this.Text = "Change Part";
                        break;

                    default:
                        CNotice.printLogID("TPTI");

                        // 해당사항이 없는 항목이 넘어 왔기 때문에 바로 retrun 해서 아래의 동작을 하지 않는다.
                        return;
                }
            
                if (face == null)
                {
                    CNotice.printLogID("CTPS1");
                    return;
                }

                textBoxBaseX.Text = face.BasePoint.X.ToString();
                textBoxBaseY.Text = face.BasePoint.Y.ToString();

                if (face.getPointCount() < MIN_POLYGON_CONTROL_COUNT)
                {
                    CNotice.printLogID("CTPS");
                    return;
                }

                /// 파트이름을 표시만하고 수정을 하지 못하게 한다.
                textBoxPartName.Text = partName;

                if(face.FaceType == EMFaceType.RECTANGLE)
                {
                    // 설정과 동시에 이벤트가 호출되고 
                    // 이벤트함수에서 FaceType 가 지정되면서 Popup 창의 형태와 UserControl 이 초기화 된다.
                    comboBoxFaceType.SelectedItem = EMFaceType.RECTANGLE.ToString();

                    this.ListPointUI[0].StrCoordX = face.RelativePointList[0].X.ToString();
                    this.ListPointUI[0].StrCoordZ = face.RelativePointList[0].Y.ToString();

                    this.ListPointUI[1].StrCoordX = face.RelativePointList[2].X.ToString();
                    this.ListPointUI[1].StrCoordZ = face.RelativePointList[2].Y.ToString();            
                }
                else if(face.FaceType == EMFaceType.POLYGON)
                {
                    // 설정과 동시에 이벤트가 호출되고 
                    // 이벤트함수에서 FaceType 가 지정되면서 Popup 창의 형태와 UserControl 이 초기화 된다.
                    comboBoxFaceType.SelectedItem = EMFaceType.POLYGON.ToString();

                    for(int i = 0; i < face.getPointCount() ; i++)
                    {
                        // 기본 생성 Control 수 보다 작을 때는 있는 Control 에 데이터를 담고
                        // 클 때는 Control 를 생성하면서 데이터를 담은다.
                        if(i >= MIN_POLYGON_CONTROL_COUNT)
                            this.addPointControl(new CPointUI(), true, this.panelPointControl);

                        this.ListPointUI[i].StrCoordX = face.RelativePointList[i].X.ToString();
                        this.ListPointUI[i].StrCoordZ = face.RelativePointList[i].Y.ToString();

                        if (face.RelativePointList[i].LineKind == EMLineKind.ARC)
                            this.ListPointUI[i].IsArc = true;
                        else
                            this.ListPointUI[i].IsArc = false;

                        if (face.RelativePointList[i].DirectionArc == EMDirectionArc.BACKWARD)
                            this.ListPointUI[i].IsArcDirection = true;
                        else
                            this.ListPointUI[i].IsArcDirection = false;
                    }
                }
                else
                {
                    CNotice.printLogID("UAWF");
                    return;
                }

                /// 수정을 할때는 형상 과 Node Type 변경을 못하도록 한다.
                comboBoxFaceType.Enabled = false;

                comboBoxNodeType.SelectedItem = m_partType.ToString();

                if(m_partType != EMKind.NON_KIND)
                    comboBoxNodeType.Enabled = false;
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);
            }  
        }

        /// <summary>
        /// 사각형 형상입력 창이 뜰때 2개의 Point 컨트롤을 기본으로 한다.
        /// </summary>
        private void addRectanglePointControl(Panel panel)
        {
            for(int i=0 ; i< RECTANGLE_CONTROL_COUNT; i++)
                this.addPointControl(new CPointUI(), false, panel);

            this.resetSequence();
        }

        /// <summary>
        /// 다각형 형상입력 창이 뜰때 4개의 Point 컨트롤을 기본으로 한다.
        /// </summary>
        private void addPolylinePointControl(Panel panel)
        {
            for(int i=0 ; i< MIN_POLYGON_CONTROL_COUNT; i++)
                this.addPointControl(new CPointUI(), true, panel);
       
            this.resetSequence();
        }

        /// <summary>
        /// 좌표입력 컨트롤을 하나 추가한다
        /// </summary>
        /// <param name="pointControl">좌표 객체</param>
        /// <param name="panel">좌표를 추가할 Panel</param>
        private CPointUI addPointControl(CPointUI pointControl, bool showButton, Panel panel)
        {
            pointControl.Dock = DockStyle.Top;
            pointControl.IsRectangle = showButton;

            try
            {
                #region------------------------- 이벤트 호출 영역 ------------------------------
                /// 이벤트 호출 영역은 addControlPoint() 안에 있으나 호출 때 동작하는 영역이 아니다.
                /// 단, 함수내에 있는 이유는 addControlPoint() 안에 있어서
                /// 파라메터로 넘어오는 변수를 직접 접근할 수 있어, 이벤트 함수의 변수 접근 문제를 해결했다.
                /// 
                pointControl.AddCoordinates += (s, e) =>
                {
                    /// 추가 버튼 클릭했을때 현재 컨트롤 아래 추가
                    /// - 컨트롤 추가 
                    ///  * addPointControl() 의 호출 영역을 호출 한다.
                    CPointUI current = this.addPointControl(new CPointUI(), showButton, panel);

                    /// - 컨트롤 정렬
                    List<CPointUI> sortList = new List<CPointUI>();
                    for (int i = 0; i < panel.Controls.Count; i++)
                    {
                        CPointUI c = panel.Controls[i] as CPointUI;
                        if (c.Equals(current)) continue; // 현재 추가된 항목은 통과
                        if (c.Equals(pointControl)) break; // 버튼 클릭한 항목을때 그 위쪽은 탐색할 필요 없으므로 루프 종료

                        sortList.Add(c);
                    }
                
                    for (int i = sortList.Count - 1; i >= 0; i--)
                    {
                        sortList[i].BringToFront();
                    }

                    this.resetSequence();

                };

                /// addPointControl() 이 호출될 때 실행되지 않고 이벤트가 발생할 때 동작한다.
                pointControl.RemoveCoordinates += (s, e) =>
                {
                    if (this.m_faceType == EMFaceType.RECTANGLE)
                    {
                        if (this.panelPointControl.Controls.Count == RECTANGLE_CONTROL_COUNT)
                        {
                            e.Cancel = true;
                            CNotice.noticeWarningID("RRAL");
                            return;
                        }
                    }
                    else
                    {
                        if (this.panelPointControl.Controls.Count <= MIN_POLYGON_CONTROL_COUNT)
                        {
                            e.Cancel = true;
                            CNotice.noticeWarningID("PRAL");
                            return;
                        }
                    }
                };

                /// addControlPoint() 이 호출될 때 실행되지 않고 이벤트가 발생할 때 동작한다.
                pointControl.RemoveComplete += (s, e) =>
                {
                    this.resetSequence();

                    // 생성창에서만 Add 버튼을 하나로 표시한다
                    if(m_bCreatePopupWindow == true)
                    {
                        /// PointControl 삭제때 Add 버튼 하나만 표시하기
                        /// 
                        /// 정렬이 끝난 후에 PointControl Panel 의 가장 아래 PointControl ADD 버튼만 보이게 하여
                        /// PointControl 의 삽입이 항상 아래에서만 이루어지도록 해서 Tab 의 문제를 해결 했다.
                        for (int i = 0; i < panelPointControl.Controls.Count; i++)
                        {
                            if (i == 0)
                                ((CPointUI)panelPointControl.Controls[i]).showAddButton();
                            else
                                ((CPointUI)panelPointControl.Controls[i]).hideAddButton();
                        }
                    }
                };
                #endregion

                #region------------------------ 함수 호출 영역 -----------------------------

                panel.Controls.Add(pointControl);
                pointControl.BringToFront();

                this.resetSequence();

                // 생성창에서만 Add 버튼을 하나로 표시한다
                if (m_bCreatePopupWindow == true)
                {
                    /// 2. PointControl 추가때 Add 버튼 하나만 표시하기
                    /// 
                    /// 정렬이 끝난 후에 PointControl Panel 의 가장 아래 PointControl ADD 버튼만 보이게 하여
                    /// PointControl 의 삽입이 항상 아래에서만 이루어지도록 해서 Tab 의 문제를 해결 했다.
                    for (int i = 0; i < panelPointControl.Controls.Count; i++)
                    {
                        if (i == 0)
                            ((CPointUI)panelPointControl.Controls[i]).showAddButton();
                        else
                            ((CPointUI)panelPointControl.Controls[i]).hideAddButton();
                    }
                }

                return pointControl;

                #endregion
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                // null 를 리턴하지 않고 추가되지 않은 이전의 pointControl 를 리턴한다. 
                return pointControl;
            }  
        }
           
        
        /// <summary>
        /// Point Control 은 추가나 삭제가 가능하다.
        /// 따라서 각 Point Control 의 순번을 List 안에서의 번호를 일치시킨다.
        /// 
        /// 추가나 삭제때 꼭 호출이 되어야 한다.
        /// (단, 추가는 마지막 Point Control 에서만 이루어지도록 해서 순서 변경이 없도록 하였다)
        /// </summary>
        private void resetSequence()
        {
            try
            {
                for (int i = 0; i < this.panelPointControl.Controls.Count; i++)
                {
                    ((CPointUI)this.panelPointControl.Controls[i]).Sequence = this.panelPointControl.Controls.Count - i;
                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }  
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {

            if (this.m_faceType != EMFaceType.POLYGON)
            {
                CNotice.printLogID("TBOA");
                return;
            }

            this.addPointControl(new CPointUI(), true, this.panelPointControl);
        }

        private void comboBoxFaceType_SelectedIndexChanged(object sender, EventArgs e)
        {
            FaceType = (EMFaceType)comboBoxFaceType.SelectedIndex;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private bool isInputDataOK()
        {
            /// 1. 입력값 확인
            /// 
            if (textBoxPartName.Text.Length == 0)
            {
                CNotice.noticeWarningID("PEAP");
                return false;
            }

            for (int i = 0; i < ListPointUI.Count; i++)
            {               
                if (ListPointUI[i].StrCoordX.Trim().Length == 0)
                {
                    CNotice.noticeWarningID("PETC");
                    return false;
                }

                if (ListPointUI[i].StrCoordZ.Trim().Length == 0)
                {
                    CNotice.noticeWarningID("PETC");
                    return false;
                }
            }

            string strX1, strY1, strX2, strY2;

            /// 동일한 좌표값이 점이 중복으로 있는 경우
            for (int i = 0; i < ListPointUI.Count - 1; i++)
            {
                for (int j = i + 1; j < ListPointUI.Count; j++)
                {
                    strX1 = ListPointUI[i].StrCoordX.Trim();
                    strY1 = ListPointUI[i].StrCoordZ.Trim();
                    strX2 = ListPointUI[j].StrCoordX.Trim();
                    strY2 = ListPointUI[j].StrCoordZ.Trim();

                    if(strX1 == strX2 && strY1 == strY2)
                    {
                        CNotice.noticeWarningID("TATP");
                        return false;
                    }
                }
            }

            /// 파트 초기 생성때는 m_strPartName = string.Empty 로 PopupShape 객체를 생성하고,
            /// 파트 수정 때는 m_strPartName 에 이름을 넣어서  PopupShape 객체를 생성하기 때문에 파트의 수정인지를 m_strPartName 로 파악한다.
            if (m_strPartName.Length == 0)
            {
                /// [문제]
                ///  - Form 에서는 Parent를 사용할 수 없어 Owner 속성을 사용하지만
                ///    종종 Owner 가 null 로 넘어오는 문제가 발생한다.
                /// [해결]
                ///  - PopupShape 창을 생성하기 전에 Owner 속성을 FormMain 으로 초기화 해 두어야
                ///    확실하게 FormMain 을 얻을 수 있다.
                FormMain formMain = ((FormMain)this.Owner);

                if (formMain == null)
                {
                    CNotice.printLogID("CNGM");
                    return false;
                }

                if (true == formMain.m_design.isExistNode(textBoxPartName.Text))
                {
                    CNotice.noticeWarningID("TPAE");
                    return false;
                }
            }
            
            return true;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            /// 완벽한 입력인 상태에서만 Draw 및 저장이 가능한다.
            if ( isInputDataOK() == false)
            {
                DialogResult = DialogResult.Cancel;
                return;
            }
            
            if (PartType == EMKind.COIL && FaceType == EMFaceType.POLYGON)
            {
                if(false == isRectangleShapeInPopup())
                {
                    if (CSettingData.m_emLanguage == EMLanguage.Korean)
                        CNotice.noticeWarning("Coil 의 형상이 직사각형이 아닙니다.\nCoil 지정을 취소합니다.");
                    else
                        CNotice.noticeWarning("The shape of the Coil is not rectangular.\n.Cancels the Coil assignment.");

                    DialogResult = DialogResult.Cancel;
                    return;
                }
            }

            // 확인을 위해 임시 생성한다.
            CFace faceTemp = makeFaceInPopup();

            if (faceTemp == null)
            {
                CNotice.noticeWarningID("TWAP1");
                DialogResult = DialogResult.Cancel;
                return;
            }

            if (false == faceTemp.isShapeOK())
            {
                CNotice.printLogID("TWAP3");
                DialogResult = DialogResult.Cancel;
                return;
            }
            
            FormMain formMain = ((FormMain)this.Owner);

            if (formMain != null)
            {
                // 혹시 FEMM 의 화면이 닫힌 경우 FEMM 의 화면을 복원합니다.
                formMain.reopenFEMM();
            }

            m_strPartName = textBoxPartName.Text;

            // 문제가 없으면 정상 종료를 리턴한다.
            DialogResult = DialogResult.OK;
        }

        // Base Point 와 상대좌표로 좌표값이 저장되는 Face 가 생성된다.
        public CFace makeFaceInPopup()
        {
            try
            {
                CFace face = new CFace();

                face.BasePoint.X = Double.Parse(textBoxBaseX.Text);
                face.BasePoint.Y = Double.Parse(textBoxBaseY.Text);

                if (FaceType == EMFaceType.RECTANGLE)
                {
                    if (ListPointUI.Count != 2)
                    {
                        CNotice.printLogID("TATP1");

                        // null 을 리턴하고 호출하는 측에서 꼭 null 검사를 한다.
                        return null;
                    }

                    double x1, y1, x2, y2;

                    x1 = Double.Parse(ListPointUI[0].StrCoordX);
                    y1 = Double.Parse(ListPointUI[0].StrCoordZ);
                    x2 = Double.Parse(ListPointUI[1].StrCoordX);
                    y2 = Double.Parse(ListPointUI[1].StrCoordZ);

                    face.setRectanglePoints(x1, y1, x2, y2);
                }
                else
                {
                    if (ListPointUI.Count < 4)
                    {
                        CNotice.printLogID("TANM");

                        // null 을 리턴하고 호출하는 측에서 꼭 null 검사를 한다.
                        return null;
                    }

                    // PartType 가 코일이고 Polygon 형상을 가지고 있는 경우라면 (DXF로 읽어드리고 코일로 지정하는 경우)
                    // Rectangle 로 바꾸어 저장한다.
                    // 만약, Retangle 조건이 아니라면 지나쳐서 Polygon 으로 저장한다.
                    if (PartType == EMKind.COIL)
                    {
                        CFace retFace = makeRectangleFaceInPopup();
                        
                        if (retFace != null )
                        {
                            return retFace;
                        }                            
                    }

                    List<CPoint> listPoint = new List<CPoint>();

                    foreach (CPointUI pointControl in ListPointUI)
                    {
                        // 매번 신규로 생성을 해야 한다.
                        CPoint point = new CPoint();

                        point.X = Double.Parse(pointControl.StrCoordX);
                        point.Y = Double.Parse(pointControl.StrCoordZ);

                        if (pointControl.IsArc == true)
                            point.LineKind = EMLineKind.ARC;
                        else
                            point.LineKind = EMLineKind.STRAIGHT;

                        if (pointControl.IsArcDirection == true)
                            point.DirectionArc = EMDirectionArc.BACKWARD;
                        else
                            point.DirectionArc = EMDirectionArc.FORWARD;

                        listPoint.Add(point);
                    }

                    face.setPolygonPoints(listPoint);

                }

                return face;
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                // null 을 리턴하고 호출하는 측에서 꼭 null 검사를 한다.
                return null;
            }  
        }

        private CFace makeRectangleFaceInPopup()
        {
            double minX, minZ, maxX, maxZ;

            minX = minZ = 1e100;
            maxX = maxZ = -1e100;

            CFace face = new CFace();
            try
            {

                if (false == isRectangleShapeInPopup())
                    // null 을 리턴하고 호출하는 측에서 꼭 null 검사를 한다.
                    return null;

                foreach (CPointUI pointUI in ListPointUI)
                {
                    if (minX > Convert.ToDouble(pointUI.StrCoordX))
                        minX = Convert.ToDouble(pointUI.StrCoordX);

                    if (minZ > Convert.ToDouble(pointUI.StrCoordZ))
                        minZ = Convert.ToDouble(pointUI.StrCoordZ);

                    if (maxX < Convert.ToDouble(pointUI.StrCoordX))
                        maxX = Convert.ToDouble(pointUI.StrCoordX);

                    if (maxZ < Convert.ToDouble(pointUI.StrCoordZ))
                        maxZ = Convert.ToDouble(pointUI.StrCoordZ);
                }

                if (minX >= maxX || minZ >= maxZ)
                    // null 을 리턴하고 호출하는 측에서 꼭 null 검사를 한다.
                    return null;

                face.setRectanglePoints(minX, minZ, maxX, maxZ);
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                // null 을 리턴하고 호출하는 측에서 꼭 null 검사를 한다.
                return null;
            }

            return face;
        }

        public void buttonDraw_Click(object sender, EventArgs e)
        {
            try
            {
                /// 완벽한 입력인 상태에서만 Draw 가 가능한다.
                bool retOK = isInputDataOK();

                if (retOK == false)
                    return;

                /// [문제]
                ///  - Form 에서는 Parent를 사용할 수 없어 Owner 속성을 사용하지만
                ///    종종 Owner 가 null 로 넘어오는 문제가 발생한다.
                /// [해결]
                ///  - PopupShape 창을 생성하기 전에 Owner 속성을 FormMain 으로 초기화 해 두어야
                ///    확실하게 FormMain 을 얻을 수 있다.
                FormMain formMain = ((FormMain)this.Owner);

                if (formMain == null)
                {
                    CNotice.printLogID("CNGM");
                    return;
                }
                    
                /// 형상 유효성 확인을 위해 임시로 생성한다.
                /// 
                CFace faceTemp = makeFaceInPopup();

                if (faceTemp == null)
                {
                    CNotice.noticeWarningID("TWAP1");
                    return;
                }

                if (false == faceTemp.isShapeOK())
                {
                    CNotice.printLogID("TWAP3");
                    return ;
                }
                    
                CScriptFEMM femm = formMain.m_femm;

                femm.deleteAll();

                /// 1. 작업 중인 Face 를 제외하고 형상 그리기
                foreach (CNode node in formMain.m_design.GetNodeList)
                {
                    if (node.GetType().BaseType.Name == "CShapeParts")
                    {
                        if (node.NodeName != m_strPartName)
                        {
                            ((CShapeParts)node).Face.drawFace(femm);
                        }
                    }
                }

                // FEMM 을 최상위로 올린다.
                CProgramFEMM.showFEMM();
                
                // 혹시 FEMM 의 화면이 닫힌 경우 FEMM 의 화면을 복원합니다.
                formMain.reopenFEMM();

                /// 2. 작업중인 Face 형상 그리기
                faceTemp.drawFace(femm);    

            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }  
        }


        /// <summary>
        /// 형상 입력때나 수정때 형상을 다시 그린다
        ///  - 수정 부품만 빼고 타 부품들은 다시 그리고, 수정 부품은 창에 기록된 값을 그린다.
        /// </summary>
        /// <param name="pointControl">좌표점 PointControl</param>
        /// <param name="bRedraw">형상 수정이 없어도 강제로 ARC 변경때 강제로 수정함</param>
        internal void drawTemporaryFace(CPointUI pointControl, bool bRedraw = false)
        {
            try
            {
                /// [문제]
                ///  - Form 에서는 Parent를 사용할 수 없어 Owner 속성을 사용하지만
                ///    종종 Owner 가 null 로 넘어오는 문제가 발생한다.
                /// [해결]
                ///  - PopupShape 창을 생성하기 전에 Owner 속성을 FormMain 으로 초기화 해 두어야
                ///    확실하게 FormMain 을 얻을 수 있다.
                FormMain formMain = ((FormMain)this.Owner);

                if (formMain == null)
                {
                    CNotice.printLogID("CNGM");
                    return;
                }

                CScriptFEMM femm = formMain.m_femm;

                CNode nodeParts = formMain.m_design.getNode(m_strPartName);

                /// 0. 해당 좌표점의 수정이 있었는지를 판단한다.
                ///  - 수정이 있는 경우만 다시 그리기 위해서이다.
                bool retCheck = false;

                /// 초기 생성때는 이전 nodeParts 가 없음으로 사용하지 않고, 기존 노드의 수정때만 사용한다.
                if(nodeParts != null)
                {
                    /// 좌표 Control 에 빈칸이 존재하는 지를 확인함
                    for (int i = 0; i < ListPointUI.Count; i++)
                    {
                        /// 해당 좌표점만 비교한다.
                        /// 만약, Parts 의 모든 좌표점을 비교하면 다른 좌표점이 수정되었을때 나머지 좌표점의 수정이 없어도 다시 그리기가 된다.
                        if (ListPointUI[i] == pointControl)
                        {
                            if (((CShapeParts)nodeParts).Face.RelativePointList[i].X != Double.Parse(ListPointUI[i].StrCoordX.Trim()))
                                retCheck = true;

                            if (((CShapeParts)nodeParts).Face.RelativePointList[i].Y != Double.Parse(ListPointUI[i].StrCoordZ.Trim()))
                                retCheck = true;
                        }
                    }
                }

                // Arc 관련 이벤트 호출이면 강제로 다시그리기를 한다.
                if (bRedraw == true)
                    retCheck = true;

                if(retCheck == true)
                {
                    femm.deleteAll();

                    /// 1. 혹시 수정중이라면, 현재 작업 중인 Face 를 제외하고 형상 그리기
                    foreach (CNode node in formMain.m_design.GetNodeList)
                    {
                        if (node.GetType().BaseType.Name == "CShapeParts")
                        {
                            if (node.NodeName != m_strPartName)
                            {
                                ((CShapeParts)node).Face.drawFace(femm);
                            }
                        }
                    }

                }

                /// 2. 현재 수정중이거나 생성중인 Face 의 형상 그리기 
                retCheck = true;

                /// 좌표 Control 에 빈칸이 존재하는 지를 확인함
                for (int i = 0; i < ListPointUI.Count; i++)
                {
                    if (ListPointUI[i].StrCoordX.Trim().Length == 0)
                        retCheck = false;

                    if (ListPointUI[i].StrCoordZ.Trim().Length == 0)
                        retCheck = false;
                }
                
                double dBase_X, dBase_Y;
                dBase_X = dBase_Y = 0;

                dBase_X = Double.Parse(textBoxBaseX.Text.Trim());
                dBase_Y = Double.Parse(textBoxBaseY.Text.Trim());

                // 모든 데이터가 입력된 경우는 폐곡선의 정상적인 Face 를 그린다.
                if(retCheck == true)
                {
                    CFace faceTemp = makeFaceInPopup();

                    if (null != faceTemp)
                    {
                        faceTemp.drawFace(femm);
                    }
                    else
                    {
                        CNotice.printLogID("TSWN");
                    }
                }
                // 모든 데이터가 아직 입력되지 않은 상태는 입력중인 데이터만으로 그림을 그린다.
                else
                {
                    double dP1_X, dP1_Y, dP2_X, dP2_Y;

                    bool bArc, bArcDirection;

                    dP1_X = dP1_Y = dP2_X = dP2_Y = 0;                   
                    bArc = bArcDirection = false;

                    for (int i = 0; i < ListPointUI.Count; i++)
                    {
                        retCheck = true;

                        if (ListPointUI[i].StrCoordX.Trim().Length == 0)
                            retCheck = false;
                        else
                            dP1_X = Double.Parse(ListPointUI[i].StrCoordX.Trim()) + dBase_X;

                        if (ListPointUI[i].StrCoordZ.Trim().Length == 0)
                            retCheck = false;
                        else
                            dP1_Y = Double.Parse(ListPointUI[i].StrCoordZ.Trim()) + dBase_Y;

                        /// X, Y 값이 모두 입력된 Point Control 인 경우
                        if (retCheck == true)
                        {
                            if (i == 0)
                                /// 사각형, 다각형 모두 적용된다.
                                femm.drawPoint(dP1_X, dP1_Y);
                            else
                            {
                                if(this.FaceType == EMFaceType.RECTANGLE)
                                {
                                    CNotice.printLogID("ATTW");
                                }
                                /// 다각형만 적용된다.
                                /// 만약 사각형의 경우 i = 1 까지 모두 채워지면 모두 입력된 상태로 if 문의 위에처 처리되기 때문이다.
                                bArc = ListPointUI[i].IsArc;
                                bArcDirection = ListPointUI[i].IsArcDirection;
                                    
                                if (bArc == true)
                                    femm.drawArc(dP2_X, dP2_Y, dP1_X, dP1_Y, bArcDirection);
                                else
                                    femm.drawLine(dP2_X, dP2_Y, dP1_X, dP1_Y);
                            }

                            // 이번 점을 이전 점으로 저장한다.
                            dP2_X = dP1_X;
                            dP2_Y = dP1_Y;
                        }
                        /// 채워지지 않은 좌표값을 발견하면 바로 빠져 나간다
                        else
                            break;
                    }
                }

                /// 선택된 좌표점을 붉은 색으로 표시한다.
                /// 
                /// Base X, Y 변경 할때나 Leave 이벤트는 제외해야 함으로 null 을 넘겨오도록 되어 있다.
                if (pointControl != null)
                {
                    /// XY 값 모두 들어 있는 경우에만 표시를 한다.
                    if (pointControl.StrCoordX != "" && pointControl.StrCoordZ != "")
                    {
                        CPoint selectedPoint = new CPoint();

                        selectedPoint.X = Double.Parse(pointControl.StrCoordX) + dBase_X;
                        selectedPoint.Y = Double.Parse(pointControl.StrCoordZ) + dBase_Y;

                        femm.clearSelected();

                        femm.selectPoint(selectedPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }  
        }

        private void buttonFitAll_Click(object sender, EventArgs e)
        {
            FormMain formMain = ((FormMain)this.Owner);

            if (formMain == null)
            {
                CNotice.printLogID("CNGM");
                return;
            }

            // FEMM 을 최상위로 올린다.
            CProgramFEMM.showFEMM();

            // 혹시 FEMM 의 화면이 닫힌 경우 FEMM 의 화면을 복원합니다.
            formMain.reopenFEMM();

            formMain.m_femm.zoomFit();
        }

        private void textBoxBaseY_KeyUp(object sender, KeyEventArgs e)
        {
            /// Enter 에서만 동작한다.
            if (e.KeyCode == Keys.Enter)
            {
                // 다시 그린다.
                drawTemporaryFace(null, true);
            }
        }

        private void textBoxBaseY_Leave(object sender, EventArgs e)
        {
            // 다시 그린다.
            drawTemporaryFace(null, true);
        }

        private void textBoxBaseX_KeyUp(object sender, KeyEventArgs e)
        {
            /// Enter 에서만 동작한다.
            if (e.KeyCode == Keys.Enter)
            {
                // 다시 그린다.
                drawTemporaryFace(null, true);
            }
        }

        private void textBoxBaseX_Leave(object sender, EventArgs e)
        {
            // 다시 그린다.
            drawTemporaryFace(null, true);
        }
    
        private void textBoxBaseY_KeyPress(object sender, KeyPressEventArgs e)
        {
            /// 숫자, 소수점, Back, 엔터만 입력 가능하도록함
            if (!(Char.IsDigit(e.KeyChar)) && e.KeyChar != '.' && e.KeyChar != '-' &&
                    e.KeyChar != Convert.ToChar(Keys.Back) && e.KeyChar != Convert.ToChar(Keys.Enter))
            {
                e.Handled = true;
            }
        }

        private void textBoxBaseX_KeyPress(object sender, KeyPressEventArgs e)
        {
            /// 숫자, 소수점, Back, 엔터만 입력 가능하도록함
            if (!(Char.IsDigit(e.KeyChar)) && e.KeyChar != '.' && e.KeyChar != '-' &&
                    e.KeyChar != Convert.ToChar(Keys.Back) && e.KeyChar != Convert.ToChar(Keys.Enter))
            {
                e.Handled = true;
            }
        }

        private void comboBoxPartType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Type 의 가장 위에 PARTS 가 있어서 1을 더해 준다.
            m_partType = (EMKind)comboBoxNodeType.SelectedIndex + 1;
        }

        private bool isRectangleShapeInPopup()
        {
            bool bCheck = false;

            CPointUI startPointUI, endPointUI;
            double dDistanceX, dDistanceZ;

            if (ListPointUI.Count != 4)
            {
                bCheck = false;
            }
            else
            {
                // 회전하지 않은 직사각형인지를 확인한다.
                for (int i = 0; i < 4; i++)
                {
                    if (i != 3)
                    {
                        startPointUI = ListPointUI[i];
                        endPointUI = ListPointUI[i + 1];
                    }
                    else
                    {
                        startPointUI = ListPointUI[i];
                        endPointUI = ListPointUI[0];
                    }

                    dDistanceX = Math.Abs(Convert.ToDouble(startPointUI.StrCoordX) - Convert.ToDouble(endPointUI.StrCoordX));
                    dDistanceZ = Math.Abs(Convert.ToDouble(startPointUI.StrCoordZ) - Convert.ToDouble(endPointUI.StrCoordZ));

                    // 회전하지 않았으면 모든 라인은 수직이거나 수평이기 때문에 한쪽의 좌표의 거리는 0이 있어야 한다.
                    if (CSmallUtil.isZeroPosition(dDistanceX) == true || CSmallUtil.isZeroPosition(dDistanceZ) == true)
                        bCheck = true;
                    else
                        bCheck = false;
                }
            }

            return bCheck;
        }
    }
}
