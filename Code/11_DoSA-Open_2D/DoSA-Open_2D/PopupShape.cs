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
        const int POPUP_SIZE_Y = 580;

        const int MIN_POLYGON_CONTROL_COUNT = 4;
        const int RECTANGLE_CONTROL_COUNT = 2;

        private EMFaceType m_faceType = EMFaceType.RECTANGLE;

        private List<CPointControl> m_listPointControl = null;

        public string m_strPartName;

        private bool m_bCreatePopupWindow;

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
                        CNotice.printTraceID("AFTE");
                        break;
                }
            }
        }

        /// <summary>
        /// 좌표정보 리스트
        /// </summary>
        public List<CPointControl> ListPointControl
        {
            get
            {
                try 
                { 
                    m_listPointControl = new List<CPointControl>();

                    for (int i = this.panelPointControl.Controls.Count - 1; i >= 0; i--)
                    {
                        m_listPointControl.Add((CPointControl)this.panelPointControl.Controls[i]);
                    }

                    return m_listPointControl;
                }
                catch (Exception ex)
                {
                    CNotice.printTrace(ex.Message);

                    return null;
                }  
            }
        }

        /// <summary>
        /// Face 의 형상 정보가 없는 경우 호출되는 생성자
        /// </summary>
        public PopupShape(EMFaceType drawType, EMKind emKind)
        {
            InitializeComponent();

            m_strPartName = string.Empty;

            m_bCreatePopupWindow = true;
            
            textBoxBaseX.Text = "0.0";
            textBoxBaseY.Text = "0.0";

            switch (emKind)
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
                    CNotice.printTraceID("TPTI");
                    return;
            }
          
            comboBoxFaceType.SelectedItem = drawType.ToString();

            /// 코일의 경우는 코일계산 때문에 Rectangle 로 고정을 해야 한다.
            if (emKind == EMKind.COIL)
                comboBoxFaceType.Enabled = false;
        }

        /// <summary>
        /// Face 의 형상 정보가 있는 경우 호출되는 생성자
        /// </summary>
        public PopupShape(string partName, CFace face, EMKind emKind)
        {
            InitializeComponent();

            m_strPartName = partName;

            m_bCreatePopupWindow = false;

            try
            {
                switch (emKind)
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

                    default:
                        CNotice.printTraceID("TPTI");
                        return;
                }
            
                if (face == null)
                {
                    CNotice.printTraceID("CTPS1");
                    return;
                }

                textBoxBaseX.Text = face.BasePoint.m_dX.ToString();
                textBoxBaseY.Text = face.BasePoint.m_dY.ToString();

                if (face.getPointCount() < MIN_POLYGON_CONTROL_COUNT)
                {
                    CNotice.printTraceID("CTPS");
                    return;
                }

                /// 파트이름을 표시만하고 수정을 하지 못하게 한다.
                textBoxPartName.Text = partName;
                textBoxPartName.Enabled = false;

                if(face.FaceType == EMFaceType.RECTANGLE)
                {
                    // 설정과 동시에 이벤트가 호출되고 
                    // 이벤트함수에서 FaceType 가 지정되면서 Popup 창의 형태와 UserControl 이 초기화 된다.
                    comboBoxFaceType.SelectedItem = EMFaceType.RECTANGLE.ToString();

                    this.ListPointControl[0].StrCoordX = face.RelativePointList[0].m_dX.ToString();
                    this.ListPointControl[0].StrCoordY = face.RelativePointList[0].m_dY.ToString();

                    this.ListPointControl[1].StrCoordX = face.RelativePointList[2].m_dX.ToString();
                    this.ListPointControl[1].StrCoordY = face.RelativePointList[2].m_dY.ToString();            
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
                            this.addPointControl(new CPointControl(), true, this.panelPointControl);

                        this.ListPointControl[i].StrCoordX = face.RelativePointList[i].m_dX.ToString();
                        this.ListPointControl[i].StrCoordY = face.RelativePointList[i].m_dY.ToString();

                        if (face.RelativePointList[i].m_emLineKind == EMLineKind.ARC)
                            this.ListPointControl[i].IsArc = true;
                        else
                            this.ListPointControl[i].IsArc = false;

                        if (face.RelativePointList[i].m_emDirectionArc == EMDirectionArc.BACKWARD)
                            this.ListPointControl[i].IsArcDirection = true;
                        else
                            this.ListPointControl[i].IsArcDirection = false;
                    }
                }
                else
                {
                    CNotice.printTraceID("UAWF");
                    return;
                }

                /// 코일의 경우는 코일계산 때문에 Rectangle 로 고정을 해야 한다.
                if (emKind == EMKind.COIL)
                    comboBoxFaceType.Enabled = false;
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
            }  
        }

        /// <summary>
        /// 사각형 형상입력 창이 뜰때 2개의 Point 컨트롤을 기본으로 한다.
        /// </summary>
        private void addRectanglePointControl(Panel panel)
        {
            for(int i=0 ; i< RECTANGLE_CONTROL_COUNT; i++)
                this.addPointControl(new CPointControl(), false, panel);

            this.resetSequence();
        }

        /// <summary>
        /// 다각형 형상입력 창이 뜰때 4개의 Point 컨트롤을 기본으로 한다.
        /// </summary>
        private void addPolylinePointControl(Panel panel)
        {
            for(int i=0 ; i< MIN_POLYGON_CONTROL_COUNT; i++)
                this.addPointControl(new CPointControl(), true, panel);
       
            this.resetSequence();
        }

        /// <summary>
        /// 좌표입력 컨트롤을 하나 추가한다
        /// </summary>
        /// <param name="pointControl">좌표 객체</param>
        /// <param name="panel">좌표를 추가할 Panel</param>
        private CPointControl addPointControl(CPointControl pointControl, bool showButton, Panel panel)
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
                    CPointControl current = this.addPointControl(new CPointControl(), showButton, panel);

                    /// - 컨트롤 정렬
                    List<CPointControl> sortList = new List<CPointControl>();
                    for (int i = 0; i < panel.Controls.Count; i++)
                    {
                        CPointControl c = panel.Controls[i] as CPointControl;
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
                                ((CPointControl)panelPointControl.Controls[i]).showAddButton();
                            else
                                ((CPointControl)panelPointControl.Controls[i]).hideAddButton();
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
                            ((CPointControl)panelPointControl.Controls[i]).showAddButton();
                        else
                            ((CPointControl)panelPointControl.Controls[i]).hideAddButton();
                    }
                }

                return pointControl;

                #endregion
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return null;
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
                    ((CPointControl)this.panelPointControl.Controls[i]).Sequence = this.panelPointControl.Controls.Count - i;
                }
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
            }  
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {

            if (this.m_faceType != EMFaceType.POLYGON)
            {
                CNotice.printTraceID("TBOA");
                return;
            }

            this.addPointControl(new CPointControl(), true, this.panelPointControl);
        }

        private void comboBoxdrawType_SelectedIndexChanged(object sender, EventArgs e)
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

            for (int i = 0; i < ListPointControl.Count; i++)
            {               
                if (ListPointControl[i].StrCoordX.Trim().Length == 0)
                {
                    CNotice.noticeWarningID("PETC");
                    return false;
                }

                if (ListPointControl[i].StrCoordY.Trim().Length == 0)
                {
                    CNotice.noticeWarningID("PETC");
                    return false;
                }
            }

            string strX1, strY1, strX2, strY2;

            /// 동일한 좌표값이 점이 중복으로 있는 경우
            for (int i = 0; i < ListPointControl.Count - 1; i++)
            {
                for (int j = i + 1; j < ListPointControl.Count; j++)
                {
                    strX1 = ListPointControl[i].StrCoordX.Trim();
                    strY1 = ListPointControl[i].StrCoordY.Trim();
                    strX2 = ListPointControl[j].StrCoordX.Trim();
                    strY2 = ListPointControl[j].StrCoordY.Trim();

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
                    CNotice.printTraceID("CNGM");
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
                return;

            /// 형상 유효성 확인
            /// 
            CFace face = makeFace();

            if (face == null)
            {
                CNotice.noticeWarningID("TWAP1");
                return;
            }

            if (false == face.isShapeOK())
            {
                CNotice.printTraceID("TWAP3");
                return;
            }
            
            m_strPartName = textBoxPartName.Text;
            this.DialogResult = DialogResult.OK;

            FormMain formMain = ((FormMain)this.Owner);

            if (formMain == null)
            {
                CNotice.printTraceID("CNGM");
                return;
            }

            // 혹시 FEMM 의 화면이 닫힌 경우 FEMM 의 화면을 복원합니다.
            formMain.reopenFEMM();
        }

        // Base Point 와 상대좌표로 좌표값이 저장되는 Face 가 생성된다.
        public CFace makeFace()
        {
            try
            {
                CFace face = new CFace();

                face.BasePoint.m_dX = Double.Parse(textBoxBaseX.Text);
                face.BasePoint.m_dY = Double.Parse(textBoxBaseY.Text);

                if (FaceType == EMFaceType.RECTANGLE)
                {
                    if (ListPointControl.Count != 2)
                    {
                        CNotice.printTraceID("TATP1");
                        return null;
                    }

                    double x1, y1, x2, y2;

                    x1 = Double.Parse(ListPointControl[0].StrCoordX);
                    y1 = Double.Parse(ListPointControl[0].StrCoordY);
                    x2 = Double.Parse(ListPointControl[1].StrCoordX);
                    y2 = Double.Parse(ListPointControl[1].StrCoordY);

                    face.setRectanglePoints(x1, y1, x2, y2);
                }
                else
                {
                    if (ListPointControl.Count < 4)
                    {
                        CNotice.printTraceID("TANM");
                        return null;
                    }

                    List<CPoint> listPoint = new List<CPoint>();

                    foreach (CPointControl pointControl in ListPointControl)
                    {
                        // 매번 신규로 생성을 해야 한다.
                        CPoint point = new CPoint();

                        point.m_dX = Double.Parse(pointControl.StrCoordX);
                        point.m_dY = Double.Parse(pointControl.StrCoordY);

                        if (pointControl.IsArc == true)
                            point.m_emLineKind = EMLineKind.ARC;
                        else
                            point.m_emLineKind = EMLineKind.STRAIGHT;

                        if (pointControl.IsArcDirection == true)
                            point.m_emDirectionArc = EMDirectionArc.BACKWARD;
                        else
                            point.m_emDirectionArc = EMDirectionArc.FORWARD;

                        listPoint.Add(point);
                    }

                    face.setPolygonPoints(listPoint);
                }

                return face;
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return null;
            }  
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
                    CNotice.printTraceID("CNGM");
                    return;
                }
                    
                /// 형상 유효성 확인
                /// 
                CFace face = makeFace();

                if (face == null)
                {
                    CNotice.noticeWarningID("TWAP1");
                    return;
                }

                if (false == face.isShapeOK())
                {
                    CNotice.printTraceID("TWAP3");
                    return ;
                }
                    
                CScriptFEMM femm = formMain.m_femm;

                femm.deleteAll();

                /// 1. 작업 중인 Face 를 제외하고 형상 그리기
                foreach (CNode node in formMain.m_design.NodeList)
                {
                    if (node.GetType().BaseType.Name == "CParts")
                    {
                        if (node.NodeName != m_strPartName)
                        {
                            ((CParts)node).Face.drawFace(femm);
                        }
                    }
                }

                // FEMM 을 최상위로 올린다.
                CProgramFEMM.showFEMM();
                
                // 혹시 FEMM 의 화면이 닫힌 경우 FEMM 의 화면을 복원합니다.
                formMain.reopenFEMM();

                /// 2. 작업중인 Face 형상 그리기
                face.drawFace(femm);    

            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
            }  
        }


        /// <summary>
        /// 형상 입력때나 수정때 형상을 다시 그린다
        ///  - 수정 부품만 빼고 타 부품들은 다시 그리고, 수정 부품은 창에 기록된 값을 그린다.
        /// </summary>
        /// <param name="pointControl">좌표점 PointControl</param>
        /// <param name="bRedraw">형상 수정이 없어도 강제로 ARC 변경때 강제로 수정함</param>
        internal void drawTemporaryFace(CPointControl pointControl, bool bRedraw = false)
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
                    CNotice.printTraceID("CNGM");
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
                    for (int i = 0; i < ListPointControl.Count; i++)
                    {
                        /// 해당 좌표점만 비교한다.
                        /// 만약, Parts 의 모든 좌표점을 비교하면 다른 좌표점이 수정되었을때 나머지 좌표점의 수정이 없어도 다시 그리기가 된다.
                        if (ListPointControl[i] == pointControl)
                        {
                            if (((CParts)nodeParts).Face.RelativePointList[i].m_dX != Double.Parse(ListPointControl[i].StrCoordX.Trim()))
                                retCheck = true;

                            if (((CParts)nodeParts).Face.RelativePointList[i].m_dY != Double.Parse(ListPointControl[i].StrCoordY.Trim()))
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
                    foreach (CNode node in formMain.m_design.NodeList)
                    {
                        if (node.GetType().BaseType.Name == "CParts")
                        {
                            if (node.NodeName != m_strPartName)
                            {
                                ((CParts)node).Face.drawFace(femm);
                            }
                        }
                    }

                }

                /// 2. 현재 수정중이거나 생성중인 Face 의 형상 그리기 
                retCheck = true;

                /// 좌표 Control 에 빈칸이 존재하는 지를 확인함
                for (int i = 0; i < ListPointControl.Count; i++)
                {
                    if (ListPointControl[i].StrCoordX.Trim().Length == 0)
                        retCheck = false;

                    if (ListPointControl[i].StrCoordY.Trim().Length == 0)
                        retCheck = false;
                }
                
                double dBase_X, dBase_Y;
                dBase_X = dBase_Y = 0;

                dBase_X = Double.Parse(textBoxBaseX.Text.Trim());
                dBase_Y = Double.Parse(textBoxBaseY.Text.Trim());

                CFace face = null; 

                // 모든 데이터가 입력된 경우는 폐곡선의 정상적인 Face 를 그린다.
                if(retCheck == true)
                {
                    face = makeFace();

                    if (null != face)
                    {
                        face.drawFace(femm);
                    }
                    else
                    {
                        CNotice.printTraceID("TSWN");
                    }
                }
                // 모든 데이터가 아직 입력되지 않은 상태는 입력중인 데이터만으로 그림을 그린다.
                else
                {
                    double dP1_X, dP1_Y, dP2_X, dP2_Y;

                    bool bArc, bArcDirection;

                    dP1_X = dP1_Y = dP2_X = dP2_Y = 0;                   
                    bArc = bArcDirection = false;

                    for (int i = 0; i < ListPointControl.Count; i++)
                    {
                        retCheck = true;

                        if (ListPointControl[i].StrCoordX.Trim().Length == 0)
                            retCheck = false;
                        else
                            dP1_X = Double.Parse(ListPointControl[i].StrCoordX.Trim()) + dBase_X;

                        if (ListPointControl[i].StrCoordY.Trim().Length == 0)
                            retCheck = false;
                        else
                            dP1_Y = Double.Parse(ListPointControl[i].StrCoordY.Trim()) + dBase_Y;

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
                                    CNotice.printTraceID("ATTW");
                                }
                                /// 다각형만 적용된다.
                                /// 만약 사각형의 경우 i = 1 까지 모두 채워지면 모두 입력된 상태로 if 문의 위에처 처리되기 때문이다.
                                bArc = ListPointControl[i].IsArc;
                                bArcDirection = ListPointControl[i].IsArcDirection;
                                    
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
                    if (pointControl.StrCoordX != "" && pointControl.StrCoordY != "")
                    {
                        CPoint selectedPoint = new CPoint();

                        selectedPoint.m_dX = Double.Parse(pointControl.StrCoordX) + dBase_X;
                        selectedPoint.m_dY = Double.Parse(pointControl.StrCoordY) + dBase_Y;

                        femm.clearSelected();

                        femm.selectPoint(selectedPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
            }  
        }

        private void buttonFitAll_Click(object sender, EventArgs e)
        {
            FormMain formMain = ((FormMain)this.Owner);

            if (formMain == null)
            {
                CNotice.printTraceID("CNGM");
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
    }
}
