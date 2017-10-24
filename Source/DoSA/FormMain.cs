using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// Debugging
using System.Diagnostics;

// File
using System.IO;

using System.Reflection;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;

// DoSA 생성 클래스들을 오픈한다
// 같은 namespace 를 사용해도 가능하나 ClassView 에서 보기가 어려워서 구분해서 사용한다.
using Parts;
using Experiments;
using Nodes;
using Scripts;
using Shapes;
using gtLibrary;

namespace DoSA
{
    public partial class FormMain : Form
    {
		// Treeview 접근 INDEX
		const int FIRST_PARTS_INDEX = 0;
		const int FIRST_ANALYSIS_INDEX = 1;

		public CDesign m_design = new CDesign();

        private CManageFile m_manageFile = new CManageFile();

        public CScriptFEMM m_femm = null;

        private string m_strBackupNodeName = string.Empty;

        //-----------------------------------------------------------------------------------------------------
        // 초기화
        //-----------------------------------------------------------------------------------------------------

        #region - 생성자

        public FormMain()
        {
            InitializeComponent();

            // 기존에 동작을 하고 있는 FEMM 이 있으면 오류가 발생한다.
            killProcessOfFEMM();
            
            initializeProgram();
            
            // FEMM 에서 지원되는 재질을 Loading 한다.
            loadMaterial();
        }

        private void killProcessOfFEMM()
        {
            int nCount = 0;
            Process[] processList = null;

            do
            {
                processList = Process.GetProcessesByName("femm");

                if (processList.Length > 0)
                    processList[0].Kill();

                Thread.Sleep(50);

                // 무한 루프를 방지한다.
                if (nCount > 100)
                    return;

                nCount++;

            } while (processList.Length > 0);
        }
        #endregion

        #region - 프로그램 초기화

        private void initializeProgram()
        {
            try
            {
                // 실행파일의 위치를 읽어낸다.
                CSettingData.m_strProgramDirName = System.Windows.Forms.Application.StartupPath;

                // 출력방향을 결정함 (아래 코드가 동작하면 파일 출력, 동작하지 않으면 Output 창 출력)
                Trace.Listeners.Add(new TextWriterTraceListener(Path.Combine(CSettingData.m_strProgramDirName, "Log", DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + ".Log")));

                // 이벤트 생성 부
                // 
                // 내부함수인 printLogEvent() 의 함수포인트를 사용해서 이벤트 함수를 설정한다
                CNotice.Notice += printLogEvent;

                string strSettingFileFullName = Path.Combine(CSettingData.m_strProgramDirName, "setting.ini");

                PopupSetting frmSetting = new PopupSetting();
                frmSetting.StartPosition = FormStartPosition.CenterParent;

                if (false == m_manageFile.isExistFile(strSettingFileFullName))
                {
                    CNotice.noticeWarning("설정 파일을 찾을 수 없습니다.\n환경 설정 후에 프로그램이 실행 됩니다.");

                    // 초기에 환경변수를 설정 할 때는 Optioal 항목들을 보이지 않는 조건으로 설정한다.
                    CSettingData.m_bShowProperyGridCollapse = true;

                    // 파일자체가 없기 때문에 다이얼로그의 데이터 설정없이 바로 호출한다.
                    if (DialogResult.OK == frmSetting.ShowDialog())
                    {
                        frmSetting.saveSettingToFile();
                    }
                    else
                    {
                        CNotice.noticeWarning("초기 설정의 취소하시면 프로그램이 종료 됩니다.");

                        System.Windows.Forms.Application.ExitThread();
                        Environment.Exit(0);
                    }
                }
                else
                {
                    frmSetting.loadSettingFromFile();

                    if (CSettingData.verifyData(false) == false)
                    {
                        CNotice.noticeWarning("환경설정에 문제가 있습니다. \n환경 설정 후에 프로그램이 실행 됩니다.");

                        if (DialogResult.OK == frmSetting.ShowDialog())
                        {
                            frmSetting.saveSettingToFile();
                        }
                        else
                        {
                            CNotice.noticeWarning("초기 설정의 취소하시면 프로그램이 종료 됩니다.");

                            System.Windows.Forms.Application.ExitThread();
                            Environment.Exit(0);
                        }
                    }

                    // WorkingDirectory 을 읽어온 후에 
                    // 작업의 편의를 위해 디렉토리를 WorkingDirectory 로 변경한다.
                    m_manageFile.setCurrentDirectory(CSettingData.m_strWorkingDirName);
                }
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
            }
        }

        #endregion

        #region - Notice Event 호출 함수

        // 이벤트 발생 때 호출되는 함수
        void printLogEvent(EMOutputTarget emTarget, string strMSG)
        {
            if (emTarget == EMOutputTarget.TRACE)
            {
                Trace.WriteLine(DateTime.Now.ToString() + ", " + strMSG);
                Trace.Flush();
            }
            else
            {
                messageListView.addItem(strMSG);
            }
        }

        #endregion

        #region - 재질 초기화
        
        private void loadMaterial()
        {
            List<string> listMaterialName = new List<string>();

            try
            {

                #region //--------------------- 기본 재료 추가하기 -------------------------

                //------------------------------------------------
                // 자기회로 Maxwell 내장 연자성 재료
                //------------------------------------------------
                // 내장 연자성재료를 추가할 때는 BH 곡선의 내장 연자성재료 설정도 같이 변경해 주어야 한다
                CPropertyItemList.steelList.Add("Pure Iron");
                
                CPropertyItemList.steelList.Add("1006 Steel");
                CPropertyItemList.steelList.Add("1010 Steel");
                CPropertyItemList.steelList.Add("1018 Steel");
                CPropertyItemList.steelList.Add("1020 Steel");
                CPropertyItemList.steelList.Add("1117 Steel");

                CPropertyItemList.steelList.Add("416 Stainless Steel");
                CPropertyItemList.steelList.Add("430 Stainless Steel");
                CPropertyItemList.steelList.Add("455 Stainless Steel");

                // 자기회로 Maxwell 내장 비자성 재료
                // 내장 비자성재료를 추가할 때도 BH 곡선의 내장 비자성재료 설정도 같이 변경해 주어야 한다
                CPropertyItemList.steelList.Add("Aluminum, 1100");
                CPropertyItemList.steelList.Add("Copper");
                CPropertyItemList.steelList.Add("316 Stainless Steel");
                CPropertyItemList.steelList.Add("304 Stainless Steel");

                //------------------------------------------------
                // 자기회로 내장 영구자석
                //------------------------------------------------
                CPropertyItemList.magnetList.Add("NdFeB 32 MGOe");
                CPropertyItemList.magnetList.Add("NdFeB 37 MGOe");
                CPropertyItemList.magnetList.Add("NdFeB 40 MGOe");
                CPropertyItemList.magnetList.Add("NdFeB 52 MGOe");
                CPropertyItemList.magnetList.Add("NdFeB 10 MGOe (Bonded)");
                
                CPropertyItemList.magnetList.Add("SmCo 20 MGOe");
                CPropertyItemList.magnetList.Add("SmCo 24 MGOe");
                CPropertyItemList.magnetList.Add("SmCo 27 MGOe");                
                
                CPropertyItemList.magnetList.Add("Alnico 5");
                CPropertyItemList.magnetList.Add("Alnico 6");
                CPropertyItemList.magnetList.Add("Alnico 8");

                CPropertyItemList.magnetList.Add("Ceramic 5");
                CPropertyItemList.magnetList.Add("Ceramic 8");

                //------------------------------------------------
                // 코일 동선 재료
                //------------------------------------------------
                CPropertyItemList.coilWireList.Add("Aluminum");
                CPropertyItemList.coilWireList.Add("Copper");

                #endregion
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
            }

        }
        #endregion

        #region - 전체 초기화
        //전체 초기화 한다
        private void closeDesign()
        {
            // 기존 자료를 초기화 한다
            // Second Node 들을 삭제한다.
            foreach (TreeNode firstLayerNode in treeViewMain.Nodes)
                firstLayerNode.Nodes.Clear();

            // First Node 들을 삭제한다.
            treeViewMain.Nodes.Clear();
            
            m_design.clearDesign();

            splitContainerRight.Panel1.Controls.Clear();
            splitContainerRight.Panel1.Controls.Add(this.panelEmpty);

            // PropertyGrid 창을 초기화 한다.
            propertyGridMain.SelectedObject = null;
        }
        #endregion

        //-----------------------------------------------------------------------------------------------------
        // 이벤트
        //-----------------------------------------------------------------------------------------------------
        
        #region - Ribbon Menu

        private void ribbonButtonNew_Click(object sender, EventArgs e)
        {
            if (m_design.m_bChanged == true)
            {
                if (DialogResult.OK == CNotice.noticeWarningOKCancel("기존 디자인을 저장 하시겠습니까?"))
                {
                    saveDesignFile();
                }
            }

            PopupNew formNew = new PopupNew("Design");
            formNew.StartPosition = FormStartPosition.CenterParent;

            /// 이해할 수 없지만, 자동으로 Owner 설정이 되는 경우도 있고 아닌 경우도 있기 때문에
            /// Shape 창에서 MainForm 을 접근할 수 있도록 미리 설정을 한다.
            formNew.Owner = this;

            if (DialogResult.Cancel == formNew.ShowDialog())
                return;

            // 기존 디자인 데이터를 모두 삭제한다.
            closeDesign();

            // 확장자나 경로가 없는 다지인 명이다.
            string strDesignName = formNew.textBoxName.Text;

            // 빈칸 확인으로 null 비교를 사용하지 말라. 
            // - .Length == 0 나 "" 를 사용하라
            if (strDesignName.Length == 0)
            {
                CNotice.printTrace("디자인명 없이 디자인 생성작업이 진행되고 있습니다.");
                return;
            }

            // 생성을 할 때는 기본 작업 디렉토리를 사용해서 Actuator 작업파일의 절대 경로를 지정하고,
            // 작업파일을 Open 할 때는 파일을 오픈하는 위치에서 작업 디렉토리를 얻어내어 다시 설정한다.
            // 왜냐하면, 만약 작업 디렉토리를 수정하는 경우 기존의 작업파일을 열 수 없기 때문이다.
            string strDesignDirName = Path.Combine(CSettingData.m_strWorkingDirName, strDesignName);

            // 다지인 디렉토리를 생성한다.
            m_manageFile.createDirectory(strDesignDirName);

            m_design.m_strDesignDirName = strDesignDirName;
            m_design.m_strDesignName = strDesignName;


            // 프로젝트가 시작 했음을 표시하기 위해서 TreeView 에 기본 가지를 추가한다.
            TreeNode treeNode = new TreeNode("Parts", (int)EMKind.PARTS, (int)EMKind.PARTS);
            treeViewMain.Nodes.Add(treeNode);

            treeNode = new TreeNode("Experiments", (int)EMKind.EXPERIMENTS, (int)EMKind.EXPERIMENTS);
            treeViewMain.Nodes.Add(treeNode);

            // 수정 되었음을 기록한다.
            m_design.m_bChanged = true;

            /// 새로운 FEMM 을 연다
            openNewFEMM();

            // 제목줄에 디자인명을 표시한다
            this.Text = "Design Toolkit of Solenoid & Actuator - " + m_design.m_strDesignName;

            CNotice.printUserMessage(m_design.m_strDesignName + " 디자인이 생성 되었습니다.");    

        }

        private void ribbonButtonOpen_Click(object sender, EventArgs e)
        {
            if (m_design.m_bChanged == true)
            {
                if (DialogResult.OK == CNotice.noticeWarningOKCancel("기존 디자인을 저장 하시겠습니까?"))
                {
                    saveDesignFile();
                }
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();

            // 파일 열기창 설정
            openFileDialog.Title = "Open a Design File";
            // 디자인 파일을 열 때 디렉토리는 프로그램 작업 디렉토리로 하고 있다.
            openFileDialog.InitialDirectory = CSettingData.m_strWorkingDirName;
            openFileDialog.FileName = null;
            openFileDialog.Filter = "Toolkit Files (*.dsa)|*.dsa|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                // 기존 디자인 데이터를 모두 삭제한다.
                closeDesign();

                string strActuatorDesignFileFullName = openFileDialog.FileName;

                loadDesignFile(strActuatorDesignFileFullName);

                // 디자인 파일이 생성될 때의 디자인 작업 디렉토리는 프로그램 기본 디렉토리 강제 설정하고 있다.
                // 만약 디렉토리를 옮긴 디자인 디렉토리를 오픈 할 경우라면 
                // 이전 다지인 작업 디렉토리를 그대로 사용하면 디렉토리 문제가 발생하여 실행이 불가능하게 된다.
                // 이를 해결하기 위해
                // 작업파일을 Open 할 때는 파일을 오픈하는 위치로 작업파일의 디렉토리를 다시 설정하고 있다.
                m_design.m_strDesignDirName = Path.GetDirectoryName(strActuatorDesignFileFullName);

                // 프로젝트가 시작 했음을 표시하기 위해서 TreeView 에 기본 가지를 추가한다.
                TreeNode treeNode = new TreeNode("Parts", (int)EMKind.PARTS, (int)EMKind.PARTS);
                treeViewMain.Nodes.Add(treeNode);

                treeNode = new TreeNode("Experiments", (int)EMKind.EXPERIMENTS, (int)EMKind.EXPERIMENTS);
                treeViewMain.Nodes.Add(treeNode);

                foreach (CNode node in m_design.NodeList)
                {
                    this.addTreeNode(node.NodeName, node.m_kindKey);
                }
            }
            else
            {
                return;
            }

            openNewFEMM();

            redrawParts();

            // 제목줄에 디자인명을 표시한다
            this.Text = "Design Toolkit of Solenoid & Actuator - " + m_design.m_strDesignName;

            CNotice.printUserMessage(m_design.m_strDesignName + " 디자인이 생성 되었습니다.");    
        }

        private void ribbonOrbMenuItemClose_Click(object sender, EventArgs e)
        {
            if (m_design.m_bChanged == true)
            {
                if (DialogResult.OK == CNotice.noticeWarningOKCancel("디자인을 저장 하시겠습니까?"))
                {
                    saveDesignFile();
                }
            }

            // 기존 디자인 데이터를 모두 삭제한다.
            closeDesign();

            // 제목줄에 디자인명을 삭제한다
            this.Text = "Design Toolkit of Solenoid & Actuator";

            quitFEMM();
        }

        private void ribbonButtonSave_Click(object sender, EventArgs e)
        {
            if (m_design.m_strDesignName.Length == 0)
            {
                CNotice.noticeWarning("저장을 실행할 작업 디자인이 없습니다.\n작업을 취소 합니다.");
                return;
            }

            if (true == saveDesignFile())
            {
                CNotice.noticeInfomation(m_design.m_strDesignName + " 디자인의 저장이 완료 되었습니다.", "저장 알림");
                CNotice.printUserMessage(m_design.m_strDesignName + " 디자인의 저장이 완료 되었습니다.");
            }
        }

        private void ribbonButtonSaveAs_Click(object sender, EventArgs e)
        {
            string strOrgDesignDirName, strSaveAsDesignDirName;
            string strSaveAsDesignName;

            CWriteFile writeFile = new CWriteFile();

            string strOrgDesignName = this.m_design.m_strDesignName;
            strOrgDesignDirName = this.m_design.m_strDesignDirName;

            // 디자인이 없는 경우는 DesignName 없기 때문에 이름으로 작업디자인이 있는지를 판단한다.
            if (strOrgDesignName.Length == 0)
            {
                CNotice.noticeWarning("다른 이름으로 저장을 실행할 작업 디자인이 없습니다.\n작업을 취소 합니다.");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Title = "Write a New Design Name";
            saveFileDialog.InitialDirectory = CSettingData.m_strWorkingDirName;
            saveFileDialog.FileName = strOrgDesignName + "_Modify";

            DialogResult result = saveFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                try
                {
                    // 확장자가 제외된 전체 경로명이 넘어온다.
                    // TofU 는 선택된 Design 명으로 디렉토리를 만들기 때문에 전체 확장자를 제외한 전체 경로명이 SaveAs 디자인 경로명이 된다.
                    strSaveAsDesignDirName = saveFileDialog.FileName;

                    // 신규 디자인 디렉토리가 들어갈 상위 디렉토리를 찾는다.
                    string strSaveAsUpperDirName = Path.GetDirectoryName(strSaveAsDesignDirName);

                    // 확장자가 제외된 디자인 네임을 얻어온다. (전체 경로의 마지막 명칭이다.)
                    strSaveAsDesignName = Path.GetFileName(strSaveAsDesignDirName);

                    // SaveAs 할 디렉토리에 새로운 디렉토리와 겹치는 디렉토리가 있는지를 확인하기 위해서 디렉토리들을 읽어낸다.
                    List<string> listDirectories = m_manageFile.getDirectoryList(strSaveAsUpperDirName);

                    foreach (string directoryName in listDirectories)
                    {
                        // 디자인 이름이 아니라 디자인 이름을 포함한 경로로 존재여부를 판단한다.
                        if (directoryName == strSaveAsDesignDirName)
                        {
                            CNotice.noticeWarning("해당 디렉토리에 이미 존재하는 디자인 명을 선택하였습니다. \n실행을 취소 합니다.");
                            return;
                        }
                    }

                    String strOrgDesingFileFullName = Path.Combine(strOrgDesignDirName, strOrgDesignName + ".dsa");
                    String strSaveAsDesignFileFullName = Path.Combine(strSaveAsDesignDirName, strSaveAsDesignName + ".dsa");


                    #region // --------------- 파일과 디렉토리 복사 ---------------------

                    // SaveAs 디자인 디렉토리 생성
                    m_manageFile.createDirectory(strSaveAsDesignDirName);

                    // 디자인 파일 복사
                    m_manageFile.copyFile(strOrgDesingFileFullName, strSaveAsDesignFileFullName);
                    
                    #endregion                    


                    // 현 모델을 SaveAs 모델명으로 변경한다.
                    m_design.m_strDesignDirName = strSaveAsDesignDirName;
                    m_design.m_strDesignName = strSaveAsDesignName;

                    // 수정모델을 읽어드린 후에 바로 저장한다.
                    saveDesignFile();

                    // 화면을 갱신한다.
                    splitContainerRight.Panel1.Controls.Clear();
                    splitContainerRight.Panel1.Controls.Add(this.panelEmpty);

                    // PropertyGrid 창을 초기화 한다.
                    propertyGridMain.SelectedObject = null;

                    // 제목줄에 디자인명을 변경한다
                    this.Text = "Design Toolkit of Solenoid & Actuator - " + m_design.m_strDesignName;

                    CNotice.noticeInfomation(m_design.m_strDesignName + " 디자인으로 저장 되었습니다.", "다른 이름 저장 알림");
                    CNotice.printUserMessage(m_design.m_strDesignName + " 디자인으로 저장 되었습니다.");

                }
                catch (Exception ex)
                {
                    CNotice.printTrace(ex.Message);
                    return;
                }
            }
        }

        private void ribbonOrbMenuItemExit_Click(object sender, EventArgs e)
        {
            if (m_design.m_bChanged == true)
            {
                if (DialogResult.OK == CNotice.noticeWarningOKCancel("디자인을 저장 하시겠습니까?"))
                {
                    saveDesignFile();
                }
            }

            System.Windows.Forms.Application.Exit();
        }

        private void ribbonButtonCoil_Click(object sender, EventArgs e)
        {
            addRawNode(EMKind.COIL);
        }

        private void ribbonButtonMagnet_Click(object sender, EventArgs e)
        {
            addRawNode(EMKind.MAGNET);
        }
        
        private void ribbonButtonSteel_Click(object sender, EventArgs e)
        {
            addRawNode(EMKind.STEEL);
        }

        private void ribbonButtonForce_Click(object sender, EventArgs e)
        {
            addRawNode(EMKind.FORCE_EXPERIMENT);
        }

        private void ribbonButtonStroke_Click(object sender, EventArgs e)
        {
            addRawNode(EMKind.STROKE_EXPERIMENT);
        }

        private void ribbonButtonCurrent_Click(object sender, EventArgs e)
        {
            addRawNode(EMKind.CURRENT_EXPERIMENT);
        }

        private void ribbonButtonCheckShape_Click(object sender, EventArgs e)
        {

            if (m_design.m_strDesignName.Length == 0)
            {
                CNotice.noticeWarning("형상을 확인할 작업 디자인이 없습니다.\n작업을 취소 합니다.");
                return;
            }

            openNewFEMM();

            redrawParts();
        }

        private void redrawParts()
        {
            List<CLine> listLineAll = new List<CLine>();
            List<CLine> listLine = null;
            CFace face = null;

            m_femm.deleteAll();

            foreach (CNode node in m_design.NodeList)
            {
                if (node.GetType().BaseType.Name == "CParts")
                {
                    CParts nodeParts = (CParts)node;

                    face = nodeParts.Face;

                    if (null != face)
                    {
                        listLine = face.LineList;

                        /// 모든 라인들을 하나의 Line List 에 담는다.
                        foreach (CLine line in listLine)
                            listLineAll.Add(line);

                        nodeParts.Face.drawFace(m_femm, nodeParts.MovingPart);
                    }
                }
            }

            m_femm.zoomFit();
        }

        private bool checkIntersectionLines()
        {
            List<CLine> listLineAll = new List<CLine>();
            List<CLine> listLine = null;
            CFace face = null;

            foreach (CNode node in m_design.NodeList)
            {
                if (node.GetType().BaseType.Name == "CParts")
                {
                    CParts nodeParts = (CParts)node;

                    face = nodeParts.Face;

                    if (null != face)
                    {
                        listLine = face.LineList;

                        /// 모든 라인들을 하나의 Line List 에 담는다.
                        foreach (CLine line in listLine)
                            listLineAll.Add(line);
                    }
                }
            }

            face = new CFace();

            for (int i = 0; i < listLineAll.Count - 1; i++)
            {
                for (int j = i + 1; j < listLineAll.Count; j++)
                {
                    if (true == face.checkIntersectionLine(listLineAll[i], listLineAll[j]))
                    {
                        /// 교차 간섭 함
                        CNotice.noticeWarning("라인 간섭이 발생 했습니다.");

                        return true;
                    }
                }
            }

            return false;
        }

        private CScriptFEMM openNewFEMM(int widthFEMM = 500)
        {
            if(m_femm != null)
            {
                quitFEMM();
            }

            /// 좌측에 FEMM 공간을 확보하기 위해서 DoSA 의 위치를 지정한다
            this.Left = 600;

            m_femm = new CScriptFEMM();

            const int FEMM_WIDTH = 500;

            CProgramFEMM.moveFEMM(this.Location.X - FEMM_WIDTH, this.Location.Y, widthFEMM);

            return m_femm;
        }

        private void resizeFEMM(int widthFEMM = 500)
        {
            if (m_femm == null)
                return;

            this.Left = 600;

            const int FEMM_WIDTH = 500;

            CProgramFEMM.moveFEMM(this.Location.X - FEMM_WIDTH, this.Location.Y, widthFEMM);

            m_femm.zoomFit();
        }

        private void ribbonButtonSetting_Click(object sender, EventArgs e)
        {

            PopupSetting frmSetting = new PopupSetting();

            if (DialogResult.OK == frmSetting.ShowDialog())
                frmSetting.saveSettingToFile();
        }

        private void ribbonButtonHelp_Click(object sender, EventArgs e)
        {
            string strHelpFileFullName = Path.Combine(CSettingData.m_strProgramDirName, "Help", "TofU-2D_User_Guide.pdf");

            if (m_manageFile.isExistFile(strHelpFileFullName) == false)
            {
                CNotice.noticeWarning("도움말 파일이 존재하지 않습니다.\nTofU 디렉토리 > Help > TofU-2D Guide.pdf 를 확인하세요.");
                return;
            }

            System.Diagnostics.Process.Start(strHelpFileFullName);
        }

        private void ribbonButtonAbout_Click(object sender, EventArgs e)
        {
            PopupAboutBox frmAbout = new PopupAboutBox();

            frmAbout.ShowDialog();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 이름이 지정된 Design 만 저장을 확인한다.
            if (m_design.m_bChanged == true)
            {
                if (DialogResult.OK == CNotice.noticeWarningOKCancel("디자인을 저장 하시겠습니까?"))
                {
                    saveDesignFile();
                }
            }
        }

        #endregion

        #region - Button


        private void buttonExperimentCurrent_Click(object sender, EventArgs e)
        {
            CCurrentExperiment currentExperiment = (CCurrentExperiment)propertyGridMain.SelectedObject;

            // 현재 표시되고 있는 PropertyGird 창에서 Experiment 이름을 찾아 낸다
            string strExperimentName = currentExperiment.NodeName;
            string strExperimentDirName = Path.Combine(m_design.m_strDesignDirName, strExperimentName);

            string strExperimentFullName = Path.Combine(strExperimentDirName, strExperimentName + ".fem");
            string strStrokeFileFullName = Path.Combine(strExperimentDirName, strExperimentName + ".csv");


            if (false == verifyCurrentExperiment(currentExperiment))
                return;

            if (m_manageFile.isExistDirectory(strExperimentDirName) == true)
            {
                DialogResult ret = CNotice.noticeWarningOKCancel("이전 실험결과가 존재합니다.\n이전 결과를 삭제하고 진행 하시겠습니까?", "신규실험 진행");

                if (ret == DialogResult.Cancel)
                    return;

                m_manageFile.deleteDirectory(strExperimentDirName);

                // 삭제되는 시간이 필요한 듯 한다.
                Thread.Sleep(1000);
            }

            // 실험 디렉토리를 생성한다.
            m_manageFile.createDirectory(strExperimentDirName);

            // 해석전 현 설정을 저장한다.
            saveDesignFile();

            m_femm = openNewFEMM();

            m_design.getMaterial(m_femm);

            m_design.drawDesign(m_femm);

            /// 전류, 전압을 영으로 설정해서 기본모델을 만든다.
            m_design.setBlockPropeties(m_femm, 0);

            /// Stroke 가 양이면 Plus 만 경계영역을 만들 때 반영하고
            /// Stroke 가 음이면 Minus 만 경계영역을 만들 때 반영한다.
            if (currentExperiment.MovingStroke >= 0)
                m_design.setBoundary(m_femm, currentExperiment.MovingStroke, 0);
            else
                m_design.setBoundary(m_femm, 0, currentExperiment.MovingStroke);

            m_femm.zoomFit(true);

            m_femm.saveAs(strExperimentFullName);

            /// FEMM 기본모델은 구동부 이동전으로 저장하고 해석전에 구동부를 이동해서 해석한다.
            m_femm.moveMovingParts(currentExperiment.MovingStroke);

            double dInitialCurrent = currentExperiment.InitialCurrent;
            double dFinalCurrent = currentExperiment.FinalCurrent;
            int nStepCount = currentExperiment.StepCount;

            double dStepIncrease = Math.Abs(dFinalCurrent - dInitialCurrent) / nStepCount;

            double dCurrent;

            List<string> listString = new List<string>();

            /// 총 계산횟수는 Step + 1 회이다.
            for (int i = 0; i < nStepCount + 1; i++)
            {
                dCurrent = dInitialCurrent + dStepIncrease * i;

                m_design.changeCurrent(m_femm, dCurrent);

                double dForce = m_femm.solveForce();

                string strCurrent = String.Format("{0}", dCurrent);
                string strForce = String.Format("{0}", dForce);

                listString.Add(strCurrent + "," + strForce);
            }

            closePostView();

            CWriteFile writefile = new CWriteFile();

            writefile.writeLineString(strStrokeFileFullName, listString, true);

            plotCurrentResult();

            // Result 버튼이 동작하게 한다.
            buttonLoadCurrentResult.Enabled = true;
        }

        private void plotCurrentResult()
        {
            // 기존 이미지를 숨기로 결과를 표시할 Chart 를 보이게 한다.
            chartCurrentResult.Visible = true;
            pictureBoxCurrent.Visible = false;

            try
            {
                CCurrentExperiment currentExperiment = (CCurrentExperiment)propertyGridMain.SelectedObject;

                //// 현재 표시되고 있는 PropertyGird 창에서 Experiment 이름을 찾아 낸다
                string strExperimentName = currentExperiment.NodeName;
                string strExperimentDirName = Path.Combine(m_design.m_strDesignDirName, strExperimentName);

                string strCurrentFileFullName = Path.Combine(strExperimentDirName, strExperimentName + ".csv");

                if (false == m_manageFile.isExistFile(strCurrentFileFullName))
                {
                    CNotice.noticeWarning("변위-자기력 실험결과가 존재 하지 않습니다.");
                    return;
                }

                List<double> listDataX = new List<double>();
                List<double> listDataY = new List<double>();

                CReadFile readFile = new CReadFile();

                readFile.readCSVData(strCurrentFileFullName, 0, ref listDataX, 0);
                readFile.readCSVData(strCurrentFileFullName, 1, ref listDataY, 0);

                if (listDataX.Count != listDataY.Count)
                {
                    CNotice.printTrace("CSV 파일의 각열의 행수가 일치하지 않습니다.");
                    return;
                }

                double dXMin, dXMax, dYMin, dYMax;

                dXMin = currentExperiment.InitialCurrent;
                dXMax = currentExperiment.FinalCurrent;

                //dYMin = listDataY.Min();
                //dYMax = listDataY.Max();

                dYMin = Double.NaN;
                dYMax = Double.NaN;


                // X 시간축면 스케일을 설정한다.
                drawXYChart(chartCurrentResult, listDataX, listDataY, "Current [A]", "Force [N]", dXMin, dXMax, dYMin, dYMax);

            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                CNotice.printTrace("변위해석 결과의 차트에서 예외가 발생했습니다.");
                return;
            }
        }

        private bool verifyCurrentExperiment(CCurrentExperiment currentExperiment)
        {
            if (currentExperiment.InitialCurrent >= currentExperiment.FinalCurrent)
            {
                CNotice.noticeWarning("최종전류가 초기전류보다 커야 합니다.");
                return false;
            }

            if (currentExperiment.StepCount <= 0)
            {
                CNotice.noticeWarning("전류 스텝은 영보다 커야 합니다.");
                return false;
            }
            
            return true;
        }

        private void buttonCurrentResult_Click(object sender, EventArgs e)
        {
            plotCurrentResult();
        }

        private void buttonMagnetUp_Click(object sender, EventArgs e)
        {
            CNode node = (CNode)propertyGridMain.SelectedObject;

            if ("CMagnet" != node.GetType().Name)
            {
                Trace.WriteLine("Type mismatch in the FormMain:buttonMagnetUp_Click");
                return;
            }

            ((CMagnet)node).emMagnetDirection = EMMagnetDirection.UP;

            propertyGridMain.Refresh();
        }

        private void buttonMagnetDown_Click(object sender, EventArgs e)
        {
            CNode node = (CNode)propertyGridMain.SelectedObject;

            if ("CMagnet" != node.GetType().Name)
            {
                Trace.WriteLine("Type mismatch in the FormMain:buttonMagnetDown_Click");
                return;
            }

            ((CMagnet)node).emMagnetDirection = EMMagnetDirection.DOWN;

            propertyGridMain.Refresh();
        }

        private void buttonMagnetLeft_Click(object sender, EventArgs e)
        {
            CNode node = (CNode)propertyGridMain.SelectedObject;

            if ("CMagnet" != node.GetType().Name)
            {
                Trace.WriteLine("Type mismatch in the FormMain:buttonMagnetLeft_Click");
                return;
            }

            ((CMagnet)node).emMagnetDirection = EMMagnetDirection.LEFT;

            propertyGridMain.Refresh();
        }

        private void buttonMagnetRight_Click(object sender, EventArgs e)
        {
            CNode node = (CNode)propertyGridMain.SelectedObject;

            if ("CMagnet" != node.GetType().Name)
            {
                Trace.WriteLine("Type mismatch in the FormMain:buttonMagnetRight_Click");
                return;
            }

            ((CMagnet)node).emMagnetDirection = EMMagnetDirection.RIGHT;

            propertyGridMain.Refresh();
        }

        private void buttonDesignCoil_Click(object sender, EventArgs e)
        {
            ((CCoil)propertyGridMain.SelectedObject).designCoil();

            propertyGridMain.Refresh();
        }

        private void buttonChangeShape_Click(object sender, EventArgs e)
        {
            CParts nodeParts = (CParts)propertyGridMain.SelectedObject;

            changePartsShape(nodeParts);
        }

        private void changePartsShape(CParts nodeParts)
        {
            try
            {
                // 형상이 설정되지 않는 경우는
                // Part 별로 다른 형상을 기본값으로 PopupShape 객체를 생성한다.
                if (nodeParts.Face == null)
                {
                    CNotice.printTrace("Face 가 초기화 되지 않은 Parts 가 생성되어 있다.");
                    return;
                }

                string partName = nodeParts.NodeName;
 
                PopupShape popupShape = new PopupShape(partName, nodeParts.Face, nodeParts.m_kindKey);
                popupShape.StartPosition = FormStartPosition.CenterParent;

                /// 이해할 수 없지만, 자동으로 Owner 설정이 되는 경우도 있고 아닌 경우도 있기 때문에
                /// Shape 창에서 MainForm 을 접근할 수 있도록 미리 설정을 한다.
                popupShape.Owner = this;

                if (DialogResult.OK == popupShape.ShowDialog(this))
                {
                    CFace face = popupShape.makeFace();

                    if (null != face)
                    {
                        nodeParts.Face = face;

                        /// 형상에 맞추어 코일 설계 사양정보를 초기화 한다.
                        if (nodeParts.m_kindKey == EMKind.COIL)
                            ((CCoil)nodeParts).initialShapeDesignValue();
                    }
                    else
                    {
                        CNotice.noticeWarning("형상이 정상적으로 생성되지 못하였습니다.");
                        CNotice.printTrace("형상이 정상적으로 생성되지 못했다.");
                    }

                    if (false == m_femm.checkLive())
                        openNewFEMM();

                    redrawParts();
                }
                /// 수정한 내용을 Draw 한 후에 취소 되는 경우를 대비해서 빠져나가면서 다시 그리기를 한다.
                else
                {
                    redrawParts();
                    return;
                }                
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                CNotice.printTrace("형상 생성과정에서 예외가 발생했습니다.");
                return;
            }

            // 수정 되었음을 기록한다.
            m_design.m_bChanged = true;

            /// 수정된 코일형상을 프로퍼티에 표시한다.
            propertyGridMain.Refresh();
        }

        private void buttonForceResult_Click(object sender, EventArgs e)
        {
            plotForceResult();
        }

        private void plotForceResult()
        {
            CForceExperiment forceExperiment = (CForceExperiment)propertyGridMain.SelectedObject;

            // 현재 실험의 이름을 m_nodeList 에서 찾지 않고
            // 현재 표시되고 있는 PropertyGird 창에서 Experiment 이름을 찾아 낸다
            string strExperimentName = ((CForceExperiment)propertyGridMain.SelectedObject).NodeName;
            string strExperimentDirName = Path.Combine(m_design.m_strDesignDirName, strExperimentName);

            string desityImageFileFullName = Path.Combine(strExperimentDirName, strExperimentName + ".bmp");
            string strForceFileFullName = Path.Combine(strExperimentDirName, strExperimentName + ".txt");

            bool bCheck = false;

            string strReturn;
            double dForce;
            CReadFile readfile = new CReadFile();

            bCheck = m_manageFile.isExistFile(strForceFileFullName);

            if (bCheck == true)
            {
                strReturn = readfile.pickoutString(strForceFileFullName, "force:", 7, 21);
                dForce = Double.Parse(strReturn);

                textBoxForce.Text = dForce.ToString();
            }
            else
            {
                CNotice.noticeWarning("자기력 가상실험 결과가 존재 하지 않습니다.");
                return;
            }

            bCheck = m_manageFile.isExistFile(desityImageFileFullName);

            if (bCheck == true)
            {
                // 파일을 잡고 있지 않기 위해서 임시 이미지를 사용하고 Dispose 한다.
                Image tmpImage = Image.FromFile(desityImageFileFullName);

                pictureBoxForce.Image = new Bitmap(tmpImage);
                pictureBoxForce.SizeMode = PictureBoxSizeMode.StretchImage;

                // 이미지이 연결을 끊어서 사용 가능하게 한다.
                tmpImage.Dispose();
            }
            else
            {
                CNotice.noticeWarning("자속밀도 패턴이 존재 하지 않습니다.");
                return;
            }
        }

        private bool verifyForceExperiment(CForceExperiment forceExperiment)
        {
            bool bCheck = false;

            foreach (CNode node in m_design.NodeList)
            {
                // 자기회로 재료만 지정
                if (node.GetType().BaseType.Name == "CParts")
                {
                    if (((CParts)node).MovingPart == EMMoving.MOVING)
                        bCheck = true;
                }
            }

            if (bCheck == false)
            {
                CNotice.noticeWarning("자기력 실험을 위해서는 하나 이상의 동작부가 설정되어야 합니다.");
                return false;
            }

            return true;
        }

        private void buttonExperimentForce_Click(object sender, EventArgs e)
        {
            CForceExperiment forceExperiment = (CForceExperiment)propertyGridMain.SelectedObject;

            // 현재 실험의 이름을 m_nodeList 에서 찾지 않고
            // 현재 표시되고 있는 PropertyGird 창에서 Experiment 이름을 찾아 낸다
            string strExperimentName = forceExperiment.NodeName;
            string strExperimentDirName = Path.Combine(m_design.m_strDesignDirName, strExperimentName);

            string strExperimentFullName = Path.Combine(strExperimentDirName, strExperimentName + ".fem");
            string strForceFileFullName = Path.Combine(strExperimentDirName, strExperimentName + ".txt");
            string strFieldImageFullName = Path.Combine(strExperimentDirName, strExperimentName + ".bmp");            
            

            if (false == verifyForceExperiment(forceExperiment))
                return;

            if (m_manageFile.isExistDirectory(strExperimentDirName) == true)
            {
                DialogResult ret = CNotice.noticeWarningOKCancel("이전 실험결과가 존재합니다.\n이전 결과를 삭제하고 진행 하시겠습니까?", "신규실험 진행");

                if (ret == DialogResult.Cancel)
                    return;

                m_manageFile.deleteDirectory(strExperimentDirName);

                // 삭제되는 시간이 필요한 듯 한다.
                Thread.Sleep(1000);
            }

            // 실험 디렉토리를 생성한다.
            m_manageFile.createDirectory(strExperimentDirName);

            // 해석전 현 설정을 저장한다.
            saveDesignFile();

            m_femm = openNewFEMM(1040);

            m_design.getMaterial(m_femm);

            m_design.drawDesign(m_femm);

            m_design.setBlockPropeties(m_femm, forceExperiment.Voltage);

            /// Stroke 가 양이면 Plus 만 경계영역을 만들 때 반영하고
            /// Stroke 가 음이면 Minus 만 경계영역을 만들 때 반영한다.
            if (forceExperiment.MovingStroke >= 0)
                m_design.setBoundary(m_femm, forceExperiment.MovingStroke, 0);
            else
                m_design.setBoundary(m_femm, 0, forceExperiment.MovingStroke);

            m_femm.zoomFit(true);

            /// 저장 전에 이동량을 반영한다.
            m_femm.moveMovingParts(forceExperiment.MovingStroke);

            m_femm.saveAs(strExperimentFullName);
                       
            double dForce = m_femm.solveForce(strFieldImageFullName);

            string strForce = String.Format("{0,15:N5}", dForce);

            closePostView();

            resizeFEMM();

            CWriteFile writefile = new CWriteFile();      

            List<string> listString = new List<string>();

            listString.Add("force:" + strForce);

            writefile.writeLineString(strForceFileFullName, listString, true);

            plotForceResult();

            // Result 버튼이 동작하게 한다.
            buttonLoadForceResult.Enabled = true;

            /// DoSA 를 활성화하여 창을 최상위에 위치시킨다.
            this.Activate();

        }

        private void closePostView()
        {
            m_femm.closePost();

            redrawParts();
        }

        private void buttonStrokeResult_Click(object sender, EventArgs e)
        {
            plotStrokeResult();
        }

        private void buttonExperimentStroke_Click(object sender, EventArgs e)
        {
            CStrokeExperiment strokeExperiment = (CStrokeExperiment)propertyGridMain.SelectedObject;

            double dInitialStroke = strokeExperiment.InitialStroke;
            double dFinalStroke = strokeExperiment.FinalStroke;
            int nStepCount = strokeExperiment.StepCount;

            // 현재 표시되고 있는 PropertyGird 창에서 Experiment 이름을 찾아 낸다
            string strExperimentName = strokeExperiment.NodeName;
            string strExperimentDirName = Path.Combine(m_design.m_strDesignDirName, strExperimentName);

            string strExperimentFullName = Path.Combine(strExperimentDirName, strExperimentName + ".fem");
            string strStrokeFileFullName = Path.Combine(strExperimentDirName, strExperimentName + ".csv");

            
            if (false == verifyStrokeExperiment(strokeExperiment))
                return;

            if (m_manageFile.isExistDirectory(strExperimentDirName) == true)
            {
                DialogResult ret = CNotice.noticeWarningOKCancel("이전 실험결과가 존재합니다.\n이전 결과를 삭제하고 진행 하시겠습니까?", "신규실험 진행");

                if (ret == DialogResult.Cancel)
                    return;

                m_manageFile.deleteDirectory(strExperimentDirName);

                // 삭제되는 시간이 필요한 듯 한다.
                Thread.Sleep(1000);
            }

            // 실험 디렉토리를 생성한다.
            m_manageFile.createDirectory(strExperimentDirName);

            // 해석전 현 설정을 저장한다.
            saveDesignFile();

            m_femm = openNewFEMM();

            m_design.getMaterial(m_femm);

            m_design.drawDesign(m_femm);

            m_design.setBlockPropeties(m_femm, strokeExperiment.Voltage);

            /// 최소, 최대변위의 부호에 따라 해석영역의 크기를 다르게 제작한다.
            if( dFinalStroke > 0 )
            {
                if( dInitialStroke > 0 )
                    m_design.setBoundary(m_femm, dFinalStroke, 0);
                else
                    m_design.setBoundary(m_femm, dFinalStroke, dInitialStroke);
            }
            else
            {
                if( dInitialStroke > 0 )
                {
                    CNotice.printTrace("최소, 최대변위에 발생할 수 없는 경우가 발생했다.");
                    return;
                }
                else
                    m_design.setBoundary(m_femm, 0, dInitialStroke);
            }
            
            m_femm.zoomFit(true);

            m_femm.saveAs(strExperimentFullName);

            double dStepIncrease = Math.Abs(dFinalStroke - dInitialStroke) / nStepCount;

            double dStroke;
 
            List<string> listString = new List<string>();      
                 
            /// 총 계산횟수는 Step + 1 회이다.
            for (int i = 0; i < nStepCount + 1; i++ )
            {
                dStroke = dInitialStroke + dStepIncrease * i;

                /// 항상 초기위치 기준으로 이동한다.
                m_femm.moveMovingParts(dStroke);

                double dForce = m_femm.solveForce();

                /// 해석을 마치고 나면 구동부를 다시 초기위치로 복귀시킨다.
                m_femm.moveMovingParts(-dStroke);

                string strStroke = String.Format("{0}", dStroke);
                string strForce = String.Format("{0}", dForce);

                listString.Add(strStroke + "," + strForce);
            }

            closePostView();

            CWriteFile writefile = new CWriteFile();

            writefile.writeLineString(strStrokeFileFullName, listString, true);

            plotStrokeResult();
            
            // Result 버튼이 동작하게 한다.
            buttonLoadStrokeResult.Enabled = true;
        }

        private void quitFEMM()
        {
            if(m_femm != null)
            {
                CProgramFEMM.killProcessOfFEMM();

                m_femm = null;
            }
        }

        private bool verifyStrokeExperiment(CStrokeExperiment strokeExperiment)
        {
            bool bCheck = false;

            if (strokeExperiment.InitialStroke >= strokeExperiment.FinalStroke)
            {
                CNotice.noticeWarning("최종변위가 초기변위보다 커야 합니다.");
                return false;
            }

            if (strokeExperiment.StepCount <= 0)
            {
                CNotice.noticeWarning("변위 스텝은 영보다 커야 합니다.");
                return false;
            }

            foreach (CNode node in m_design.NodeList)
            {
                // 자기회로 재료만 지정
                if (node.GetType().BaseType.Name == "CParts")
                {
                    if (((CParts)node).MovingPart == EMMoving.MOVING)
                        bCheck = true;
                }
            }

            if (bCheck == false)
            {
                CNotice.noticeWarning("자기력 실험을 위해서는 하나 이상의 동작부가 설정되어야 합니다.");
                return false;
            }

            return true;
        }

        private void plotStrokeResult()
        {
            // 기존 이미지를 숨기로 결과를 표시할 Chart 를 보이게 한다.
            chartStrokeResult.Visible = true;
            pictureBoxStroke.Visible = false;

            try
            {
                CStrokeExperiment strokeExperiment = (CStrokeExperiment)propertyGridMain.SelectedObject;

                //// 현재 표시되고 있는 PropertyGird 창에서 Experiment 이름을 찾아 낸다
                string strExperimentName = strokeExperiment.NodeName;
                string strExperimentDirName = Path.Combine(m_design.m_strDesignDirName, strExperimentName);

                string strStrokeFileFullName = Path.Combine(strExperimentDirName, strExperimentName + ".csv");

                if (false == m_manageFile.isExistFile(strStrokeFileFullName))
                {
                    CNotice.noticeWarning("변위-자기력 실험결과가 존재 하지 않습니다.");
                    return;
                }

                List<double> listDataX = new List<double>();
                List<double> listDataY = new List<double>();

                CReadFile readFile = new CReadFile();

                readFile.readCSVData(strStrokeFileFullName, 0, ref listDataX, 0);
                readFile.readCSVData(strStrokeFileFullName, 1, ref listDataY, 0);

                if (listDataX.Count != listDataY.Count)
                {
                    CNotice.printTrace("CSV 파일의 각열의 행수가 일치하지 않습니다.");
                    return;
                }

                double dXMin, dXMax, dYMin, dYMax;

                dXMin = strokeExperiment.InitialStroke;
                dXMax = strokeExperiment.FinalStroke;

                //dYMin = listDataY.Min();
                //dYMax = listDataY.Max();

                dYMin = Double.NaN;
                dYMax = Double.NaN;
                

                // X 시간축면 스케일을 설정한다.
                drawXYChart(chartStrokeResult, listDataX, listDataY, "Stroke [mm]", "Force [N]", dXMin, dXMax, dYMin, dYMax);

            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                CNotice.printTrace("변위해석 결과의 차트에서 예외가 발생했습니다.");
                return;
            }
        }


        #endregion

        #region - Windows Message

        /// <summary>
        /// FEMM 의 위치가 DoSA 와 연동되도록 한다.
        /// </summary>
        private void FormMain_Move(object sender, EventArgs e)
        {
            int posX = ((FormMain)sender).Location.X;
            int posY = ((FormMain)sender).Location.Y;

            const int FEMM_WIDTH = 500;

            CProgramFEMM.moveFEMM(posX - FEMM_WIDTH, posY);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------
        // 기능 함수
        //-----------------------------------------------------------------------------------------------------

        #region Save & Load Data

        private bool saveDesignFile()
        {
            if (m_design.m_strDesignName.Length == 0)
            {
                CNotice.printTrace("생성되지 않는 디자인을 저장하려고 합니다.");
                return false;
            }

            string strActuatorDesignFileFullName = Path.Combine(m_design.m_strDesignDirName, m_design.m_strDesignName + ".dsa");

            StreamWriter writeStream = new StreamWriter(strActuatorDesignFileFullName);
            CWriteFile writeFile = new CWriteFile();

            // Project 정보를 기록한다.
            writeFile.writeBeginLine(writeStream, "DoSA_Project", 0);

            writeFile.writeDataLine(writeStream, "Writed", DateTime.Now, 1);
            writeFile.writeDataLine(writeStream, "DoSA_Version", Assembly.GetExecutingAssembly().GetName().Version, 1);
            writeFile.writeDataLine(writeStream, "File_Version", FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion, 1);

            m_design.writeObject(writeStream);

            writeFile.writeEndLine(writeStream, "DoSA_Project", 0);


            // 사용자 정보를 기록한다.
            writeFile.writeBeginLine(writeStream, "Check", 0);

            writeFile.writeEndLine(writeStream, "Check", 0);

            writeStream.Close();

            // 저장을 하고 나면 초기화 한다.
            m_design.m_bChanged = false;

            return true;
        }

        private bool loadDesignFile(string strDesignFileFullName)
        {
            CReadFile readFile = new CReadFile();
            List<string> listStringLines = new List<string>();
            List<string> listStringDesignData = new List<string>();
            bool bDesignLine = false;

            try
            {
                // 전체 내용을 읽어드린다.
                readFile.readAllLines(strDesignFileFullName, ref listStringLines);

                foreach (string strLine in listStringLines)
                {
                    // Design 구문 안의 내용만 listDesignActuator 담는다.
                    //
                    // endLine 과 beginLine 을 확인하는 위치가 중요하다. 
                    // Design 의 시작과 끝을 알리는 구문 Line 을 포함하지 않기 위해 endLine확인 -> 복사 -> beginLine확인 순서로 진행한다.
                    if (readFile.isEndLine(strLine) == "Design")
                        bDesignLine = false;

                    // Design 구문만 listStringDesignData 에 담는다
                    if (bDesignLine == true)
                    {
                        listStringDesignData.Add(strLine);
                    }
                    else
                    {
                        if (readFile.isBeginLine(strLine) == "Design")
                            bDesignLine = true;
                    }
                }

                // 저장파일의 Project 영역안의 Design 영역을 분석하여 m_designActuator 로 Design 을 읽어온다
                readObject(listStringDesignData);

            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return false;
            }

            return true;
        }

        // writeObject() 는 종류가 정해져 있는 Parts 객체를 상위클래스 Node 로 호출하기 때문에
        // Virtual 함수를 사용할 수 있어 CDesign 의 멤버변수를 사용할 수 있다.
        // 하지만 readObject() 은 아직 존재하지 않은 객체를 종류별로 만들어야 하기 때문에  
        // CDesign 의 멤버변수를 사용할 수 없어서 MainForm 의 멤버함수로 사용한다.
        internal void readObject(List<string> listStringDesignData)
        {
            string strCommand = string.Empty;
            string strData = string.Empty;
            bool bNodeDataRegion = false;

            CReadFile readFile = new CReadFile();
            List<string> listStringNode = new List<string>();

            foreach (string strLine in listStringDesignData)
            {
                // Design 구문 안의 내용만 listDesignActuator 담는다.
                //
                // endLine 과 beginLine 을 확인하는 위치가 중요하다. 
                // Design 의 시작과 끝을 알리는 구문 Line 을 포함하지 않기 위해 endLine확인 -> 복사 -> beginLine확인 순서로 진행한다.
                strCommand = readFile.isEndLine(strLine);
                if (strCommand != null)
                {
                    bNodeDataRegion = false;

                    // [주의사항]
                    // - m_designActuator.addNode() 는 RemainedShapeName 에서 형상을 찾고 제외함으로 Open 용도로 사용해서는 안되고 생성용도로만 사용하라.
                    switch (strCommand)
                    {
                        // CMagneticParts 하위 객체
                        case "Coil":
                            CCoil coil = new CCoil();
                            if (true == coil.readObject(listStringNode))
                                m_design.NodeList.Add(coil);
                            break;

                        case "Steel":
                            CSteel steel = new CSteel();
                            if (true == steel.readObject(listStringNode))
                                m_design.NodeList.Add(steel);
                            break;

                        case "Magnet":
                            CMagnet magnet = new CMagnet();
                            if (true == magnet.readObject(listStringNode))
                                m_design.NodeList.Add(magnet);
                            break;

                        // CExpriment 하위 객체
                        case "ForceExperiment":
                            CForceExperiment forceExperiment = new CForceExperiment();
                            if (true == forceExperiment.readObject(listStringNode))
                                m_design.NodeList.Add(forceExperiment);
                            break;

                        case "StrokeExperiment":
                            CStrokeExperiment strokeExperiment = new CStrokeExperiment();
                            if (true == strokeExperiment.readObject(listStringNode))
                                m_design.NodeList.Add(strokeExperiment);
                            break;

                        case "CurrentExperiment":
                            CCurrentExperiment currentExperiment = new CCurrentExperiment();
                            if (true == currentExperiment.readObject(listStringNode))
                                m_design.NodeList.Add(currentExperiment);
                            break;

                        default:
                            break;
                    }

                    // End 명령줄이 Node 이외 영역 처리가 되는 것을 막는다. 
                    continue;
                }

                // Node 영역 처리
                if (bNodeDataRegion == true)
                {
                    listStringNode.Add(strLine);
                }
                // Node 이외 영역 처리
                else
                {
                    // Node 데이터 영역의 시작점인지를 확인한다.
                    strCommand = readFile.isBeginLine(strLine);

                    // Node 데이터 시작점이라면 "노드명" 이 리턴된다.
                    if (strCommand != null)
                    {
                        listStringNode.Clear();
                        // 노드 데이터를 List 에 저장하라.
                        bNodeDataRegion = true;
                    }
                    else
                    {
                        readFile.readDataInLine(strLine, ref strCommand, ref strData);

                        switch (strCommand)
                        {
                            case "DesignName":
                                m_design.m_strDesignName = strData;
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
        }
        #endregion

        #region TreeView관련

        private void treeViewMain_DoubleClick(object sender, EventArgs e)
        {
            // [주의] 
            // Node Name 이 SelectedNode.Name 아니라 SelectedNode.Text 에 들어 있다
            string selectedNodeText = this.treeViewMain.SelectedNode.Text;

            if (selectedNodeText != "Parts" && selectedNodeText != "Experiment")
            {
                CNode node = m_design.getNode(selectedNodeText);

                if (node == null)
                {
                    CNotice.printTrace("트리명의 노드가 m_design 에 존재하지 않습니다.");
                    return;
                }

                if (node.GetType().BaseType.Name == "CParts")
                    changePartsShape((CParts)node);

                // 수정 되었음을 기록한다.
                m_design.m_bChanged = true;
            }
        }

        //노드 추가를 위한 유효성검사
        private CNode addRawNode(EMKind emKind)
        {
            string strName = string.Empty;
            bool bRet = false;

            if (m_design.m_strDesignName.Length == 0)
            {
                CNotice.noticeWarning("노드를 추가할 작업 디자인이 없습니다.\n작업을 취소 합니다.");
                return null;
            }

            CNode retNode = null;

            if(emKind == EMKind.COIL || emKind == EMKind.MAGNET || emKind == EMKind.STEEL)
            {
                PopupShape popupShape = null;

                /// Part 별로 다른 형상을 기본값으로 사용한다.
                if (emKind == EMKind.COIL || emKind == EMKind.MAGNET)
                    popupShape = new PopupShape(EMFaceType.RECTANGLE, emKind);
                else
                    popupShape = new PopupShape(EMFaceType.POLYGON, emKind);

                popupShape.StartPosition = FormStartPosition.CenterParent;

                /// 이해할 수 없지만, 자동으로 Owner 설정이 되는 경우도 있고 아닌 경우도 있기 때문에
                /// Shape 창에서 MainForm 을 접근할 수 있도록 미리 설정을 한다.
                popupShape.Owner = this;

                if (DialogResult.Cancel == popupShape.ShowDialog())
                    return null;

                strName = popupShape.m_strPartName;

                //==================================================================
                // 결과리포트에서 지원하는 만큼 복수개의 Coil 과 Spring 을 지원한다.
                //
                // Coil : 2, MAGNET : 4, STEEL : 6
                //==================================================================
                switch (emKind)
                {
                    case EMKind.COIL:
                        CCoil coil = new CCoil();
                        retNode = coil;         /// for return
                        coil.NodeName = strName;
                        coil.m_kindKey = emKind;

                        bRet = m_design.addNode(coil);
                        break;

                    case EMKind.MAGNET:
                        CMagnet magnet = new CMagnet();
                        retNode = magnet;         /// for return
                        magnet.NodeName = strName;
                        magnet.m_kindKey = emKind;

                        bRet = m_design.addNode(magnet);
                        break;

                    case EMKind.STEEL:
                        CSteel steel = new CSteel();
                        retNode = steel;         /// for return
                        steel.NodeName = strName;
                        steel.m_kindKey = emKind;

                        bRet = m_design.addNode(steel);
                        break;    

                    default:
                        CNotice.printTrace("엉뚱한 파트종류가 접근 되었다.");
                        return null;
                }

                CFace face = popupShape.makeFace();

                if (null != face)
                {
                    ((CParts)retNode).Face = face;

                    /// 형상에 맞추어 코일 설계 사양정보를 초기화 한다.
                    if(emKind == EMKind.COIL)
                        ((CCoil)retNode).initialShapeDesignValue();
                }                    
                else
                {
                    CNotice.noticeWarning("형상이 정상적으로 생성되지 못하였습니다.");
                    CNotice.printTrace("형상이 정상적으로 생성되지 못했다.");

                    return null;
                }

                if (false == m_femm.checkLive())
                    openNewFEMM();

                redrawParts();                
            }
            else
            {
                string strKind;

                switch (emKind)
                {
                    case EMKind.FORCE_EXPERIMENT:
                        strKind = "Force Experiment";
                        break;

                    case EMKind.STROKE_EXPERIMENT:
                        strKind = "Stroke Experiment";
                        break;

                    case EMKind.CURRENT_EXPERIMENT:
                        strKind = "Current Experiment";
                        break;

                    default:
                        CNotice.printTrace("종류에 없는 Node 를 생성창을 띄울려고 있다.");
                        return null;
                }

                PopupNew popupNodeName = new PopupNew(strKind);
                popupNodeName.StartPosition = FormStartPosition.CenterParent;

                /// 이해할 수 없지만, 자동으로 Owner 설정이 되는 경우도 있고 아닌 경우도 있기 때문에
                /// Shape 창에서 MainForm 을 접근할 수 있도록 미리 설정을 한다.
                popupNodeName.Owner = this;

                if (DialogResult.Cancel == popupNodeName.ShowDialog())
                    return null;

                strName = popupNodeName.m_strName;

                switch (emKind)
                {
                    case EMKind.FORCE_EXPERIMENT:
                        CForceExperiment forceExperiment = new CForceExperiment();
                        retNode = forceExperiment;         /// for return
                        forceExperiment.NodeName = strName;
                        forceExperiment.m_kindKey = emKind;

                        bRet = m_design.addNode(forceExperiment);
                        break;

                    case EMKind.STROKE_EXPERIMENT:
                        CStrokeExperiment strokeExperiment = new CStrokeExperiment();
                        retNode = strokeExperiment;         /// for return
                        strokeExperiment.NodeName = strName;
                        strokeExperiment.m_kindKey = emKind;

                        bRet = m_design.addNode(strokeExperiment);
                        break;

                    case EMKind.CURRENT_EXPERIMENT:
                        CCurrentExperiment currentExperiment = new CCurrentExperiment();
                        retNode = currentExperiment;         /// for return
                        currentExperiment.NodeName = strName;
                        currentExperiment.m_kindKey = emKind;

                        bRet = m_design.addNode(currentExperiment);
                        break;

                    default:
                        CNotice.printTrace("엉뚱한 파트종류가 접근 되었다.");
                        return null;
                }
            }                

            // 수정 되었음을 기록한다.
            m_design.m_bChanged = true;

            if (bRet == true)
            {
                // Treeview 에 추가한다
                addTreeNode(strName, emKind);
                // 해당 Node 의 Properies View 와 Information Windows 를 표시한다
                showNode(strName);
            }
            else
            {
                CNotice.noticeWarning("이미 사용하고 있는 이름입니다.");
                return null;
            }

            return retNode;
        }

        //노드추가
        private void addTreeNode(string strName, EMKind kind)
        {
            // emKIND 와 imageList 의 이미지와 순서가 같아야 한다
            TreeNode treeNode = new TreeNode(strName, (int)kind, (int)kind);

            // [ 유의사항 ]
            // TreeView 의 구조는 노드안에 노드가 들어있는 구조이다
            // 특정 가지에 TreeNode 를 추가하려면 
            // Nodes[2].Nodes[0].Nodes.Add() 와 같이 TreeNode 를 따라 특정노드로 들어간후에 추가한다
            switch (kind)
            {
                case EMKind.COIL:
                case EMKind.MAGNET:
                case EMKind.STEEL:
                    treeViewMain.Nodes[FIRST_PARTS_INDEX].Nodes.Add(treeNode);
                    break;

                case EMKind.FORCE_EXPERIMENT:
                case EMKind.STROKE_EXPERIMENT:
                case EMKind.CURRENT_EXPERIMENT:
                    treeViewMain.Nodes[FIRST_ANALYSIS_INDEX].Nodes.Add(treeNode);
                    break;

                default:
                    return;
            }

            // 추가후 노드를 선택한다
            treeViewMain.SelectedNode = treeNode;
        }

        //트리 노드를 선택
        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // 신기하게 treeViewMain_Click 나 treeViewMain_MouseUp 이벤트에서 동작시키면 이상하게 동작한다.
            // 그래서 중복 호출이 되더라도 AfterSelect 을 사용한다.
            showNode(this.treeViewMain.SelectedNode.Text);            
        }

        // 선택한 노드를 Information Window 와 Property View 에 보여준다
        private void showNode(string nodeName)
        {
            CNode node = m_design.getNode(nodeName);

            string strExperimentDirName = string.Empty;

            try
            {
                if (node != null)
                {
                    // 프로퍼티창을 변경한다.
                    propertyGridMain.SelectedObject = node;

                    /// 프로퍼티창에서 이름을 변경할 때 기존에 이미 있는 이름을 선택하는 경우
                    /// 복구를 위해 저장해 둔다.
                    m_strBackupNodeName = node.NodeName;

                    // Design Fileds 를 보이는 것을 결정한다.
                    if (CSettingData.m_bShowProperyGridCollapse == true)
                        CollapseOrExpandCategory(propertyGridMain, "Design Fields (optional)", true);
                    else
                        CollapseOrExpandCategory(propertyGridMain, "Design Fields (optional)", false);

                    // Expand Treeview when starting
                    foreach (TreeNode tn in treeViewMain.Nodes)
                        tn.Expand();

                    splitContainerRight.Panel1.Controls.Clear();

                    strExperimentDirName = Path.Combine(m_design.m_strDesignDirName, node.NodeName);

                    if (m_femm != null)
                    {
                        /// 부품이 선택되면 FEMM 에 선택 표시를 한다
                        if (node.GetType().BaseType.Name == "CParts")
                        {
                            /// FEMM 이 없으면 다시 오픈한다.
                            if (false == m_femm.checkLive())
                            {
                                openNewFEMM();
                                redrawParts();
                            }

                            CParts parts = (CParts)node;

                            parts.Face.clearSelected(m_femm);
                            parts.Face.selectFace(m_femm);
                        }
                        /// 부품이 아닌 경우는 선택을 해지한다
                        else
                            m_femm.clearSelected();
                    }

                    switch (node.m_kindKey)
                    {
                        case EMKind.COIL:
                            splitContainerRight.Panel1.Controls.Add(this.panelCoil);
                            break;

                        case EMKind.MAGNET:
                            splitContainerRight.Panel1.Controls.Add(this.panelMagnet);
                            break;

                        case EMKind.STEEL:
                            splitContainerRight.Panel1.Controls.Add(this.panelSteel);

                            CSteel steel = (CSteel)node;
                            drawBHCurve(steel.Material);
                            break;

                        case EMKind.FORCE_EXPERIMENT:

                            string strFieldImageFullName = Path.Combine(strExperimentDirName, node.NodeName + ".txt");

                            // 해석결과가 존재하지 않으면 Result 와 Report 버튼을 비활성화 한다.
                            if (m_manageFile.isExistFile(strFieldImageFullName) == true)
                            {
                                buttonLoadForceResult.Enabled = true;
                            }
                            else
                            {
                                buttonLoadForceResult.Enabled = false;
                            }

                            splitContainerRight.Panel1.Controls.Add(this.panelForce);

                            // 초기이미지가 없어서 이미지를 비우고 있다.
                            loadDefaultImage(EMKind.FORCE_EXPERIMENT);
                            textBoxForce.Text = "0.0";

                            break;

                        case EMKind.STROKE_EXPERIMENT:

                            string strResultForceStrokeFileFullName = Path.Combine(strExperimentDirName, node.NodeName + ".csv");

                            // 해석결과가 존재하지 않으면 Result 와 Report 버튼을 비활성화 한다.
                            if (m_manageFile.isExistFile(strResultForceStrokeFileFullName) == true)
                            {
                                buttonLoadStrokeResult.Enabled = true;
                            }
                            else
                            {
                                buttonLoadStrokeResult.Enabled = false;
                            }

                            splitContainerRight.Panel1.Controls.Add(this.panelStroke);
                            // 해석결과로 Panel 이미지가 변경된 경우를 대비해서 초기이미지로 복원한다.                        
                            loadDefaultImage(EMKind.STROKE_EXPERIMENT);
                            break;

                        case EMKind.CURRENT_EXPERIMENT:

                            string strResultForceCurrentFileFullName = Path.Combine(strExperimentDirName, node.NodeName + ".csv");

                            // 해석결과가 존재하지 않으면 Result 와 Report 버튼을 비활성화 한다.
                            if (m_manageFile.isExistFile(strResultForceCurrentFileFullName) == true)
                            {
                                buttonLoadCurrentResult.Enabled = true;
                            }
                            else
                            {
                                buttonLoadCurrentResult.Enabled = false;
                            }

                            splitContainerRight.Panel1.Controls.Add(this.panelCurrent);
                            // 해석결과로 Panel 이미지가 변경된 경우를 대비해서 초기이미지로 복원한다.                        
                            loadDefaultImage(EMKind.CURRENT_EXPERIMENT);
                            break;

                        default:
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
            }

        }

        //트리뷰에서 삭제 한다
        private void treeView_KeyUp(object sender, KeyEventArgs e)
        {
            // Delete 키에서 Tree 를 삭제한다.
            if (e.KeyCode == Keys.Delete)
            {
                // [주의] 
                // Node Name 이 SelectedNode.Name 아니라 SelectedNode.Text 에 들어 있다
                string selectedNodeText = this.treeViewMain.SelectedNode.Text;

                if (selectedNodeText != "Parts" && selectedNodeText != "Experiment")
                {
                    CNode node = m_design.getNode(selectedNodeText);

                    if (node == null)
                    {
                        CNotice.printTrace("트리명의 노드가 m_design 에 존재하지 않습니다.");
                        return;
                    }

                    // 가상 실험 Node 의 경우는 결과 디렉토리와 연결이 되기 때문에
                    // 해석 결과 디렉토리가 있는 경우는 해석결과를 삭제할지를 물어보고 같이 삭제한다.
                    if (node.GetType().BaseType.Name == "CExperiment")
                    {
                        string strExperimentDirName = Path.Combine(m_design.m_strDesignDirName, node.NodeName);

                        if (m_manageFile.isExistDirectory(strExperimentDirName) == true)
                        {
                            DialogResult ret = CNotice.noticeWarningOKCancel("결과가 존재하는 가상실험입니다.\n삭제를 진행 하시겠습니까?");

                            if (ret == DialogResult.Cancel)
                                return;

                            m_manageFile.deleteDirectory(strExperimentDirName);

                            // 삭제되는 시간이 필요한 듯 한다.
                            Thread.Sleep(1000);
                        }
                    }

                    // 수정 되었음을 기록한다.
                    m_design.m_bChanged = true;

                    this.treeViewMain.SelectedNode.Remove();
                    deleteRawNode(selectedNodeText);
                }

                /// FEMM 이 없으면 다시 오픈한다.
                if (false == m_femm.checkLive())
                    openNewFEMM();

                redrawParts();
            }
        }

        /// 트리를 삭제 한다
        private void deleteRawNode(string nodeName)
        {
            // 내부적으로 명칭배열까지도 모두 삭제한다.
            m_design.deleteNode(nodeName);

            // 정보창을 초기화 한다
            splitContainerRight.Panel1.Controls.Clear();
            splitContainerRight.Panel1.Controls.Add(this.panelEmpty);

            // PropertyGrid 창을 초기화 한다.
            propertyGridMain.SelectedObject = null;
        }

        #endregion

        #region property관련
        //property 수정
        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            CNode node = (CNode)propertyGridMain.SelectedObject;

            string strChangedItemValue = e.ChangedItem.Value.ToString();
            string strChangedItemLabel = e.ChangedItem.Label;
            
            try
            {
                // Node 의 이름이 변경된 경우
                if (strChangedItemLabel == "Node Name")
                {
                    /// Node 이름의 경우 PropertyGrid 에서 수정과 동시에 Node 이름이 같이 변경되기 때문에
                    /// 기존에 존재하는 이름으로 변경했는지의 판단은 Node 이름을 겹치는 것으로 확인해야 한다.
                    if (true == m_design.duplicateNodeName())
                    {
                        CNotice.noticeWarning("이미 사용하고 있는 이름입니다.");

                        /// PropertyGrid 에 Node 를 올릴 때 저장해둔 Node 이름으로 복원한다.
                        node.NodeName = m_strBackupNodeName;

                        propertyGridMain.Refresh();

                        return;
                    }

                    /// 변경된 이름은 입력과 동시에 Node 에 반영되었기 때문에 저장을 불 필요한다.
                    //node.NodeName = strChangedItemValue;

                    // 복원용 이름은 PropertyGrid 에 Node 를 올릴 때만 저장되기 때문에
                    // PropertyGrid 를 갱신하지 않는 상태에서 여러번 이름을 변경하다가 문제가 발생하면
                    // 이전 이름인 PropertyGrid 에 Node 를 올릴 때의 이름으로 복구 된다.
                    // 이를 방지하기 위해서 재대로 저장이 된 경우는 복원용 이름을 변경한다.
                    m_strBackupNodeName = strChangedItemValue;

                    /// 트리의 이름을 변경한다.
                    this.treeViewMain.SelectedNode.Text = strChangedItemValue;
                }            
                
                switch (node.m_kindKey)
                {
                    case EMKind.COIL:

                        CCoil coil = (CCoil)node;

                        if (strChangedItemLabel == "Copper Diameter [mm]" || strChangedItemLabel == "Coil Wire Grade")
                        {
                            coil.WireDiameter = coil.calculateWireDiameter();
                        }

                        break;

                    case EMKind.STEEL:

                        // 연자성체의 재질을 선택한 경우만 실행을 한다.
                        if (strChangedItemLabel == "Part Material")
                        {
                            drawBHCurve(strChangedItemValue);
                        }

                        break;

                    case EMKind.FORCE_EXPERIMENT:

                        CForceExperiment forceExperiment = (CForceExperiment)node;

                        if (e.ChangedItem.Label == "Voltage [V]")
                        {
                            // 총 저항은 합산이 필요함으로 0.0f 로 초기화 한다.
                            double total_resistance = 0.0f;

                            // 총 저항
                            foreach (CNode nodeTemp in m_design.NodeList)
                                if (nodeTemp.m_kindKey == EMKind.COIL)
                                {
                                    total_resistance += ((CCoil)nodeTemp).Resistance;
                                }

                            // 전류
                            if (total_resistance != 0.0f)
                                forceExperiment.Current = (forceExperiment.Voltage / total_resistance);
                            else
                                forceExperiment.Current = 0.0f;
                        }
                        break;

                    case EMKind.STROKE_EXPERIMENT:

                        CStrokeExperiment strokeExperiment = (CStrokeExperiment)node;

                        if (e.ChangedItem.Label == "Voltage [V]")
                        {
                            // 총 저항은 합산이 필요함으로 0.0f 로 초기화 한다.
                            double total_resistance = 0.0f;

                            // 총 저항
                            foreach (CNode nodeTemp in m_design.NodeList)
                                if (nodeTemp.m_kindKey == EMKind.COIL)
                                {
                                    total_resistance += ((CCoil)nodeTemp).Resistance;
                                }

                            // 전류
                            if (total_resistance != 0.0f)
                                strokeExperiment.Current = (strokeExperiment.Voltage / total_resistance);
                            else
                                strokeExperiment.Current = 0.0f;
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
            }

            // 수정 되었음을 기록한다.
            m_design.m_bChanged = true;

            propertyGridMain.Refresh();
        }

        //property Category구성
        private void CollapseOrExpandCategory(PropertyGrid propertyGrid, string categoryName, bool bExpand = false)
        {
            GridItem root = propertyGrid.SelectedGridItem;
            //Get the parent
            while (root.Parent != null)
                root = root.Parent;

            if (root != null)
            {
                foreach (GridItem g in root.GridItems)
                {
                    if (g.GridItemType == GridItemType.Category && g.Label == categoryName)
                    {
                        g.Expanded = bExpand;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Information Window관련

        //steel 그래프를 생성한다
        private void drawBHCurve(String strMaterialName)
        {
            List<double> listH = new List<double>();
            List<double> listB = new List<double>();

            //string strMaxwellMaterialDirName = CSettingData.m_strMaxwellMaterialDirName;
            string strProgramMaterialDirName = Path.Combine(CSettingData.m_strProgramDirName, "Materials");

            // 내장 비자성 재료
            if (strMaterialName == "Aluminum, 1100" || strMaterialName == "Copper" || strMaterialName == "316 Stainless Steel" || strMaterialName == "304 Stainless Steel")
            {
                chartBHCurve.Series.Clear();                    // Series 삭제
                // Series 생성
                System.Windows.Forms.DataVisualization.Charting.Series sBHCurve = chartBHCurve.Series.Add("BH");
                sBHCurve.ChartType = SeriesChartType.Line;      // 그래프 모양을 '선'으로 지정

                listH.Clear();
                listB.Clear();

                // 공기와 같은 투자율로 처리한다.
                for (int x = 0; x <= 30000; x += 5000)
                {
                    listH.Add(x);
                    listB.Add(x * 1.25663706143592E-06);
                }

                drawXYChart(chartBHCurve, listH, listB, "H [A/m]", "B [T]", 0.0f, 30000.0f, 0.0f, 2.5f);
            }
            /// 내장 연자성 재료
            else
            {
                string strMaterialFileFullName = Path.Combine(strProgramMaterialDirName, "FEMM.smat");

                if (true == getMaterialBHData(strMaterialFileFullName, strMaterialName, ref listH, ref listB))
                {
                    if (listH.Count == 0)
                    {
                        CNotice.printTrace("선택한 연자성재료의 데이터가 존재하지 않습니다.");
                        return;
                    }

                    if (listH.Count != listB.Count)
                    {
                        CNotice.printTrace("BH 데이터의 XY 값 크기가 일치하지 않습니다.");
                        return;
                    }

                    drawXYChart(chartBHCurve, listH, listB, "H [A/m]", "B [T]", 0.0f, 30000.0f, 0.0f, 2.5f);
                }
            }

        }

        //Marerial 데이터를 읽어온다
        private bool getMaterialBHData(string strFileFullName, string strMeterialName, ref List<double> listH, ref List<double> listB)
        {
            List<string> listString = new List<string>();
            CReadFile readFile = new CReadFile();

            string strLine, strName, strData, strTemp;
            int iIndex;
            bool bBHDataGathering = false;

            // 이전에 사용하던 List 데이터를 우선 삭제한다.
            listH.Clear();
            listB.Clear();

            if (false == m_manageFile.isExistFile(strFileFullName))
            {
                CNotice.printTrace("재질 파일이 존재하지 않습니다.");
                return false;
            }

            try
            {
                readFile.readAllLines(strFileFullName, ref listString);

                for (int i = 0; i < listString.Count; i++)
                {
                    strLine = listString[i];

                    // keyword 앞을 \t 를 제거한다.
                    strLine = strLine.Trim();

                    if (strLine == "$begin \'MaterialDef\'")
                    {
                        strName = listString[i + 1];

                        strName = strName.Trim();

                        iIndex = strName.IndexOf("'");
                        strTemp = strName.Substring(iIndex + 1);
                        iIndex = strTemp.IndexOf("'");
                        strTemp = strTemp.Remove(iIndex);

                        // BH 곡선 수집을 시작한다.
                        if (strTemp == strMeterialName)
                            bBHDataGathering = true;
                    }

                    // BH 곡선 수집을 종료한다.
                    if (strLine == "$end \'MaterialDef\'")
                        bBHDataGathering = false;

                    if (strLine == "$begin \'Coordinate\'" && bBHDataGathering == true)
                    {
                        // H 값 읽기
                        strData = listString[i + 1];
                        strTemp = strData.Trim();       // 앞의 공백 제거함    
                        strTemp = strTemp.Substring(2); // X= 제거함
                        listH.Add(Convert.ToDouble(strTemp));

                        // B 값 읽기
                        strData = listString[i + 2];
                        strTemp = strData.Trim();       // 앞의 공백 제거함
                        strTemp = strTemp.Substring(2); // Y= 제거함
                        listB.Add(Convert.ToDouble(strTemp));

                        // 하나의 BH 곡선을 읽고 나면
                        // 아래의 X, Y, $end 3 line 은 그냥 점프 한다.
                        i = i + 3;
                    }
                }
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                CNotice.printTrace("Maxwell 재질파일 로딩에 예외가 발생했습니다.");
                return false;
            }

            return true;
        }

        //Information Window에 Stroke 이미지로드
        private void loadDefaultImage(EMKind kind)
        {
            string strImageFullFileName;
            bool bRet = false;

            try
            { 
                switch (kind)
                {
                    case EMKind.FORCE_EXPERIMENT:
                        // 이미지를 비운다
                        pictureBoxForce.Image = null;
                        break;

                    case EMKind.STROKE_EXPERIMENT:

                        // Chart 를 대신해서 이미지가 보이게 한다.
                        chartStrokeResult.Visible = false;
                        pictureBoxStroke.Visible = true;

                        strImageFullFileName = Path.Combine(CSettingData.m_strProgramDirName, "Images", "Stroke_Information.png");
                        bRet = m_manageFile.isExistFile(strImageFullFileName);

                        if (bRet == true)
                        {
                            // 파일을 잡고 있지 않기 위해서 임시 이미지를 사용하고 Dispose 한다.
                            Image tmpImage = Image.FromFile(strImageFullFileName);

                            pictureBoxStroke.Image = new Bitmap(tmpImage);

                            // 보유하고 있는 이미지는 이미 크기가 맞게 되어 있으므로 STretch 를 하지 않는다.
                            // 괜히 진행하면 이미지만 깨어져 보인다.
                            //pictureBoxStroke.SizeMode = PictureBoxSizeMode.StretchImage;

                            // 이미지이 연결을 끊어서 사용 가능하게 한다.
                            tmpImage.Dispose();
                        }
                        else
                        {
                            CNotice.printTrace("Imformation 이미지를 읽어드릴 수 없습니다.");
                            return;
                        }

                        break;

                    case EMKind.CURRENT_EXPERIMENT:

                        // Chart 를 대신해서 이미지가 보이게 한다.
                        chartCurrentResult.Visible = false;
                        pictureBoxCurrent.Visible = true;

                        strImageFullFileName = Path.Combine(CSettingData.m_strProgramDirName, "Images", "Current_Information.png");
                        bRet = m_manageFile.isExistFile(strImageFullFileName);

                        if (bRet == true)
                        {
                            // 파일을 잡고 있지 않기 위해서 임시 이미지를 사용하고 Dispose 한다.
                            Image tmpImage = Image.FromFile(strImageFullFileName);

                            pictureBoxCurrent.Image = new Bitmap(tmpImage);

                            // 보유하고 있는 이미지는 이미 크기가 맞게 되어 있으므로 STretch 를 하지 않는다.
                            // 괜히 진행하면 이미지만 깨어져 보인다.
                            //pictureBoxStroke.SizeMode = PictureBoxSizeMode.StretchImage;

                            // 이미지이 연결을 끊어서 사용 가능하게 한다.
                            tmpImage.Dispose();
                        }
                        else
                        {
                            CNotice.printTrace("Imformation 이미지를 읽어드릴 수 없습니다.");
                            return;
                        }

                        break;

                    case EMKind.COIL:
                    case EMKind.MAGNET:
                    case EMKind.STEEL:
                        break;

                    default:
                        return;
                }
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }           
        }

        private void drawXYChart(System.Windows.Forms.DataVisualization.Charting.Chart chartData,
                                List<double> listX, List<double> listY,
                                string strXLabel = "X", string strYLabel = "Y",
                                double dMinX = Double.NaN, double dMaxX = Double.NaN, double dMinY = Double.NaN, double dMaxY = Double.NaN)
        {
            try
            {
                chartData.Series.Clear();                    // Series 삭제

                // Series 생성
                System.Windows.Forms.DataVisualization.Charting.Series sCurve = chartData.Series.Add("Data");
                sCurve.ChartType = SeriesChartType.Line;      // 그래프 모양을 '선'으로 지정

                // 데이터를 추가한다.
                for (int i = 0; i < listX.Count; i++)
                    sCurve.Points.AddXY(listX[i], listY[i]);

                chartData.ChartAreas[0].AxisX.Title = strXLabel;
                chartData.ChartAreas[0].AxisY.Title = strYLabel;
                chartData.ChartAreas[0].AxisX.TitleFont = new System.Drawing.Font("Arial", 12, FontStyle.Regular);
                chartData.ChartAreas[0].AxisY.TitleFont = new System.Drawing.Font("Arial", 12, FontStyle.Regular);

                chartData.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";

                if (dMinX >= dMaxX || dMinY >= dMaxY)
                {
                    CNotice.printTrace("Chart 축 크기설정에 최소값이 오히려 최대값보다 크게 설정되었습니다.");
                    return;
                }

                chartData.ChartAreas[0].AxisX.Minimum = dMinX;    // X 축 범위 지정
                chartData.ChartAreas[0].AxisX.Maximum = dMaxX;

                // 축 스케일을 입력하는 경우는 무조건 구분등분을 5로 설정하였다.
                if (dMinX != double.NaN)
                    chartData.ChartAreas[0].AxisX.Interval = (dMaxX - dMinX) / 5.0f;

                chartData.ChartAreas[0].AxisY.Minimum = dMinY;   // Y1 축 범위 지정
                chartData.ChartAreas[0].AxisY.Maximum = dMaxY;

                // 축 스케일을 입력하는 경우는 무조건 구분등분을 5로 설정하였다.
                if (dMinY != double.NaN)
                    chartData.ChartAreas[0].AxisY.Interval = (dMaxY - dMinY) / 5.0f;

                chartData.ChartAreas[0].RecalculateAxesScale();

                sCurve.Color = Color.SteelBlue;
                sCurve.BorderWidth = 2;
                sCurve.MarkerColor = Color.SteelBlue;
                sCurve.MarkerSize = 8;
                sCurve.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Diamond;
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                CNotice.printTrace("XY Chart 출력에서 예외가 발생했습니다.");
                return;
            }
        }

        #endregion

    }
}
