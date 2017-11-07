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

        public string m_strPartName = string.Empty;

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
                        this.panelPointLine.Controls.Clear();
                        this.m_faceType = EMFaceType.RECTANGLE;

                        this.addRectanglePointControl(this.panelPointLine);
                        this.Size = new Size(POPUP_SIZE_X, POPUP_SIZE_Y);

                        break;

                    case EMFaceType.POLYGON:
                        this.panelPointLine.Controls.Clear();
                        this.m_faceType = EMFaceType.POLYGON;

                        this.addPolylinePointControl(this.panelPointLine);
                        this.Size = new Size(POPUP_SIZE_X, POPUP_SIZE_Y);
                        break;

                    default:
                        CNotice.printTrace("형상입력 Form 의 Face Type 오류가 발생했다.");
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

                    for (int i = this.panelPointLine.Controls.Count - 1; i >= 0; i--)
                    {
                        m_listPointControl.Add((CPointControl)this.panelPointLine.Controls[i]);
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
                    CNotice.printTrace("형상 생성에서 파트종류가 잘못 호출 되었다.");
                    return;
            }
          
            comboBoxFaceType.SelectedItem = drawType.ToString();

            /// 코일의 경우는 코일계산 때문에 Rectangle 로 고정을 해야 한다.
            if (emKind == EMKind.COIL)
                comboBoxFaceType.Enabled = false;

            textBoxBaseX.Text = "0.0";
            textBoxBaseY.Text = "0.0";
        }

        /// <summary>
        /// Face 의 형상 정보가 있는 경우 호출되는 생성자
        /// </summary>
        public PopupShape(string partName, CFace face, EMKind emKind)
        {
            InitializeComponent();

            m_strPartName = partName;

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
                        CNotice.printTrace("형상 생성에서 파트종류가 잘못 호출 되었다.");
                        return;
                }
            
                if (face == null)
                {
                    CNotice.printTrace("null CFace 로 PopupShape 생성자를 호출했다.");
                    return;
                }

                textBoxBaseX.Text = face.BasePoint.m_dX.ToString();
                textBoxBaseY.Text = face.BasePoint.m_dY.ToString();

                if (face.getPointCount() < MIN_POLYGON_CONTROL_COUNT)
                {
                    CNotice.printTrace("CPointLine 가 4개 미만인 CFace 로 PopupShape 생성자를 호출했다.");
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
                            this.addPointControl(new CPointControl(), true, this.panelPointLine);

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
                    CNotice.printTrace("사용하지 않는 Face Type 를 사용했다.");
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
            this.addPointControl(new CPointControl(), false, panel);
            this.addPointControl(new CPointControl(), false, panel);

            this.resetSequence();
        }

        /// <summary>
        /// 다각형 형상입력 창이 뜰때 4개의 Point 컨트롤을 기본으로 한다.
        /// </summary>
        private void addPolylinePointControl(Panel panel)
        {
            this.addPointControl(new CPointControl(), true, panel);
            this.addPointControl(new CPointControl(), true, panel);
            this.addPointControl(new CPointControl(), true, panel);
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
                /// addControlPointLine() 이 호출될 때 실행되지 않고 이벤트가 발생할 때 동작한다.
                /// addControlPointLine() 안에 있어서 넘어오는 파라메터에 바로 접근함으로서 
                /// Event 호출에서 문제가 되는 변수 접근 문제를 해결할 수 있다.
                pointControl.AddCoordinates += (s, e) =>
                {
                    /// 추가 버튼 클릭했을때 현재 컨트롤 아래 추가
                    /// 1. 컨트롤 추가
                    CPointControl current = this.addPointControl(new CPointControl(), showButton, panel);

                    /// 2. 컨트롤 정렬
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
                        if (this.panelPointLine.Controls.Count == RECTANGLE_CONTROL_COUNT)
                        {
                            e.Cancel = true;
                            MessageBox.Show("Rectangle 일때는 좌표가 2개 필요하므로 삭제할 수 없습니다.");
                            return;
                        }
                    }
                    else
                    {
                        if (this.panelPointLine.Controls.Count <= MIN_POLYGON_CONTROL_COUNT)
                        {
                            e.Cancel = true;
                            MessageBox.Show("Polygon 일때는 좌표가 4개 이상 필요하므로 삭제할 수 없습니다.");
                            return;
                        }
                    }
                };

                /// addControlPointLine() 이 호출될 때 실행되지 않고 이벤트가 발생할 때 동작한다.
                pointControl.RemoveComplete += (s, e) =>
                {
                    this.resetSequence();
                };

                panel.Controls.Add(pointControl);
                pointControl.BringToFront();

                this.resetSequence();

                /// PointControl Panel 에서 가장 아래의 PointControl ADD 버튼만 보이게 하여
                /// PointControl 의 삽입이 항상 아래에서만 이루어지도록 해서 Tab 의 문제를 해결 했다.
                for (int i = 0; i < panelPointLine.Controls.Count; i++)
                {
                    if (i == 0)
                        ((CPointControl)panelPointLine.Controls[i]).showAddButton();
                    else
                        ((CPointControl)panelPointLine.Controls[i]).hideAddButton();
                }

                return pointControl;
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return null;
            }  
        }
           
        
        /// <summary>
        /// 순번 다시 채번
        /// </summary>
        private void resetSequence()
        {
            try
            {
                for (int i = 0; i < this.panelPointLine.Controls.Count; i++)
                {
                    ((CPointControl)this.panelPointLine.Controls[i]).Sequence = this.panelPointLine.Controls.Count - i;
                }
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
            }  
        }

        /// <summary>
        /// 좌표 clear
        /// </summary>
        public void clearControls()
        {
            this.panelPointLine.Controls.Clear();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {

            if (this.m_faceType != EMFaceType.POLYGON)
            {
                CNotice.printTrace("Rectangle 에서 Add 라인 버튼이 클릭 되었다.");
                return;
            }

            this.addPointControl(new CPointControl(), true, this.panelPointLine);
        }

        private void comboBoxdrawType_SelectedIndexChanged(object sender, EventArgs e)
        {
            FaceType = (EMFaceType)comboBoxFaceType.SelectedIndex;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private bool verifyInputData()
        {
            if (textBoxPartName.Text.Length == 0)
            {
                CNotice.noticeWarning("파트 이름을 입력해 주십시요");
                return false;
            }

            for (int i = 0; i < ListPointControl.Count; i++)
            {               
                if (ListPointControl[i].StrCoordX.Trim().Length == 0)
                {
                    CNotice.noticeWarning("좌표값을 입력해 주십시요");
                    return false;
                }

                if (ListPointControl[i].StrCoordY.Trim().Length == 0)
                {
                    CNotice.noticeWarning("좌표값을 입력해 주십시요");
                    return false;
                }
            }

            /// 2. 형상 유효성 확인
            /// 
            CFace face = makeFace();

            if( face == null )
            {
                CNotice.noticeWarning("형상 생성에 문제가 있습니다.");
                return false;
            }
                
            if( false == face.checkFaceShape())
            {
                CNotice.noticeWarning("Face 형상에 문제가 발생했습니다. \n(라인 교차 or 음의 X 좌표)");
                return false;
            }

            /// 형상의 초기생성인지 수정인지를 m_strPartName 의 유무로 파악한다.
            if(m_strPartName.Length == 0)
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
                    CNotice.printTrace("부모 창인 Main Form 을 얻어오지 못했다.");
                    return false;
                }

                if (true == formMain.m_design.isExistNode(textBoxPartName.Text))
                {
                    CNotice.noticeWarning("이미 존재하는 Part 입니다.");
                    return false;
                }
            }
            
            return true;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            /// 완벽한 입력인 상태에서만 Draw 및 저장이 가능한다.
            bool retVerify = verifyInputData();

            if(retVerify == true)
            {
                m_strPartName = textBoxPartName.Text;
                this.DialogResult = DialogResult.OK;
            }
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
                        CNotice.printTrace("사각형 형상 입력의 좌표점이 두개가 아닙니다.");
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
                        CNotice.printTrace("다각형 형상의 좌표점이 4개 이하 입니다.");
                        return null;
                    }

                    List<CPoint> listPointLine = new List<CPoint>();

                    foreach (CPointControl controlPointLine in ListPointControl)
                    {
                        // 매번 신규로 생성을 해야 한다.
                        CPoint pointLine = new CPoint();

                        pointLine.m_dX = Double.Parse(controlPointLine.StrCoordX);
                        pointLine.m_dY = Double.Parse(controlPointLine.StrCoordY);

                        if (controlPointLine.IsArc == true)
                            pointLine.m_emLineKind = EMLineKind.ARC;
                        else
                            pointLine.m_emLineKind = EMLineKind.STRAIGHT;

                        if (controlPointLine.IsArcDirection == true)
                            pointLine.m_emDirectionArc = EMDirectionArc.BACKWARD;
                        else
                            pointLine.m_emDirectionArc = EMDirectionArc.FORWARD;

                        listPointLine.Add(pointLine);
                    }

                    face.setPolygonPoints(listPointLine);
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
                bool retVerify = verifyInputData();

                if (retVerify == true)
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
                        CNotice.printTrace("부모 창인 Main Form 을 얻어오지 못했다.");
                        return;
                    }

                    CScriptFEMM femm = formMain.m_femm;

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

                    /// 2. 현재 수정중이거나 생성중인 Face 의 형상 그리기 
                    CFace face = makeFace();

                    if (null != face)
                    {
                        face.drawFace(femm);
                    }
                    else
                    {
                        CNotice.noticeWarning("형상이 정상적으로 생성되지 못하였습니다.");
                        CNotice.printTrace("형상이 정상적으로 생성되지 못했다.");
                    }

                    CProgramFEMM.showFEMM();

                    //femm.zoomFit();
                }
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
            }  
        }

        internal void drawTemporaryFace()
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
                    CNotice.printTrace("부모 창인 Main Form 을 얻어오지 못했다.");
                    return;
                }

                CScriptFEMM femm = formMain.m_femm;

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

                /// 2. 현재 수정중이거나 생성중인 Face 의 형상 그리기 
                bool retCheck = true;

                /// 좌표 Control 에 빈칸이 존재하는 지를 확인함
                for (int i = 0; i < ListPointControl.Count; i++)
                {
                    if (ListPointControl[i].StrCoordX.Trim().Length == 0)
                        retCheck = false;

                    if (ListPointControl[i].StrCoordY.Trim().Length == 0)
                        retCheck = false;
                }

                CFace face = null; 

                // 모든 데이터가 입력되어 있는 상태에서 수정만 일어나는 경우
                if(retCheck == true)
                {
                    face = makeFace();

                    if (null != face)
                    {
                        face.drawFace(femm);
                    }
                    else
                    {
                        CNotice.printTrace("형상이 정상적으로 생성 되지 못했다.");
                    }
                }
                // 모든 데이터가 아직 입력되지 않은 상태
                else
                {
                    float fX, fY, fPX, fPY;
                    fX = fY = fPX = fPY = 0;

                    bool bArc, bArcDirection;
                    bArc = bArcDirection = false;

                    for (int i = 0; i < ListPointControl.Count; i++)
                    {
                        retCheck = true;

                        if (ListPointControl[i].StrCoordX.Trim().Length == 0)
                            retCheck = false;
                        else
                            fX = (float)Double.Parse(ListPointControl[i].StrCoordX.Trim());

                        if (ListPointControl[i].StrCoordY.Trim().Length == 0)
                            retCheck = false;
                        else
                            fY = (float)Double.Parse(ListPointControl[i].StrCoordY.Trim());

                        if (retCheck == true)
                        {
                            if (i == 0)
                                /// 사각형, 다각형 모두 적용된다.
                                femm.drawPoint(fX, fY);
                            else
                            {
                                if(this.FaceType == EMFaceType.RECTANGLE)
                                {
                                    CNotice.printTrace("들어오지 말아야할 위치로 접근이 발생했다.");
                                }
                                /// 다각형만 적용된다.
                                /// 만약 사각형의 경우 i = 1 까지 모두 채워지면 모두 입력된 상태로 if 문의 위에처 처리되기 때문이다.
                                bArc = ListPointControl[i].IsArc;
                                bArcDirection = ListPointControl[i].IsArcDirection;
                                    
                                if (bArc == true)
                                    femm.drawArc(fPX, fPY, fX, fY, bArcDirection);
                                else
                                    femm.drawLine(fPX, fPY, fX, fY);
                            }

                            fPX = fX;
                            fPY = fY;
                        }
                        /// 채워지지 않은 좌표값을 발견하면 바로 빠져 나간다
                        else
                            break;
                    }
                }

                //femm.zoomFit();
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
            }  
        }

        private void buttonFitAll_Click(object sender, EventArgs e)
        {
            FormMain formMain = ((FormMain)this.Owner);

            CScriptFEMM femm = formMain.m_femm;

            femm.zoomFit();

            CProgramFEMM.showFEMM();
        }

        private void textBoxBaseY_KeyUp(object sender, KeyEventArgs e)
        {
            /// Enter 에서만 동작한다.
            if (e.KeyCode == Keys.Enter)
            {
                drawTemporaryFace();
            }
        }

        private void textBoxBaseY_Leave(object sender, EventArgs e)
        {
            drawTemporaryFace();
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
