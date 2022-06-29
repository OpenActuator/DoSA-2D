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
using Tests;
using Nodes;
using Scripts;
using Shapes;
using gtLibrary;

using Microsoft.Win32;
using System.Resources;
using System.Globalization;

using System.Net;
using SimpleDXF;

namespace DoSA
{
    public partial class FormMain : Form
    {
        #region-------------------------- 내부 변수 ----------------------------

        // Treeview 접근 INDEX
		const int FIRST_PARTS_INDEX = 0;
		const int FIRST_ANALYSIS_INDEX = 1;

        const int FEMM_DEFAULT_WIDTH = 600;
        const int FEMM_DEFAULT_HEIGHT = 900;

        private CManageFile m_manageFile = new CManageFile();

        // - Property 창이 중복 호출되는 것을 방지하기 위해 사용한다.
        // - Propety 안의 노드명을 수정할 때 잘못 수정이 발생할 경우 복원용으로도 사용한다.
        private string m_strSelectedNodeName = string.Empty;

        private string m_strBackupNodeName = string.Empty;

        private string m_strCommandLineDesignFullName = string.Empty;
        private string m_strCommandLineDataFullName = string.Empty;

        public CDesign m_design = new CDesign();

        public CScriptFEMM m_femm;

        public ResourceManager m_resManager = null;

        private bool m_bPostMode = false;
        private double m_dGridSize;
        private double m_dVectorScale;

        #endregion

        #region----------------------- 프로그램 초기화 --------------------------

        public FormMain(string strDSAFileFullName = null, string strDataFileFullName = null)
        {
            InitializeComponent();

            try
            {

                #region -------------- CSettingData 설정 -----------------------

                // initializeProgram() 안에서 CSettingData 를 사용하기 때문에 우선적으로 설정한다.
                // 여러곳에서 CSettingData 을 사용하기 때문에 가장 먼저 실시한다.
                CSettingData.m_strProgramDirPath = System.Windows.Forms.Application.StartupPath;

                m_resManager = ResourceManager.CreateFileBasedResourceManager("LanguageResource", Application.StartupPath, null);

                ///------------------------------------------------------------------------
                /// 환경설정전 언어의 초기 설정
                /// 
                /// 첫 설치 때와 같이
                /// 환경설정의 언어 설정값을 읽어드리기 전이나 설정 전에 언어를 사용하는 경우를 대비하여
                /// 환경설정의 언어 설정과 상관없이 무조건 시스템언어를 읽어서 프로그램 언어를 설정해 둔다.
                /// 
                /// 환경설정값으로 언어 설정은 이후에 바로 이어지는 CSettingData.updataLanguge() 에서 이루어진다.
                ///------------------------------------------------------------------------
                CultureInfo ctInfo = Thread.CurrentThread.CurrentCulture;

                /// 한국어가 아니라면 모두 영어로 처리하라.
                if (ctInfo.Name.Contains("ko") == true)
                    CSettingData.m_emLanguage = EMLanguage.Korean;
                else
                    CSettingData.m_emLanguage = EMLanguage.English;

                CSettingData.updataLanguage();

        	    #endregion

                // 실행전에 CSettingData 의 값들이 설정되어야 한다.
                initializeProgram();


                // 환경설정의 기본 작업디렉토리의 해당 프로그램의 디렉토리로 일단 설정한다.
                // 환경설정을 읽어온 후 에 초기화 해야 한다.
                // 주의사항 : initializeProgram() 뒤에 호출 해야 한다.
                CSettingData.m_strCurrentWorkingDirPath = CSettingData.m_strBaseWorkingDirPath;

                // FEMM 에서 지원되는 재질을 Loading 한다.
                loadMaterial();

                m_femm = null;

                /// 파라메터 처리 저장
                /// 
                /// Command Parameter 0 : 일반 실행
                /// Command Parameter 1 : 지정 디자인만 오픈
                /// Command Parameter 2 : 지정 디장인을 열고, 입력데이터 파일로 작업을 함
                /// 
                if (strDSAFileFullName != null)
                {
                    m_strCommandLineDesignFullName = strDSAFileFullName;
                    if(strDataFileFullName != null)
                        m_strCommandLineDataFullName = strDataFileFullName;
                }

                m_dGridSize = 1.0f;
                m_dVectorScale = 10.0f;

            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        //----------- Update Dialog Test --------------
        // - WiFi 를 연결하고, AssemblyInfo 에서 버전을 임의로 낮춘다.
        private void checkDoSAVersion()
        {
            // 버전을 숫자로 변환할 때 DIGIT 의 기본 단위
            const double DIGIT_BASE_NUMBER = 100.0;

            string strNewVersion = string.Empty;
            bool bCheckMainSite = true;

            try
            {
                // 첫번째 버전 확인
                strNewVersion = new WebClient().DownloadString("http://actuator.or.kr/DoSA_2D_Version.txt");
            }
            catch (WebException)
            {
                bCheckMainSite = false;
            }

            try
            {
                // 첫번째 버전 확인이 되지 않을 경우 두번째 버전 확인
                // 두번째 버전 확인에서도 예외가 발생하면 버전 확인을 포기하고 프로그램이 실행된다.
                if (bCheckMainSite == false)
                    strNewVersion = new WebClient().DownloadString("https://solenoid.or.kr/openactuator/dosa_update/DoSA_2D_Version.txt");

                string strAppDataPath = Environment.GetEnvironmentVariable("APPDATA");
                string strSettingFilePath = Path.Combine(strAppDataPath, "DoSA-2D");

                if (m_manageFile.isExistDirectory(strSettingFilePath) == false)
                    m_manageFile.createDirectory(strSettingFilePath);

                string strVersionPassFileFullName = Path.Combine(strSettingFilePath, "VersionPass.txt");

                /// 버전관리 유의사항
                /// 
                /// AssemblyInfo.cs 의 AssemblyVersion 이 아니라 AssemblyFileVersion 이 DoSA 실행파일의 Product Version 이다.
                /// 따라서 DoSA 자동업데이트나 업데이트 요청메시지를 띄우기 위해 버전 확인을 DoSA 실행파일의 버전을 사용하고 있다.
                /// 
                /// About 창에서도 동일한 버전으로 표기하기 위해 AssemblyFileVersion 를 사용하려고 하였으나 
                /// AssemblyFileVersion 는 직접 읽어오지 못해서 여기서도 DoSA 실행파일의 버전을 읽어서 ProductVersion 을 읽어낸다.
                string strEXE_FileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string strProductVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(strEXE_FileName).ProductVersion;


                string[] arrayNewVersion = strNewVersion.Split('.');
                string[] arrayProductVersion = strProductVersion.Split('.');

                int iNewVersion = 0;
                int iProductVersion = 0;

                // 버전에 문제가 있으면 바로 리턴한다.
                if (arrayNewVersion.Length != 4 || arrayProductVersion.Length != 4)
                    return;

                // 3 자리만 사용한다. 마지막 자리는 너무 자주 버전업이 되어서 사용자들에 불편을 준다
                // ex) 0.9.4.2 -> 마지막 2가 버려지고 904 가 된다.
                for (int i = 0; i < 3; i++)
                {
                    iNewVersion += (int)(Convert.ToInt32(arrayNewVersion[i]) * Math.Pow(DIGIT_BASE_NUMBER, (double)(2 - i)));
                    iProductVersion += (int)(Convert.ToInt32(arrayProductVersion[i]) * Math.Pow(DIGIT_BASE_NUMBER, (double)(2 - i)));
                }

                bool bVersionCheckDialog = false;

                CReadFile readFile = new CReadFile();
                CWriteFile writeFile = new CWriteFile();

                string strPassVersion;
                string[] arrayPassVersion;
                int iPassVersion;

                if (iNewVersion > iProductVersion)
                {
                    // 이전에 업그레이드 안함을 선택하여 PassVersion 파일이 있는 경우
                    if (m_manageFile.isExistFile(strVersionPassFileFullName) == true)
                    {
                        strPassVersion = readFile.getLine(strVersionPassFileFullName, 1);

                        arrayPassVersion = strPassVersion.Split('.');

                        // 버전에 문제가 있으면 바로 리턴한다.
                        if (arrayPassVersion.Length != 4)
                            return;

                        iPassVersion = 0;

                        // 업그레이드 확인은 셋째 자리수로 결정된다. (마지막 자리수는 사용되지 않는다.)
                        for (int i = 0; i < 3; i++)
                            iPassVersion += (int)(Convert.ToInt32(arrayPassVersion[i]) * Math.Pow(DIGIT_BASE_NUMBER, (double)(2 - i)));

                        // 저장된 보지 않기를 원하는 버전보다 신규버전이 높을 때만 신규버전 알림창을 띄운다.
                        if (iNewVersion > iPassVersion)
                            bVersionCheckDialog = true;
                        else
                            bVersionCheckDialog = false;
                    }
                    else
                    {
                        bVersionCheckDialog = true;
                    }
                }

                // 신규버전을 알리는 창을 띄운다.
                if (bVersionCheckDialog == true)
                {
                    string strMainUpdateContents = string.Empty;

                    if (bCheckMainSite == true)
                        strMainUpdateContents = new WebClient().DownloadString("https://actuator.or.kr/DoSA_2D_Update_Contents.txt");
                    else
                        strMainUpdateContents = new WebClient().DownloadString("https://solenoid.or.kr/openactuator/dosa_update/DoSA_2D_Update_Contents.txt");

                    PopupNewVersion formNewVersion = new PopupNewVersion(strNewVersion, strProductVersion, strMainUpdateContents);
                    formNewVersion.StartPosition = FormStartPosition.CenterParent;

                    formNewVersion.ShowDialog();

                    // 취소 버튼을 클릭 하는 경우
                    // - 취소를 하면 버전 확인 상관없이 프로그램이 실행 된다.
                    if (formNewVersion.m_iStatus == 3)
                        return;

                    // 다운로드 페이지 버튼을 클릭하는 경우
                    // - 프로그램을 종료 하고 다운로드 웹사이트로 이동한다. 단, 프로그램을 업데이트하지 않으면 다시 알림 창이 뜬다.
                    if (formNewVersion.m_iStatus == 1)
                    {
                        string target;

                        if (CSettingData.m_emLanguage == EMLanguage.Korean)
                        {
                            //target = "https://solenoid.or.kr/direct_kor.php?address=https://solenoid.or.kr/openactuator/dosa_2d_kor.htm";

                            // DoSA 이전 버전의 주소 설정이 아래와 같아서 html 을 삭제하지 않고 그대로 사용하고 있다.
                            target = "https://solenoid.or.kr/index_dosa_open_2d_kor.html";
                        }
                        else
                        {
                            //target = "https://solenoid.or.kr/direct_eng.php?address=https://solenoid.or.kr/openactuator/dosa_2d_eng.htm";

                            //DoSA 이전 버전의 주소 설정이 아래와 같아서 html 을 삭제하지 않고 그대로 사용하고 있다.
                            target = "https://solenoid.or.kr/index_dosa_open_2d_eng.html";
                        }

                        try
                        {
                            System.Diagnostics.Process.Start(target);
                        }
                        catch (System.ComponentModel.Win32Exception noBrowser)
                        {
                            if (noBrowser.ErrorCode == -2147467259)
                                CNotice.printLog(noBrowser.Message);
                        }
                        catch (System.Exception other)
                        {
                            CNotice.printLog(other.Message);
                        }

                        System.Windows.Forms.Application.ExitThread();
                        Environment.Exit(0);
                    }
                    // 해당 업데이트 알림 취소 버튼을 클릭하는 경우
                    // - 해당 업데이트은 더 이상 알림을 띄우지 않는다
                    else if (formNewVersion.m_iStatus == 2)
                    {
                        List<string> listStirng = new List<string>();
                        listStirng.Add(strNewVersion);

                        writeFile.writeLineString(strVersionPassFileFullName, listStirng, true);
                    }
                }
            }
            // 인터넷이 연결되지 않았으면 예외 처리가 되면서 함수를 빠져 나간다.
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }

        }

        /// <summary>
        /// 2023-03-18일 까지 코드를 유지하고 이후는 삭제한다.
        /// 
        /// 2022-03-18 의 DoSA-2D Ver0.9.15.2 에서 
        /// 프로그램 명칭을 DoSA-Open_2D --> DoSA-2D 로 변경함에 의해 발생한
        /// 기존 설치 프로그램과 작업환경 디렉토리를 삭제한다.
        /// </summary>
        private void deleteOldDirectories()
        {
            string strAppDataDirPath = Environment.GetEnvironmentVariable("APPDATA");
            string strSettingDirPath = Path.Combine(strAppDataDirPath, "DoSA-2D");
            string strOldSettingDirPath = Path.Combine(strAppDataDirPath, "DoSA-Open_2D");

            string strParentDirPath = Path.GetDirectoryName(CSettingData.m_strProgramDirPath);
            string strOldInstallDirPath = Path.Combine(strParentDirPath, "DoSA-Open_2D");

            try
            {
                // 기존 작업환경 디렉토리가 있으면 디렉토리을 바꾸어 복사하고 기존 디렉토리는 삭제한다.
                if (m_manageFile.isExistDirectory(strOldSettingDirPath) == true)
                {
                    m_manageFile.copyDirectory(strOldSettingDirPath, strSettingDirPath);
                    m_manageFile.deleteDirectory(strOldSettingDirPath);
                }

                // 기존 설치 디렉토리가 있으면 삭제한다.
                if (m_manageFile.isExistDirectory(strOldInstallDirPath) == true)
                {
                    m_manageFile.deleteDirectory(strOldInstallDirPath);
                }

            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }

        }

        private void initializeProgram()
        {
            try
            {
                string strAppDataDirPath = Environment.GetEnvironmentVariable("APPDATA");
                string strSettingDirPath = Path.Combine(strAppDataDirPath, "DoSA-2D");

                //-----------------------------------------------------------------------------
                // Notice 동작을 위해 우선 실행한다.
                //-----------------------------------------------------------------------------
                // Log 디렉토리가 없으면 생성 한다.
                string strLogDirPath = Path.Combine(strSettingDirPath, "Log");

                if (m_manageFile.isExistDirectory(strLogDirPath) == false)
                    m_manageFile.createDirectory(strLogDirPath);

                //출력방향을 결정함(아래 코드가 동작하면 파일 출력, 동작하지 않으면 Output 창 출력)
                Trace.Listeners.Add(new TextWriterTraceListener(Path.Combine(strLogDirPath, DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + ".Log")));

                // 이벤트 생성 부
                // 
                // 내부함수인 printLogEvent() 의 함수포인트를 사용해서 이벤트 함수를 설정한다
                CNotice.Notice += printLogEvent;


                /// 리소스 파일을 확인하다.
                bool retEnglish, retKorean;
                retEnglish = m_manageFile.isExistFile(Path.Combine(Application.StartupPath, "LanguageResource.en-US.resources"));
                retKorean = m_manageFile.isExistFile(Path.Combine(Application.StartupPath, "LanguageResource.ko-KR.resources"));

                if (retEnglish == false || retKorean == false)
                {
                    MessageBox.Show("There are no Language resource files.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    System.Windows.Forms.Application.ExitThread();
                    Environment.Exit(0);
                }

                int nDoSACount = CManageProcess.getProcessesCount("DoSA-2D");

                if (nDoSACount >= 2)
                {
                    if (CSettingData.m_emLanguage == EMLanguage.Korean)
                        CNotice.noticeWarning("DoSA-2D 의 중복 실행은 허용하지 않습니다.");
                    else
                        CNotice.noticeWarning("Duplicate execution of DoSA-2D is not allowed.");

                    System.Windows.Forms.Application.ExitThread();
                    Environment.Exit(0);
                }

                // 기존에 동작을 하고 있는 FEMM 이 있으면 오류가 발생한다.
                CProgramFEMM.killProcessOfFEMMs();

                //=====================================================================
                // 2023-03-18일 까지 코드를 유지하고 이후는 삭제한다
                //=====================================================================
                deleteOldDirectories();
                //=====================================================================

                /// Net Framework V4.51 이전버전이 설치 되었는지를 확인한다.
                bool retFreamework = checkFramework451();

                if (retFreamework == false)
                {
                    DialogResult result = CNotice.noticeWarningYesNoID("DRIO1", "W");
                    
                    if(result == DialogResult.Yes )
                        openWebsite(@"https://www.microsoft.com/ko-kr/download/details.aspx?id=30653");

                    System.Windows.Forms.Application.ExitThread();
                    Environment.Exit(0);
                }

                if (m_manageFile.isExistDirectory(strSettingDirPath) == false)
                    m_manageFile.createDirectory(strSettingDirPath);

                /// 환경파일 작업
                ///
                string strSettingFileFullName = Path.Combine(strSettingDirPath, "setting.ini");
                
                PopupSetting frmSetting = new PopupSetting();
                frmSetting.StartPosition = FormStartPosition.CenterParent;

                if (false == m_manageFile.isExistFile(strSettingFileFullName))
                {
                    // 첫 실행때 환경 설정파일이 존재하지 않는 경우라도
                    // FromMain() 상단에서 시스템 언어를 확인해서CSettingData.m_emLanguage 을 설정 했기 때문에
                    // 사용자가 선택 전에도 사용 언어는 지정되어 있고 공지글 언어에도 문제가 없다.

                    // 언어 설정 후에 출력해야 한다.
                    CNotice.noticeWarningID("TCFC");

                    // 파일자체가 없기 때문에 다이얼로그의 데이터 설정없이 바로 호출한다.
                    if (DialogResult.OK == frmSetting.ShowDialog())
                    {
                        frmSetting.saveSettingToFile();
                    }
                    else
                    {
                        CNotice.noticeWarningID("IYCT");

                        System.Windows.Forms.Application.ExitThread();
                        Environment.Exit(0);
                    }
                }
                else
                {
                    frmSetting.loadSettingFromFile();

                    if (CSettingData.isSettingDataOK(false) == false)
                    {
                        CNotice.noticeWarningID("TIAP7");

                        if (DialogResult.OK == frmSetting.ShowDialog())
                        {
                            frmSetting.saveSettingToFile();
                        }
                        else
                        {
                            CNotice.noticeWarningID("IYCT");

                            System.Windows.Forms.Application.ExitThread();
                            Environment.Exit(0);
                        }
                    }

                    // WorkingDirectory 을 읽어온 후에 
                    // 작업의 편의를 위해 디렉토리를 WorkingDirectory 로 변경한다.
                    m_manageFile.setCurrentDirectory(CSettingData.m_strBaseWorkingDirPath);
                }

                /// 파일에서 읽어오든 신규파일에서 생성을 하든 Setting 파일안의 프로그램 언어를 설정한다.
                CSettingData.updataLanguage();

                /// FEMM 버전을 확인한다.
                /// 
                int yearOfFEMM = CProgramFEMM.getYearFEMM();

                // 2017 년 이전 버전이면 설치를 유도한다.
                if (yearOfFEMM < 2017)
                {
                    DialogResult result = CNotice.noticeWarningYesNoID("DRIO", "W");

                    if (result == DialogResult.Yes)
                        openWebsite(@"http://www.femm.info/wiki/download");

                    System.Windows.Forms.Application.ExitThread();
                    Environment.Exit(0);
                }
                
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        #endregion

        #region----------------------- Notice Event 호출 함수 ----------------------

        // 이벤트 발생 때 호출되는 함수
        void printLogEvent(EMOutputTarget emTarget, string strMSG)
        {
            if (emTarget == EMOutputTarget.LOG_FILE)
            {
                Trace.WriteLine(DateTime.Now.ToString() + ", " + strMSG);
                Trace.Flush();
            }
            else
            {
                messageListView.addMessage(strMSG);
            }
        }

        #endregion

        #region--------------------- 재질 초기화 ---------------------------

        //-------------------------------------------------------------------------------------------
        // 재질 관련 
        //
        // - DoSA-2D 은 사용자 재질을 지원하지 않고 FEMM 내장 재질만 사용한다.
        //   따라서 FEMM 내장 재질의 이름으로 해석모델 자동생성에서 재질을 설정하기 때문에 DoSA-2D 에서 사용하는 재질명과 FEMM 내장 재질의 이름을 같아야 한다. 
        //   (FEMM Ver 21Apr2019 에서 재질명이 변경되면서 문제가 발생하였음)
        //
        // - DoSA-2D 에서 내장하고 있는 DoSA.dmat 파일은 해석모델을 생성할 때 재질을 생성하는 목적이 아니라
        //   단지, S/W 안에서 BH 곡선을 표시하기 위한 파일이다.
        //   loadMaterial() 안에서 추가되는 연자성체의 재질이 DoSA.dmat 에 존재하지 않는다면 연자성체의 BH 곡선이 표시되지 않는다.
        //
        // - loadMaterial() 에서 S/W 에서 사용가능한 모든 재질의 명칭을 읽어드린다.
        //   항상, FEMM 의 내장 재질명과 명칭을 같게 유지해야 한다.
        //-------------------------------------------------------------------------------------------
        private void loadMaterial()
        {
            List<string> listMaterialName = new List<string>();

            try
            {
                if(CProgramFEMM.getYearFEMM() >= 2019)
                {
                    //------------------------------------------------
                    // 자기회로 Maxwell 내장 연자성 재료
                    //------------------------------------------------
                    // 내장 연자성재료를 추가할 때는 BH 곡선의 내장 연자성재료 설정도 같이 변경해 주어야 한다
                    CMaterialListInFEMM.steelList.Add("Pure Iron");

                    CMaterialListInFEMM.steelList.Add("1006 Steel");
                    CMaterialListInFEMM.steelList.Add("1010 Steel");
                    CMaterialListInFEMM.steelList.Add("1018 Steel");
                    CMaterialListInFEMM.steelList.Add("1020 Steel");
                    CMaterialListInFEMM.steelList.Add("1117 Steel");

                    CMaterialListInFEMM.steelList.Add("416 Stainless Steel");
                    CMaterialListInFEMM.steelList.Add("430 Stainless Steel");
                    CMaterialListInFEMM.steelList.Add("455 Stainless Steel");

                    CMaterialListInFEMM.steelList.Add("M-19 Steel");
                    CMaterialListInFEMM.steelList.Add("M-27 Steel");
                    CMaterialListInFEMM.steelList.Add("M-36 Steel");
                    CMaterialListInFEMM.steelList.Add("M-43 Steel");
                    CMaterialListInFEMM.steelList.Add("M-45 Steel");

                    // 자기회로 Maxwell 내장 비자성 재료
                    // 내장 비자성재료를 추가할 때도 BH 곡선의 내장 비자성재료 설정도 같이 변경해 주어야 한다
                    CMaterialListInFEMM.steelList.Add("Aluminum, 1100");
                    CMaterialListInFEMM.steelList.Add("Copper");
                    CMaterialListInFEMM.steelList.Add("316 Stainless Steel");
                    CMaterialListInFEMM.steelList.Add("304 Stainless Steel");

                    // 해당 Steel 을 사용하지 않은 경우를 대비해 재질 Air 를 인가할 수 있도록 한다.
                    CMaterialListInFEMM.steelList.Add("Air");

                    //------------------------------------------------
                    // 자기회로 내장 영구자석
                    //------------------------------------------------
                    CMaterialListInFEMM.magnetList.Add("N30");
                    CMaterialListInFEMM.magnetList.Add("N33");
                    CMaterialListInFEMM.magnetList.Add("N35");
                    CMaterialListInFEMM.magnetList.Add("N38");
                    CMaterialListInFEMM.magnetList.Add("N40");
                    CMaterialListInFEMM.magnetList.Add("N42");
                    CMaterialListInFEMM.magnetList.Add("N45");
                    CMaterialListInFEMM.magnetList.Add("N48");
                    CMaterialListInFEMM.magnetList.Add("N50");
                    CMaterialListInFEMM.magnetList.Add("N52");
                    CMaterialListInFEMM.magnetList.Add("N55");

                    CMaterialListInFEMM.magnetList.Add("BN1");
                    CMaterialListInFEMM.magnetList.Add("BN2");
                    CMaterialListInFEMM.magnetList.Add("BN3");
                    CMaterialListInFEMM.magnetList.Add("BN4");
                    CMaterialListInFEMM.magnetList.Add("BN5");
                    CMaterialListInFEMM.magnetList.Add("BN6");
                    CMaterialListInFEMM.magnetList.Add("BN7");
                    CMaterialListInFEMM.magnetList.Add("BN8");
                    CMaterialListInFEMM.magnetList.Add("BN9");
                    CMaterialListInFEMM.magnetList.Add("BN10");

                    CMaterialListInFEMM.magnetList.Add("SmCo24");
                    CMaterialListInFEMM.magnetList.Add("SmCo26");
                    CMaterialListInFEMM.magnetList.Add("SmCo28");
                    CMaterialListInFEMM.magnetList.Add("SmCo30");
                    CMaterialListInFEMM.magnetList.Add("SmCo32");

                    CMaterialListInFEMM.magnetList.Add("Cast Alnico 2 (LNG12))");
                    CMaterialListInFEMM.magnetList.Add("Cast Alnico 3 (LNG10)");
                    CMaterialListInFEMM.magnetList.Add("Cast Alnico 5 (LNG40)");
                    CMaterialListInFEMM.magnetList.Add("Cast Alnico 6 (LNGT28)");
                    CMaterialListInFEMM.magnetList.Add("Cast Alnico 8 (LNGT38)");
                    CMaterialListInFEMM.magnetList.Add("Cast Alnico 9 (LNGT72)");
                    
                    CMaterialListInFEMM.magnetList.Add("Sintered Alnico 2 (FLNG12)");
                    CMaterialListInFEMM.magnetList.Add("Sintered Alnico 5 (FLNG34)");
                    CMaterialListInFEMM.magnetList.Add("Sintered Alnico 8 (FLNGT38)");

                    CMaterialListInFEMM.magnetList.Add("Ceramic 1");
                    CMaterialListInFEMM.magnetList.Add("Ceramic 5");
                    CMaterialListInFEMM.magnetList.Add("Ceramic 8");

                    // 해당 Steel 을 사용하지 않은 경우를 대비해 재질 Air 를 인가할 수 있도록 한다.
                    CMaterialListInFEMM.magnetList.Add("Air");

                    //------------------------------------------------
                    // 코일 동선 재료
                    //------------------------------------------------
                    CMaterialListInFEMM.coilWireList.Add("Aluminum");
                    CMaterialListInFEMM.coilWireList.Add("Copper");
                }
                else
                {
                    //------------------------------------------------
                    // 자기회로 Maxwell 내장 연자성 재료
                    //------------------------------------------------
                    // 내장 연자성재료를 추가할 때는 BH 곡선의 내장 연자성재료 설정도 같이 변경해 주어야 한다
                    CMaterialListInFEMM.steelList.Add("Pure Iron");
                
                    CMaterialListInFEMM.steelList.Add("1006 Steel");
                    CMaterialListInFEMM.steelList.Add("1010 Steel");
                    CMaterialListInFEMM.steelList.Add("1018 Steel");
                    CMaterialListInFEMM.steelList.Add("1020 Steel");
                    CMaterialListInFEMM.steelList.Add("1117 Steel");

                    CMaterialListInFEMM.steelList.Add("416 Stainless Steel");
                    CMaterialListInFEMM.steelList.Add("430 Stainless Steel");
                    CMaterialListInFEMM.steelList.Add("455 Stainless Steel");

                    // 자기회로 Maxwell 내장 비자성 재료
                    // 내장 비자성재료를 추가할 때도 BH 곡선의 내장 비자성재료 설정도 같이 변경해 주어야 한다
                    CMaterialListInFEMM.steelList.Add("Aluminum, 1100");
                    CMaterialListInFEMM.steelList.Add("Copper");
                    CMaterialListInFEMM.steelList.Add("316 Stainless Steel");
                    CMaterialListInFEMM.steelList.Add("304 Stainless Steel");

                    // 해당 Steel 을 사용하지 않은 경우를 대비해 재질 Air 를 인가할 수 있도록 한다.
                    CMaterialListInFEMM.steelList.Add("Air");

                    //------------------------------------------------
                    // 자기회로 내장 영구자석
                    //------------------------------------------------
                    CMaterialListInFEMM.magnetList.Add("NdFeB 32 MGOe");
                    CMaterialListInFEMM.magnetList.Add("NdFeB 37 MGOe");
                    CMaterialListInFEMM.magnetList.Add("NdFeB 40 MGOe");
                    CMaterialListInFEMM.magnetList.Add("NdFeB 52 MGOe");
                    CMaterialListInFEMM.magnetList.Add("NdFeB 10 MGOe (Bonded)");
                
                    CMaterialListInFEMM.magnetList.Add("SmCo 20 MGOe");
                    CMaterialListInFEMM.magnetList.Add("SmCo 24 MGOe");
                    CMaterialListInFEMM.magnetList.Add("SmCo 27 MGOe");                
                
                    CMaterialListInFEMM.magnetList.Add("Alnico 5");
                    CMaterialListInFEMM.magnetList.Add("Alnico 6");
                    CMaterialListInFEMM.magnetList.Add("Alnico 8");

                    CMaterialListInFEMM.magnetList.Add("Ceramic 5");
                    CMaterialListInFEMM.magnetList.Add("Ceramic 8");

                    // 해당 Steel 을 사용하지 않은 경우를 대비해 재질 Air 를 인가할 수 있도록 한다.
                    CMaterialListInFEMM.magnetList.Add("Air");

                    //------------------------------------------------
                    // 코일 동선 재료
                    //------------------------------------------------
                    CMaterialListInFEMM.coilWireList.Add("Aluminum");
                    CMaterialListInFEMM.coilWireList.Add("Copper");
                }

            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }

        }
        #endregion

        #region------------------------- 전체 초기화 ------------------------
        //전체 초기화 한다
        private void closeDesign()
        {
            try
            {
                // 데이터가 있는 경우만 Close 메시지를 알린다.
                if (m_design.GetNodeList.Count != 0)
                    CNotice.printUserMessage(m_design.m_strDesignName + m_resManager.GetString("_DHBC"));

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
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        #endregion

        
        #region--------------------- Ribbon Menu ---------------------------

        private void ribbonButtonNew_Click(object sender, EventArgs e)
        {
            try
            {
                if (m_design.m_bChanged == true)
                {
                    if (DialogResult.Yes == CNotice.noticeWarningYesNoID("DYWT", "W"))
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
                    CNotice.printLogID("DNIN");
                    return;
                }

                // 생성을 할 때는 기본 작업 디렉토리를 사용해서 Actuator 작업파일의 절대 경로를 지정하고,
                // 작업파일을 Open 할 때는 파일을 오픈하는 위치에서 작업 디렉토리를 얻어내어 다시 설정한다.
                // 왜냐하면, 만약 작업 디렉토리를 수정하는 경우 기존의 작업파일을 열 수 없기 때문이다.
                string strDesignDirName = Path.Combine(CSettingData.m_strCurrentWorkingDirPath, strDesignName);

                m_design.m_strDesignDirPath = strDesignDirName;
                m_design.m_strDesignName = strDesignName;

                // 프로젝트가 시작 했음을 표시하기 위해서 TreeView 에 기본 가지를 추가한다.
                TreeNode treeNode = new TreeNode("Parts", (int)EMKind.PARTS, (int)EMKind.PARTS);
                treeViewMain.Nodes.Add(treeNode);

                treeNode = new TreeNode("Tests", (int)EMKind.TESTS, (int)EMKind.TESTS);
                treeViewMain.Nodes.Add(treeNode);

                // 수정 되었음을 기록한다.
                m_design.m_bChanged = true;

                /// 새로운 FEMM 을 연다
                openFEMM();

                // 제목줄에 디자인명을 표시한다
                this.Text = "DoSA-2D - " + m_design.m_strDesignName;

                CNotice.printUserMessage(m_design.m_strDesignName + m_resManager.GetString("_DHBC1"));
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        private void ribbonButtonOpen_Click(object sender, EventArgs e)
        {

            try
            {
                if (m_design.m_bChanged == true)
                {
                    if (DialogResult.Yes == CNotice.noticeWarningYesNoID("DYWT", "W"))
                    {
                        saveDesignFile();
                    }
                }

                OpenFileDialog openFileDialog = new OpenFileDialog();

                // 파일 열기창 설정
                openFileDialog.Title = "Open a DoSA-2D File";
                // 디자인 파일을 열 때 디렉토리는 프로그램 작업 디렉토리로 하고 있다.
                openFileDialog.InitialDirectory = CSettingData.m_strCurrentWorkingDirPath;
                openFileDialog.FileName = null;
                openFileDialog.Filter = "DoSA-2D File (*.dsa)|*.dsa|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                DialogResult result = openFileDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string strDesignFileFullName = openFileDialog.FileName;

                    if (false == checkDesignFile(ref strDesignFileFullName))
                        return;

                    // 기존 디자인 데이터를 모두 삭제한다.
                    closeDesign();

                    if (false == loadDesignFile(strDesignFileFullName))
                        return;

                    // 디자인 파일이 생성될 때의 디자인 작업 디렉토리는 프로그램 기본 디렉토리 강제 설정하고 있다.
                    // 만약 디렉토리를 옮긴 디자인 디렉토리를 오픈 할 경우라면 
                    // 이전 다지인 작업 디렉토리를 그대로 사용하면 디렉토리 문제가 발생하여 실행이 불가능하게 된다.
                    // 이를 해결하기 위해
                    // 작업파일을 Open 할 때는 파일을 오픈하는 위치로 작업파일의 디렉토리를 다시 설정하고 있다.
                    m_design.m_strDesignDirPath = Path.GetDirectoryName(strDesignFileFullName);

                    // Design 디렉토리에서 Design 명을 제거한 디렉토리를 작업디렉토리로 설정한다.
                    CSettingData.m_strCurrentWorkingDirPath = Path.GetDirectoryName(m_design.m_strDesignDirPath);

                    // 프로젝트가 시작 했음을 표시하기 위해서 TreeView 에 기본 가지를 추가한다.
                    TreeNode treeNode = new TreeNode("Parts", (int)EMKind.PARTS, (int)EMKind.PARTS);
                    treeViewMain.Nodes.Add(treeNode);

                    treeNode = new TreeNode("Tests", (int)EMKind.TESTS, (int)EMKind.TESTS);
                    treeViewMain.Nodes.Add(treeNode);

                    foreach (CDataNode node in m_design.GetNodeList)
                        this.addTreeNode(node.NodeName, node.KindKey);
                }
                else
                {
                    return;
                }

                openFEMM();

                // 제목줄에 디자인명을 표시한다
                this.Text = "DoSA-2D - " + m_design.m_strDesignName;

                CNotice.printUserMessage(m_design.m_strDesignName + m_resManager.GetString("_DHBO"));
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        private bool checkDesignFile(ref string strDesignFileFullName)
        {
            DialogResult result;

            string strDesignName = Path.GetFileNameWithoutExtension(strDesignFileFullName);
            string strDesignDirectory = Path.GetDirectoryName(strDesignFileFullName);

            string[] arrayString = strDesignDirectory.Split(Path.DirectorySeparatorChar);

            try
            {
                // 디자인명과 디자인파일이 포함된 디렉토리명이 일치하는지 확인한다.
                if (strDesignName != arrayString[arrayString.Length - 1])
                {
                    if (CSettingData.m_emLanguage == EMLanguage.Korean)
                        result = CNotice.noticeWarningYesNo("DoSA-2D 파일의 디렉토리 구조에 문제가 있습니다.\n디렉토리 구조를 자동 생성 하겠습니까?");
                    else
                        result = CNotice.noticeWarningYesNo("There is a problem with the directory structure of the DoSA-2D file.\nDo you want to automatically create the directory structure?");

                    if (result == DialogResult.Yes)
                    {
                        string strNewDesignFileFullName = Path.Combine(strDesignDirectory, strDesignName, strDesignName + ".dsa");

                        if (true == m_manageFile.isExistDirectory(Path.Combine(strDesignDirectory, strDesignName)))
                        {
                            if (CSettingData.m_emLanguage == EMLanguage.Korean)
                                CNotice.noticeWarning("디자인 명의 디렉토리가 이미 존재합니다.");
                            else
                                CNotice.noticeWarning("A directory named design already exists.");

                            return false;
                        }

                        if (false == m_manageFile.createDirectory(Path.Combine(strDesignDirectory, strDesignName)))
                            return false;

                        m_manageFile.copyFile(strDesignFileFullName, strNewDesignFileFullName);
                        Thread.Sleep(10);
                        m_manageFile.deleteFile(strDesignFileFullName);

                        // 수정된 디렉토리로 Design 파일의 풀 패스를 변경한다.
                        strDesignFileFullName = strNewDesignFileFullName;
                    }
                    else
                    {
                        // 디렉토리 생성을 강제하지 않는다.
                        //return false;
                    }
                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return false;
            }

            return true;
        }

        private void ribbonOrbMenuItemClose_Click(object sender, EventArgs e)
        {
            try
            {
                if (m_design.m_bChanged == true)
                {
                    if (DialogResult.Yes == CNotice.noticeWarningYesNoID("DYWT1", "W"))
                    {
                        saveDesignFile();
                    }
                }

                // 저장을 하고 나면 초기화 한다.
                m_design.m_bChanged = false;

                // 기존 디자인 데이터를 모두 삭제한다.
                closeDesign();

                // 제목줄에 디자인명을 삭제한다
                this.Text = "DoSA-2D";

                quitFEMM();
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        private void ribbonButtonSave_Click(object sender, EventArgs e)
        {
            if (m_design.m_strDesignName.Length == 0)
            {
                CNotice.noticeWarningID("TIND2");
                return;
            }

            if (true == saveDesignFile())
            {
                CNotice.noticeInfomation(m_design.m_strDesignName + m_resManager.GetString("_DSHB1"), m_resManager.GetString("SN"));

                CNotice.printUserMessage(m_design.m_strDesignName + m_resManager.GetString("_DSHB"));
            }
        }

        private void ribbonButtonSaveAs_Click(object sender, EventArgs e)
        {
            string strOrgDesignDirName, strSaveAsDesignDirName;
            string strSaveAsDesignName;

            CWriteFile writeFile = new CWriteFile();

            string strOrgDesignName = this.m_design.m_strDesignName;
            strOrgDesignDirName = this.m_design.m_strDesignDirPath;

            // 디자인이 없는 경우는 DesignName 없기 때문에 이름으로 작업디자인이 있는지를 판단한다.
            if (strOrgDesignName.Length == 0)
            {
                CNotice.noticeWarningID("TIND1");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Title = "Write a New Design Name";
            saveFileDialog.InitialDirectory = CSettingData.m_strCurrentWorkingDirPath;
            saveFileDialog.FileName = strOrgDesignName + "_Modify";

            DialogResult result = saveFileDialog.ShowDialog();

            try
            {

                if (result == DialogResult.OK)
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
                            CNotice.noticeWarningID("YHCA");
                            return;
                        }
                    }

                    string strOrgDesingFileFullName;

                    if (true == m_manageFile.isExistFile(Path.Combine(strOrgDesignDirName, strOrgDesignName + ".dsa")))
                        strOrgDesingFileFullName = Path.Combine(strOrgDesignDirName, strOrgDesignName + ".dsa");
                    else
                    {
                        if (CSettingData.m_emLanguage == EMLanguage.Korean)
                            CNotice.noticeWarning("원본 DoSA-2D 디자인 파일이 존재하지 않습니다.");
                        else
                            CNotice.noticeWarning("The original DoSA-2D design file does not exist.");

                        return;
                    }

                    string strSaveAsDesignFileFullName = Path.Combine(strSaveAsDesignDirName, strSaveAsDesignName + ".dsa");


                    #region // --------------- 파일과 디렉토리 복사 ---------------------

                    // SaveAs 디자인 디렉토리 생성
                    m_manageFile.createDirectory(strSaveAsDesignDirName);

                    // 디자인 파일 복사
                    m_manageFile.copyFile(strOrgDesingFileFullName, strSaveAsDesignFileFullName);
                    
                    #endregion                    


                    // 현 모델을 SaveAs 모델명으로 변경한다.
                    m_design.m_strDesignDirPath = strSaveAsDesignDirName;
                    m_design.m_strDesignName = strSaveAsDesignName;

                    // 수정모델을 읽어드린 후에 바로 저장한다.
                    saveDesignFile();

                    // 화면을 갱신한다.
                    splitContainerRight.Panel1.Controls.Clear();
                    splitContainerRight.Panel1.Controls.Add(this.panelEmpty);

                    // PropertyGrid 창을 초기화 한다.
                    propertyGridMain.SelectedObject = null;

                    // 제목줄에 디자인명을 변경한다
                    this.Text = "DoSA-2D - " + m_design.m_strDesignName;

                    CNotice.noticeInfomation(m_design.m_strDesignName + m_resManager.GetString("_DHBS1"), m_resManager.GetString("SAN"));

                    CNotice.printUserMessage(m_design.m_strDesignName + m_resManager.GetString("_DHBS"));
                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        private void ribbonOrbMenuItemExit_Click(object sender, EventArgs e)
        {
            if (m_design.m_bChanged == true)
            {
                if (DialogResult.Yes == CNotice.noticeWarningYesNoID("DYWT", "W"))
                {
                    saveDesignFile();
                }
            }

            System.Windows.Forms.Application.Exit();
        }

        private void ribbonButtonCoil_Click(object sender, EventArgs e)
        {
            addDataNode(EMKind.COIL);
        }

        private void ribbonButtonMagnet_Click(object sender, EventArgs e)
        {
            addDataNode(EMKind.MAGNET);
        }
        
        private void ribbonButtonSteel_Click(object sender, EventArgs e)
        {
            addDataNode(EMKind.STEEL);
        }

        private void ribbonButtonForce_Click(object sender, EventArgs e)
        {
            addDataNode(EMKind.FORCE_TEST);
        }

        private void ribbonButtonStroke_Click(object sender, EventArgs e)
        {
            addDataNode(EMKind.STROKE_TEST);
        }

        private void ribbonButtonCurrent_Click(object sender, EventArgs e)
        {
            addDataNode(EMKind.CURRENT_TEST);
        }
        
        private void ribbonButtonSetting_Click(object sender, EventArgs e)
        {
            PopupSetting frmSetting = new PopupSetting();

            frmSetting.uploadSettingData();

            if (DialogResult.OK == frmSetting.ShowDialog())
            {
                frmSetting.saveSettingToFile();

                // 언어를 수정과 동시에 반영한다.
                CSettingData.updataLanguage();
            }
        }

        private void ribbonButtonHelp_Click(object sender, EventArgs e)
        {
            PopupHelp frmHelp = new PopupHelp();
            frmHelp.StartPosition = FormStartPosition.CenterParent;

            frmHelp.ShowDialog();
        }


        private void ribbonButtonAbout_Click(object sender, EventArgs e)
        {
            PopupAboutBox frmAbout = new PopupAboutBox();

            frmAbout.ShowDialog();
        }

        private void ribbonButtonDonation_Click(object sender, EventArgs e)
        {
            string target;

            if (CSettingData.m_emLanguage == EMLanguage.Korean)
            {
                target = "https://solenoid.or.kr/index_donation.html";
            }
            else
            {
                target = "https://www.buymeacoffee.com/openactuator";
            }

            try
            {
                System.Diagnostics.Process.Start(target);
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    CNotice.printLog(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                CNotice.printLog(other.Message);
            }
        }

        private void ribbonButtonImportDXF_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // 파일 열기창 설정
            openFileDialog.Title = "Import a DXF File";
            // 디자인 파일을 열 때 디렉토리는 프로그램 작업 디렉토리로 하고 있다.
            openFileDialog.InitialDirectory = CSettingData.m_strCurrentWorkingDirPath;
            openFileDialog.FileName = null;
            openFileDialog.Filter = "DXF 2D File (*.dxf)|*.dxf|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                string strDXFFileFullName = openFileDialog.FileName;

                importDXF(strDXFFileFullName);
            }
        }

        private void importDXF(string strDXFFileName)
        {
            //Loads a file from the command line argument
            Document simpleDXF = new Document(strDXFFileName);
            List<Polyline> listPolylines;
            int nCountParts = 0;

            simpleDXF.Read();

            listPolylines = simpleDXF.Polylines;

            try
            {
                foreach (Polyline polyLine in listPolylines)
                {
                    // 이름 인덱스를 증가시킨다.
                    nCountParts++;

                    CFace face = new CFace();

                    face.BasePoint.X = 0.0;
                    face.BasePoint.Y = 0.0;

                    if (polyLine.Vertexes.Count < 4)
                    {
                        CNotice.printLog("3개 이하는 Point 로 구성된 Face 는 제외 됩니다.");
                        continue;
                    }

                    List<CPoint> listPoint = new List<CPoint>();

                    bool bCheckPoint = true;

                    // 마지막점은 시작점과 동일점이기 때문에 -1 을 추가하여 제외시킨다.
                    for (int i = 0; i < polyLine.Vertexes.Count - 1; i++)
                    {
                        // 매번 신규로 생성을 해야 한다.
                        CPoint point = new CPoint();

                        point.X = polyLine.Vertexes[i].Position.X;
                        point.Y = polyLine.Vertexes[i].Position.Y;

                        // 자리수 오차에서 발생하는 0 < X < -0.1 um 사이의 값은 영으로 처리한다.
                        if (CSmallUtil.isZeroPosition(point.X))
                        {
                            point.X = 0;
                        }

                        // 축대칭해석만 지원하고 있기 때문에 
                        // 거리에 대한 영처리가 된 이 후에 point.X 가 음의 값이면 해당 List 를 Node 로 생성하지 않는다.
                        if (point.X < 0)
                        {
                            bCheckPoint = false;
                        }

                        // 무조건 Straight 만 지원 한다.
                        point.LineKind = EMLineKind.STRAIGHT;

                        listPoint.Add(point);
                    }

                    // 음의 X 값이 있거나 point 의 수가 4보다 작으면 Node 생성작업을 취소한다.
                    if (bCheckPoint == false || listPoint.Count < 4)
                        continue;

                    // DXF 에서 읽어드리는 객체는 무조건 PLOYGON 으로 지정한다.
                    face.FaceType = EMFaceType.POLYGON;
                    face.setPolygonPoints(listPoint);

                    // 형상노드를 생성하고 추가한다.
                    CNonKind nonKind = new CNonKind();

                    bool bCheckExist = false;
                    string strNodeName;

                    // 기존에 이름이 존재하면 nCount 를 증가시키면서 계속 시도한다.
                    do
                    {
                        strNodeName = "DXF_Shape_" + nCountParts.ToString();

                        bCheckExist = m_design.isExistNode(strNodeName);

                        if (bCheckExist == true) nCountParts++;
                    }
                    while (bCheckExist == true);

                    // 노드 값을 설정한다.
                    nonKind.NodeName = strNodeName;
                    nonKind.KindKey = EMKind.NON_KIND;
                    nonKind.Face = face;
                    nonKind.MovingPart = EMMoving.FIXED;

                    bool bRet = m_design.addDataNode(nonKind);

                    if (bRet == true)
                    {
                        // Treeview 에 추가한다
                        addTreeNode(nonKind.NodeName, nonKind.KindKey);

                        // 해당 Node 의 Properies View 와 Information Windows 를 표시한다
                        showDataNode(nonKind.NodeName);

                        propertyGridMain.Refresh();
                    }
                    else
                    {
                        CNotice.noticeWarningID("TNIA");
                    }
                }

                // 혹시 FEMM 의 화면이 닫힌 경우 FEMM 의 화면을 복원합니다.
                reopenFEMM();

                m_design.drawDesign(m_femm);

                // 수정 되었음을 기록한다.
                m_design.m_bChanged = true;
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        #endregion

        #region----------------------- Button -------------------------------

        private void buttonTestCurrent_Click(object sender, EventArgs e)
        {
            CCurrentTest currentTest = (CCurrentTest)propertyGridMain.SelectedObject;

            if (currentTest == null) return;

            // 현재 표시되고 있는 PropertyGird 창에서 Test 이름을 찾아 낸다
            string strTestName = currentTest.NodeName;
            string strTestDirName = Path.Combine(m_design.m_strDesignDirPath, strTestName);

            string strTestFullName = Path.Combine(strTestDirName, strTestName + ".fem");
            string strStrokeFileFullName = Path.Combine(strTestDirName, strTestName + ".csv");


            try
            {
                if (false == isCurrentTestOK(currentTest))
                    return;

                if (m_manageFile.isExistDirectory(strTestDirName) == true)
                {
                    DialogResult ret = CNotice.noticeWarningYesNoID("TIAP", "NE");

                    if (ret == DialogResult.No)
                        return;

                    m_manageFile.deleteDirectory(strTestDirName);

                    // 삭제되는 시간이 필요한 듯 한다.
                    Thread.Sleep(1000);
                }

                // 사용자 메시지를 초기화 한다.
                messageListView.clearMessage();

                if (CSettingData.m_emLanguage == EMLanguage.Korean)
                    CNotice.printUserMessage(currentTest.NodeName + " 전류별 자기력 실험을 시작합니다.");
                else
                    CNotice.printUserMessage(currentTest.NodeName + " magnetic force test by current is started.");

                // 시험 디렉토리를 생성한다.
                m_manageFile.createDirectory(strTestDirName);

                // 해석전 현 설정을 저장한다.
                saveDesignFile();

                // 혹시 FEMM 의 화면이 닫힌 경우 FEMM 의 화면을 복원합니다.
                reopenFEMM();

                m_design.addMaterials(m_femm);

                m_design.drawDesign(m_femm);

                /// 전류, 전압을 영으로 설정해서 기본모델을 만든다.
                m_design.setBlockPropeties(m_femm, 0, currentTest.MeshSizePercent);

                double minX, maxX, minY, maxY;
                minX = maxX = minY = maxY = 0;

                m_design.getModelMinMaxX(ref minX, ref maxX);
                // 이동한 상태에서 해석이 진행됨으로 이동량을 Plus 와 Minus 에 모두 사용한다.
                m_design.getModelMinMaxY(ref minY, ref maxY, currentTest.MovingStroke);

                m_design.setBoundary(m_femm, currentTest.MeshSizePercent, minX, maxX, minY, maxY);

                // FEMM 기본모델은 구동부 이동전으로 저장하고 해석전에 구동부를 이동해서 해석한다.
                m_femm.moveMovingParts(currentTest.MovingStroke);

                m_femm.saveAs(strTestFullName);

                double dInitialCurrent = currentTest.InitialCurrent;
                double dFinalCurrent = currentTest.FinalCurrent;
                int nStepCount = currentTest.StepCount;

                double dStepIncrease = Math.Abs(dFinalCurrent - dInitialCurrent) / nStepCount;
                double dCurrent;

                /// Progress Bar 설정
                progressBarCurrent.Style = ProgressBarStyle.Blocks;
                progressBarCurrent.Minimum = 0;
                progressBarCurrent.Maximum = nStepCount;
                progressBarCurrent.Step = 1;
                progressBarCurrent.Value = 0;

                List<string> listString = new List<string>();

                DateTime previousTime = new DateTime();
                previousTime = DateTime.Now;

                getPostRegion(ref minX, ref maxX, ref minY, ref maxY, 1.2f);

                string strMessage = string.Empty;

                /// 총 계산횟수는 Step + 1 회이다.
                for (int i = 0; i < nStepCount + 1; i++)
                {
                    dCurrent = dInitialCurrent + dStepIncrease * i;

                    m_design.changeCurrent(m_femm, dCurrent, currentTest.MeshSizePercent);

                    progressBarCurrent.PerformStep();
                    labelProgressCurrent.Text = "Current Step : " + i.ToString() + " / " + nStepCount.ToString();

                    // 해석 중에 전체 해석영역이 표시되는 것을 막기 위해 제품영역 정보를 송부한다. 
                    double dForce = m_femm.solveForce(minX, maxX, minY, maxY);

                    string strCurrent = String.Format("{0}", dCurrent);
                    string strForce = String.Format("{0}", dForce);

                    listString.Add(strCurrent + "," + strForce);

                    strMessage = string.Format("Count = {0}, Stroke = {1}, Current = {2}, Force = {3}", i + 1, currentTest.MovingStroke, dCurrent, dForce);
                    CNotice.printUserMessage(strMessage);
                }

                // Force 계산이 진행되면 후처리로 변화하기 때문에 후처리 모드를 저장한다.
                // 추후 Tree 를 선택할 때 전처리로 모드를 전환한다.
                m_bPostMode = true;

                DateTime currentTime = new DateTime();
                currentTime = DateTime.Now;

                TimeSpan diffTime = currentTime - previousTime;

                //closePostView();

                CWriteFile writefile = new CWriteFile();

                writefile.writeLineString(strStrokeFileFullName, listString, true);

                plotCurrentResult();

                // Result 버튼이 동작하게 한다.
                buttonLoadCurrentResult.Enabled = true;

                if (diffTime.Hours > 0)
                    CNotice.printUserMessage(strTestName + m_resManager.GetString("_THBC") +
                                                diffTime.Hours.ToString() + m_resManager.GetString("H") +
                                                diffTime.Minutes.ToString() + m_resManager.GetString("M") +
                                                diffTime.Seconds.ToString() + m_resManager.GetString("S"));
                else
                    CNotice.printUserMessage(strTestName + m_resManager.GetString("_THBC") +
                                                diffTime.Minutes.ToString() + m_resManager.GetString("M") +
                                                diffTime.Seconds.ToString() + m_resManager.GetString("S"));

                /// DoSA 를 활성화하여 창을 최상위에 위치시킨다.
                this.Activate();
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        private void buttonCurrentResult_Click(object sender, EventArgs e)
        {
            plotCurrentResult();
        }

        private void buttonMagnetUp_Click(object sender, EventArgs e)
        {
            CDataNode node = (CDataNode)propertyGridMain.SelectedObject;

            if (node == null) return;

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
            CDataNode node = (CDataNode)propertyGridMain.SelectedObject;

            if (node == null) return;

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
            CDataNode node = (CDataNode)propertyGridMain.SelectedObject;

            if (node == null) return;

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
            CDataNode node = (CDataNode)propertyGridMain.SelectedObject;

            if (node == null) return;

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
            CShapeParts nodeParts = (CShapeParts)propertyGridMain.SelectedObject;

            if (nodeParts == null) return;

            changePartsShapeInPopup(nodeParts);
        }
    

        private void buttonForceAndMagnitudeB_Result_Click(object sender, EventArgs e)
        {
            try
            {
                CForceTest forceTest = (CForceTest)propertyGridMain.SelectedObject;

                if (forceTest == null) return;

                double minX, maxX, minY, maxY;
                minX = maxX = minY = maxY = 0;

                m_design.getModelMinMaxX(ref minX, ref maxX);
                // 이동한 상태에서 해석이 진행됨으로 이동량을 Plus 와 Minus 에 모두 사용한다.
                m_design.getModelMinMaxY(ref minY, ref maxY, forceTest.MovingStroke);

                getPostRegion(ref minX, ref maxX, ref minY, ref maxY, 1.2f);

                // parameter = ture : Magnitude B
                plotForceDensityResult(minX, maxX, minY, maxY, false, true);
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        private void buttonForceAndVectorB_Result_Click(object sender, EventArgs e)
        {
            try
            {
                CForceTest forceTest = (CForceTest)propertyGridMain.SelectedObject;

                if (forceTest == null) return;

                double minX, maxX, minY, maxY;
                minX = maxX = minY = maxY = 0;

                m_design.getModelMinMaxX(ref minX, ref maxX);
                // 이동한 상태에서 해석이 진행됨으로 이동량을 Plus 와 Minus 에 모두 사용한다.
                m_design.getModelMinMaxY(ref minY, ref maxY, forceTest.MovingStroke);

                getPostRegion(ref minX, ref maxX, ref minY, ref maxY, 1.2f);

                // parameter = false : Vector B
                plotForceDensityResult(minX, maxX, minY, maxY, false, false);
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        private void buttonTestForce_Click(object sender, EventArgs e)
        {
            try
            {
                CForceTest forceTest = (CForceTest)propertyGridMain.SelectedObject;

                if (forceTest == null) return;

                // 현재 시험의 이름을 m_nodeList 에서 찾지 않고
                // 현재 표시되고 있는 PropertyGird 창에서 Test 이름을 찾아 낸다
                string strTestName = forceTest.NodeName;
                string strTestDirName = Path.Combine(m_design.m_strDesignDirPath, strTestName);

                string strTestFullName = Path.Combine(strTestDirName, strTestName + ".fem");
                string strForceFileFullName = Path.Combine(strTestDirName, strTestName + ".txt");

                if (false == isForceTestOK(forceTest))
                    return;

                if (m_manageFile.isExistDirectory(strTestDirName) == true)
                {
                    DialogResult ret = CNotice.noticeWarningYesNoID("TIAP", "NE");

                    if (ret == DialogResult.No)
                        return;

                    m_manageFile.deleteDirectory(strTestDirName);

                    // 삭제되는 시간이 필요한 듯 한다.
                    Thread.Sleep(1000);
                }

                // 사용자 메시지를 초기화 한다.
                messageListView.clearMessage();

                if (CSettingData.m_emLanguage == EMLanguage.Korean)
                    CNotice.printUserMessage(forceTest.NodeName + " 자기력 실험을 시작합니다.");
                else
                    CNotice.printUserMessage(forceTest.NodeName + " magnetic force test is started.");

                // 시험 디렉토리를 생성한다.
                m_manageFile.createDirectory(strTestDirName);

                // 해석전 현 설정을 저장한다.
                saveDesignFile();

                // 혹시 FEMM 의 화면이 닫힌 경우 FEMM 의 화면을 복원합니다.
                reopenFEMM();

                // Post Mode 인 경우 m_femm.solveForce() 에서 여러번 화면이 갱신되기 때문에 이전 결과를 닫는다.
                if (m_bPostMode == true)
                {
                    // 순서 주의 할 것
                    closePostView();

                    resizePrograms();

                    m_bPostMode = false;

                    // 초기이미지가 없어서 이미지를 비우고 있다.
                    loadDefaultImage(EMKind.FORCE_TEST);
                }

                m_design.addMaterials(m_femm);

                m_design.drawDesign(m_femm);

                m_design.setBlockPropeties(m_femm, forceTest.Voltage, forceTest.MeshSizePercent);

                double minX, maxX, minY, maxY;
                minX = maxX = minY = maxY = 0;

                m_design.getModelMinMaxX(ref minX, ref maxX);
                // 이동한 상태에서 해석이 진행됨으로 이동량을 Plus 와 Minus 에 모두 사용한다.
                m_design.getModelMinMaxY(ref minY, ref maxY, forceTest.MovingStroke);

                m_design.setBoundary(m_femm, forceTest.MeshSizePercent, minX, maxX, minY, maxY);

                /// 저장 전에 이동량을 반영한다.
                m_femm.moveMovingParts(forceTest.MovingStroke);

                m_femm.saveAs(strTestFullName);

                DateTime previousTime = new DateTime();
                previousTime = DateTime.Now;

                double dlongerLength = getPostRegion(ref minX, ref maxX, ref minY, ref maxY, 1.2f);

                // 해석 중에 전체 해석영역이 표시되는 것을 막기 위해 제품영역 정보를 송부한다. 
                double dForce = m_femm.solveForce(minX, maxX, minY, maxY);

                // Force 계산이 진행되면 후처리로 변화하기 때문에 후처리 모드를 저장한다.
                // 추후 Tree 를 선택할 때 전처리로 모드를 전환한다.
                m_bPostMode = true;

                DateTime currentTime = new DateTime();
                currentTime = DateTime.Now;

                TimeSpan diffTime = currentTime - previousTime;

                string strForce = String.Format("{0,15:N5}", dForce);

                CWriteFile writefile = new CWriteFile();

                List<string> listString = new List<string>();

                listString.Add("force:" + strForce);

                string strMessage = string.Format("Voltage = {0}, Stroke = {1}, Force = {2}", forceTest.Voltage, forceTest.MovingStroke, dForce);
                CNotice.printUserMessage(strMessage);

                writefile.writeLineString(strForceFileFullName, listString, true);


                string strPostDataFullName = Path.Combine(strTestDirName, strTestName + ".ans");

                m_dGridSize = dlongerLength / 50.0f;
                m_dVectorScale = 10;


                // 해석결과가 존재하지 않으면 Result 와 Report 버튼을 비활성화 한다.
                if (m_manageFile.isExistFile(strPostDataFullName) == true)
                {
                    // Result 버튼이 동작하게 한다.
                    buttonLoadForceAndMagnitudeB.Enabled = true;
                    buttonLoadForceAndVectorB.Enabled = true;

                    plotForceDensityResult(minX, maxX, minY, maxY, true, true);
                }
                else
                {
                    buttonLoadForceAndMagnitudeB.Enabled = false;
                    buttonLoadForceAndVectorB.Enabled = false;
                }


                if (diffTime.Hours > 0)
                    CNotice.printUserMessage(strTestName + m_resManager.GetString("_THBC") +
                                                diffTime.Hours.ToString() + m_resManager.GetString("H") +
                                                diffTime.Minutes.ToString() + m_resManager.GetString("M") +
                                                diffTime.Seconds.ToString() + m_resManager.GetString("S"));
                else
                    CNotice.printUserMessage(strTestName + m_resManager.GetString("_THBC") +
                                                diffTime.Minutes.ToString() + m_resManager.GetString("M") +
                                                diffTime.Seconds.ToString() + m_resManager.GetString("S"));

                /// DoSA 를 활성화하여 창을 최상위에 위치시킨다.
                this.Activate();
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        // 후처리 영역을 계산한다.
        private double getPostRegion(ref double minX, ref double maxX, ref double minY, ref double maxY, double dExpandScale = 1.0f)
        {
            if (dExpandScale <= 0.0f)
                return 0.0f;

            // 전체 해석영역이 아니라 제품 영역으로 후처리 화면을 설정하기 위해 제품 크기를 넘겨 준다.
            double witdhSolvedRegion = Math.Abs(maxX - minX);
            double heightSolvedResion = Math.Abs(maxY - minY);

            // 해석영역의 폭과 높이 기준으로 20% 더 크게 후처리 영역으로 사용한다.
            minX = minX - witdhSolvedRegion * (dExpandScale - 1.0f);
            maxX = maxX + witdhSolvedRegion * (dExpandScale - 1.0f);
            minY = minY - heightSolvedResion * (dExpandScale - 1.0f);
            maxY = maxY + heightSolvedResion * (dExpandScale - 1.0f);

            if (witdhSolvedRegion > heightSolvedResion)
                return witdhSolvedRegion;
            else
                return heightSolvedResion;
        }

        private void buttonStrokeResult_Click(object sender, EventArgs e)
        {
            plotStrokeResult();
        }

        private void buttonTestStroke_Click(object sender, EventArgs e)
        {
            try
            {
                CStrokeTest strokeTest = (CStrokeTest)propertyGridMain.SelectedObject;

                if (strokeTest == null) return;

                double dInitialStroke = strokeTest.InitialStroke;
                double dFinalStroke = strokeTest.FinalStroke;
                int nStepCount = strokeTest.StepCount;

                // 현재 표시되고 있는 PropertyGird 창에서 Test 이름을 찾아 낸다
                string strTestName = strokeTest.NodeName;
                string strTestDirName = Path.Combine(m_design.m_strDesignDirPath, strTestName);

                string strTestFullName = Path.Combine(strTestDirName, strTestName + ".fem");
                string strStrokeFileFullName = Path.Combine(strTestDirName, strTestName + ".csv");


                if (false == isStrokeTestOK(strokeTest))
                    return;

                if (m_manageFile.isExistDirectory(strTestDirName) == true)
                {
                    DialogResult ret = CNotice.noticeWarningYesNoID("TIAP", "NE");

                    if (ret == DialogResult.No)
                        return;

                    m_manageFile.deleteDirectory(strTestDirName);

                    // 삭제되는 시간이 필요한 듯 한다.
                    Thread.Sleep(1000);
                }

                // 사용자 메시지를 초기화 한다.
                messageListView.clearMessage();

                if (CSettingData.m_emLanguage == EMLanguage.Korean)
                    CNotice.printUserMessage(strokeTest.NodeName + " 변위별 자기력 실험을 시작합니다.");
                else
                    CNotice.printUserMessage(strokeTest.NodeName + " magnetic force test by stroke is started.");

                // 시험 디렉토리를 생성한다.
                m_manageFile.createDirectory(strTestDirName);

                // 해석전 현 설정을 저장한다.
                saveDesignFile();

                // 혹시 FEMM 의 화면이 닫힌 경우 FEMM 의 화면을 복원합니다.
                reopenFEMM();

                m_design.addMaterials(m_femm);

                m_design.drawDesign(m_femm);

                double minX, maxX, minY, maxY;
                minX = maxX = minY = maxY = 0;

                m_femm.saveAs(strTestFullName);

                double dStepIncrease = Math.Abs(dFinalStroke - dInitialStroke) / nStepCount;
                double dStroke;

                /// Progress Bar 설정
                progressBarStroke.Style = ProgressBarStyle.Blocks;
                progressBarStroke.Minimum = 0;
                progressBarStroke.Maximum = nStepCount;
                progressBarStroke.Step = 1;
                progressBarStroke.Value = 0;

                List<string> listString = new List<string>();

                DateTime previousTime = new DateTime();
                previousTime = DateTime.Now;

                string strMessage = string.Empty;

                /// 총 계산횟수는 Step + 1 회이다.
                for (int i = 0; i < nStepCount + 1; i++)
                {
                    dStroke = dInitialStroke + dStepIncrease * i;

                    m_design.setBlockPropeties(m_femm, strokeTest.Voltage, strokeTest.MeshSizePercent);

                    m_design.getModelMinMaxX(ref minX, ref maxX);
                    // 이동으로 고려해야 함으로 초기변위와 최대변위를 모두 넘긴다.
                    m_design.getModelMinMaxY(ref minY, ref maxY, dStroke);

                    m_design.setBoundary(m_femm, strokeTest.MeshSizePercent, minX, maxX, minY, maxY);

                    /// 항상 초기위치 기준으로 이동한다.
                    m_femm.moveMovingParts(dStroke);

                    progressBarStroke.PerformStep();
                    labelProgressStroke.Text = "Storke Step : " + i.ToString() + " / " + nStepCount.ToString();

                    getPostRegion(ref minX, ref maxX, ref minY, ref maxY, 1.2f);

                    // 해석 중에 전체 해석영역이 표시되는 것을 막기 위해 제품영역 정보를 송부한다. 
                    double dForce = m_femm.solveForce(minX, maxX, minY, maxY);

                    /// 해석을 마치고 나면 구동부를 다시 초기위치로 복귀시킨다.
                    m_femm.moveMovingParts(-dStroke);

                    string strStroke = String.Format("{0}", dStroke);
                    string strForce = String.Format("{0}", dForce);

                    listString.Add(strStroke + "," + strForce);

                    strMessage = string.Format("Count = {0}, Voltage = {1}, Stroke = {2}, Force = {3}", i + 1, strokeTest.Voltage, dStroke, dForce);
                    CNotice.printUserMessage(strMessage);

                    // 전처리를 부품 형상으로 초기화 한다.
                    m_design.redrawDesign(m_femm);
                }

                // Force 계산이 진행되면 후처리로 변화하기 때문에 후처리 모드를 저장한다.
                // 추후 Tree 를 선택할 때 전처리로 모드를 전환한다.
                m_bPostMode = true;


                DateTime currentTime = new DateTime();
                currentTime = DateTime.Now;

                TimeSpan diffTime = currentTime - previousTime;

                //closePostView();

                CWriteFile writefile = new CWriteFile();

                writefile.writeLineString(strStrokeFileFullName, listString, true);

                plotStrokeResult();

                // Result 버튼이 동작하게 한다.
                buttonLoadStrokeResult.Enabled = true;

                if (diffTime.Hours > 0)
                {
                    CNotice.printUserMessage(strTestName + m_resManager.GetString("_THBC") +
                                                diffTime.Hours.ToString() + m_resManager.GetString("H") +
                                                diffTime.Minutes.ToString() + m_resManager.GetString("M") +
                                                diffTime.Seconds.ToString() + m_resManager.GetString("S"));
                }
                else
                {
                    CNotice.printUserMessage(strTestName + m_resManager.GetString("_THBC") +
                                                diffTime.Minutes.ToString() + m_resManager.GetString("M") +
                                                diffTime.Seconds.ToString() + m_resManager.GetString("S"));
                }

                /// DoSA 를 활성화하여 창을 최상위에 위치시킨다.
                this.Activate();
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }
        
        #endregion
              
        #region---------------------- Windows Message -----------------------

        /// <summary>
        /// FEMM 의 위치가 DoSA 와 연동되도록 한다.
        /// </summary>
        private void FormMain_Move(object sender, EventArgs e)
        {
            //int posX = ((FormMain)sender).Location.X;
            //int posY = ((FormMain)sender).Location.Y;

            //const int FEMM_WIDTH = 500;

            //CProgramFEMM.moveFEMM(posX - FEMM_WIDTH, posY);
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 이름이 지정된 Design 만 저장을 확인한다.
            if (m_design.m_bChanged == true)
            {
                if (DialogResult.Yes == CNotice.noticeWarningYesNoID("DYWT1", "W"))
                {
                    saveDesignFile();
                }
            }
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            try
            {
                // 설치버전을 확인 한다.
                // - 버전 확인은 프로그램 동작과 상관없이 진행되기 때문에 Thread 로 실행한다.
                // - FormMain_Shown() 에서 실행해서 
                //   Main 화면이 열린 후 동작함으로써 메시지 창이 앞으로 위치하도록 한다
                Thread threadVersionCheck = new Thread(new ThreadStart(checkDoSAVersion));
                threadVersionCheck.IsBackground = true;
                threadVersionCheck.Start();

                // 커멘드 파라메터로 디자인 파일명이 넘어오지 않은 경우는 바로 리턴한다.
                if (m_strCommandLineDesignFullName == string.Empty)
                    return;
            
                if (false == m_manageFile.isExistFile(m_strCommandLineDesignFullName))
                {
                    if (CSettingData.m_emLanguage == EMLanguage.Korean)
                        CNotice.noticeWarning("디자인 파일이 존재하지 않습니다.");
                    else
                        CNotice.noticeWarning("The design file does not exist.");

                    return;
                }

                if (false == checkDesignFile(ref m_strCommandLineDesignFullName))
                    return;

                if (false == loadDesignFile(m_strCommandLineDesignFullName))
                    return;

                // 디자인 파일이 생성될 때의 디자인 작업 디렉토리는 프로그램 기본 디렉토리 강제 설정하고 있다.
                // 만약 디렉토리를 옮긴 디자인 디렉토리를 오픈 할 경우라면 
                // 이전 다지인 작업 디렉토리를 그대로 사용하면 디렉토리 문제가 발생하여 실행이 불가능하게 된다.
                // 이를 해결하기 위해
                // 작업파일을 Open 할 때는 파일을 오픈하는 위치로 작업파일의 디렉토리를 다시 설정하고 있다.
                m_design.m_strDesignDirPath = Path.GetDirectoryName(m_strCommandLineDesignFullName);

                // 프로젝트가 시작 했음을 표시하기 위해서 TreeView 에 기본 가지를 추가한다.
                TreeNode treeNode = new TreeNode("Parts", (int)EMKind.PARTS, (int)EMKind.PARTS);
                treeViewMain.Nodes.Add(treeNode);

                treeNode = new TreeNode("Tests", (int)EMKind.TESTS, (int)EMKind.TESTS);
                treeViewMain.Nodes.Add(treeNode);

                foreach (CDataNode node in m_design.GetNodeList)
                    this.addTreeNode(node.NodeName, node.KindKey);

                openFEMM();

                // 제목줄에 디자인명을 표시한다
                this.Text = "DoSA-2D - " + m_design.m_strDesignName;

                CNotice.printUserMessage(m_design.m_strDesignName + m_resManager.GetString("_DHBO"));
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        private void pictureBoxOpenActuator_Click(object sender, EventArgs e)
        {
            string target = "http://openactuator.org";

            try
            {
                System.Diagnostics.Process.Start(target);
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    CNotice.printLog(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                CNotice.printLog(other.Message);
            }
        }

        private void pictureBoxOpenActuator_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        private void pictureBoxOpenActuator_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void panelEmpty_Resize(object sender, EventArgs e)
        {
            pictureBoxOpenActuator.Location =
                new System.Drawing.Point(panelEmpty.Width / 2 - pictureBoxOpenActuator.Width / 2 + panelEmpty.Location.X,
                                             panelEmpty.Height / 2 - pictureBoxOpenActuator.Height / 2 + panelEmpty.Location.Y);
        }

        #endregion


        #region----------------------- FEMM 제어관련 기능함수 ------------------------
        // m_femm 의 스크립트를 사용하여 제어하는 함수들은 CProgramFEMM 에 추가하지 못해서 FormMain 에 두고 있다.
        // FEMM 자체적으로 처리가 가능한 함수라면 CProgramFEMM 에 추가하라.

        public void openFEMM()
        {
            try
            {
                // FEMM.exe 가 실행되지 않았으면 FEMM 을 생성하고 크기를 변경한다.
                if (m_femm == null)
                {
                    m_femm = new CScriptFEMM();

                    resizePrograms();
                }
                // FEMM.exe 가 실행되어 열려 있는 경우는 내용만 삭제하고 크기만 변경한다.
                // FEMM.exe 가 실행되었지만 열려 있지 않은 경우라면 (사용자가 강제로 닫은 경우) 는 종료하고 다시 생성한다.
                else if (false == CProgramFEMM.isOpenedWindow())
                {
                    quitFEMM();
                    m_femm = new CScriptFEMM();

                    resizePrograms();
                }
                // 이미 정상적으로 FEMM 이 동작중이라면 화면을 초기화한다.
                else
                {
                    m_femm.deleteAll();
                }

                if (m_femm != null)
                {
                    m_design.redrawDesign(m_femm);
                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        public void reopenFEMM()
        {
            try
            {
                if (m_femm == null)
                {
                    CNotice.printLogID("YATT8");
                }
                // FEMM.exe 가 실행되어 열려 있는 경우는 내용만 삭제하고 크기만 변경한다.
                // FEMM.exe 가 실행되었지만 열려 있지 않은 경우라면 (사용자가 강제로 닫은 경우) 는 종료하고 다시 생성한다.
                else if (false == CProgramFEMM.isOpenedWindow())
                {
                    quitFEMM();
                    m_femm = new CScriptFEMM();
                    resizePrograms();

                    m_design.redrawDesign(m_femm);
                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        /// <summary>
        /// FEMM 과 DoSA 를 붙여서 화면의 가로의 중심에 배치하고 있다.
        /// </summary>
        /// <param name="widthFEMM"></param>
        private void resizePrograms(int widthFEMM = FEMM_DEFAULT_WIDTH)
        {
            try
            {
                Rectangle rectScreen = Screen.PrimaryScreen.Bounds;

                // FEMM 좌측 위치는 무조건 FEMM 의 기본크기에 맞추고 있다.
                // Magnetic Density 를 위한 FEMM 크기 수정때는 DoSA 와 겹치는 것을 허용한다.
                int iLeftPosition = (int)((rectScreen.Width - FEMM_DEFAULT_WIDTH - this.Width) / 2.0);
                int iTopPosition = 50;

                /// 이상하게 두개의 프로그램 위치가 외곽에 보이지 않는 Border 가 있는것 처럼 동작해서 Border 두께를 사용하고 있다.
                int iOutsideBorderWidth = 7;

                if (m_femm == null)
                    return;

                /// Minimized 가 되어 있으면 FEMM 프로그램 크기동작이 먹지 않는다.
                /// 먼저 실행이 되어야 한다.
                m_femm.restoreMainWindow();

                /// 좌측에 FEMM 공간을 확보하기 위해서 DoSA 의 위치를 이동시킨다.
                /// Border * 2 은 FEMM 의 우측과 DoSA 의 왼측을 같이 고려한 것이다. 
                this.Left = iLeftPosition + FEMM_DEFAULT_WIDTH - iOutsideBorderWidth * 2;
                this.Top = iTopPosition;

                //CProgramFEMM.moveFEMM(this.Location.X - FEMM_DEFAULT_WIDTH, this.Location.Y, widthFEMM, FEMM_DEFAULT_HEIGHT);
                CProgramFEMM.moveFEMM(iLeftPosition, iTopPosition, widthFEMM, FEMM_DEFAULT_HEIGHT);

                m_femm.zoomFit();
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        private void quitFEMM()
        {
            if (m_femm != null)
            {
                CProgramFEMM.killProcessOfFEMMs();

                m_femm = null;
            }
        }

        private void closePostView()
        {
            m_femm.closePost();

            m_design.redrawDesign(m_femm);
        }

        #endregion

        #region----------------------- 가상시험 관련 -------------------------------


        private bool checkMaterials()
        {
            bool bCheck = true;
            string strMaterial = string.Empty;

            try
            {
                foreach (CDataNode node in m_design.GetNodeList)
                {
                    if (node.GetType().Name == "CCoil")
                    {
                        strMaterial = ((CShapeParts)node).getMaterial();

                        if (CMaterialListInFEMM.isCoilWIreInList(strMaterial) == false)
                            bCheck = false;
                    }
                    else if (node.GetType().Name == "CMagnet")
                    {
                        strMaterial = ((CShapeParts)node).getMaterial();

                        if (CMaterialListInFEMM.isMagnetlInList(strMaterial) == false)
                            bCheck = false;
                    }
                    else if (node.GetType().Name == "CSteel")
                    {
                        strMaterial = ((CShapeParts)node).getMaterial();

                        if (CMaterialListInFEMM.isSteelInList(strMaterial) == false)
                            bCheck = false;
                    }
                }

                return bCheck;
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return false;
            }
        }

        private bool checkMovingParts()
        {
            bool bCheck = false;

            try
            {
                foreach (CDataNode node in m_design.GetNodeList)
                {
                    // Parts 만 확인한다.
                    if (node.GetType().BaseType.Name == "CShapeParts")
                    {
                        if (((CShapeParts)node).MovingPart == EMMoving.MOVING)
                            bCheck = true;
                    }
                }

                return bCheck;
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return false;
            }
        }

        private bool isForceTestOK(CForceTest forceTest)
        {

            try
            {
                if (checkMovingParts() == false)
                {
                    CNotice.noticeWarningID("ALOM");
                    return false;
                }

                if (m_design.isDesignShapeOK() == false)
                {
                    CNotice.printLogID("AEOI");
                    return false;
                }

                if (checkMaterials() == false)
                {
                    if (CSettingData.m_emLanguage == EMLanguage.Korean)
                        CNotice.noticeWarning("재질이 설정되지 않는 Part 가 존재합니다.\nPart 들의 재질명을 확인하세요.");
                    else
                        CNotice.noticeWarning("There are parts with no material set.\nCheck the material names of the parts.");

                    return false;
                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return false;
            }

            return true;
        }

        private bool isStrokeTestOK(CStrokeTest strokeTest)
        {
            try
            {
                if (strokeTest.InitialStroke >= strokeTest.FinalStroke)
                {
                    CNotice.noticeWarningID("TFSM");
                    return false;
                }

                if (strokeTest.StepCount <= 0)
                {
                    CNotice.noticeWarningID("TSSM");
                    return false;
                }

                if (checkMovingParts() == false)
                {
                    CNotice.noticeWarningID("ALOM");
                    return false;
                }

                // 구동부를 초기 변위로 이동 후에 형상 검사를 한다.
                if (m_design.isDesignShapeOK(strokeTest.InitialStroke) == false)
                {
                    CNotice.noticeWarningID("AEOT1");
                    return false;
                }

                // 구동부를 최대 변위로 이동 후에 형상 검사를 한다.
                if (m_design.isDesignShapeOK(strokeTest.FinalStroke) == false)
                {
                    CNotice.noticeWarningID("AEOT2");
                    return false;
                }

                if (checkMaterials() == false)
                {
                    if (CSettingData.m_emLanguage == EMLanguage.Korean)
                        CNotice.noticeWarning("재질이 설정되지 않는 Part 가 존재합니다.\nPart 들의 재질명을 확인하세요.");
                    else
                        CNotice.noticeWarning("There are parts with no material set.\nCheck the material names of the parts.");

                    return false;
                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return false;
            }

            return true;
        }

        private bool isCurrentTestOK(CCurrentTest currentTest)
        {
            try
            {
                if (currentTest.InitialCurrent >= currentTest.FinalCurrent)
                {
                    CNotice.noticeWarningID("TFCM");
                    return false;
                }

                if (currentTest.StepCount <= 0)
                {
                    CNotice.noticeWarningID("TCSS");
                    return false;
                }

                if (m_design.isDesignShapeOK() == false)
                {
                    CNotice.printLogID("AEOT");
                    return false;
                }

                if (checkMovingParts() == false)
                {
                    CNotice.noticeWarningID("ALOM");
                    return false;
                }

                if (checkMaterials() == false)
                {
                    if (CSettingData.m_emLanguage == EMLanguage.Korean)
                        CNotice.noticeWarning("재질이 설정되지 않는 Part 가 존재합니다.\nPart 들의 재질명을 확인하세요.");
                    else
                        CNotice.noticeWarning("There are parts with no material set.\nCheck the material names of the parts.");

                    return false;
                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return false;
            }

            return true;
        }

        private void plotForceDensityResult(double minX, double maxX, double minY, double maxY, bool bForceTest, bool bMagnitude)
        {
            // 현재 시험의 이름을 m_nodeList 에서 찾지 않고
            // 현재 표시되고 있는 PropertyGird 창에서 Test 이름을 찾아 낸다
            string strTestName = ((CForceTest)propertyGridMain.SelectedObject).NodeName;
            string strTestDirName = Path.Combine(m_design.m_strDesignDirPath, strTestName);

            string strPostDataFullName = Path.Combine(strTestDirName, strTestName + ".ans");

            string strForceFileFullName = Path.Combine(strTestDirName, strTestName + ".txt");

            string strImageFileFullName = string.Empty;

            bool bCheck = false;

            string strReturn;
            double dForce;
            CReadFile readfile = new CReadFile();

            if (bForceTest == false)
            {
                // 혹시 FEMM 의 화면이 닫힌 경우 FEMM 의 화면을 복원합니다.
                reopenFEMM();
            }

            // 이미지 캡쳐 때문에 해석중에 FEMM 의 넓이를 일시적으로 넓힌다
            resizePrograms(1040);

            //--------------------- 자기력을 저장하고 화면에 표시함 --------------------------
            try
            {
                bCheck = m_manageFile.isExistFile(strForceFileFullName);

                if (bCheck == true)
                {
                    strReturn = readfile.pickoutString(strForceFileFullName, "force:", 8, 22);
                    dForce = Double.Parse(strReturn);

                    textBoxForce.Text = dForce.ToString();
                }
                else
                {
                    CNotice.noticeWarningID("TROA1");
                    return;
                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }

            try
            {
                // 자기력 해석버튼이라면 Flux 을 출력한다.
                if (bForceTest == true)
                {
                    strImageFileFullName = Path.Combine(strTestDirName, strTestName + "_flux.bmp");

                    if (m_manageFile.isExistFile(strImageFileFullName))
                        m_manageFile.deleteFile(strImageFileFullName);

                    m_femm.writeContourImage(minX, minY, maxX, maxY, strImageFileFullName, strPostDataFullName);

                }
                else
                {
                    // 후처리 모드라면 이전 후처리를 없애고 전처리로 복구해야 화면 중복을 막을 수 있다.
                    if (m_bPostMode == true)
                    {
                        // 순서 주의 할 것
                        closePostView();

                        m_bPostMode = false;
                    }

                    if (bMagnitude == true)
                    {
                        strImageFileFullName = Path.Combine(strTestDirName, strTestName + "_magnitude.bmp");

                        if (m_manageFile.isExistFile(strImageFileFullName))
                            m_manageFile.deleteFile(strImageFileFullName);

                        m_femm.writeDensityMagnitudeImage(minX, minY, maxX, maxY, strImageFileFullName, strPostDataFullName);
                    }
                    else
                    {
                        strImageFileFullName = Path.Combine(strTestDirName, strTestName + "_vector.bmp");

                        if (m_manageFile.isExistFile(strImageFileFullName))
                            m_manageFile.deleteFile(strImageFileFullName);

                        m_femm.writeDensityVectorImage(minX, minY, maxX, maxY, strImageFileFullName, strPostDataFullName,
                                                        m_dGridSize, m_dVectorScale);
                    }
                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }

            // 후처리 모드이 인것을 저장한다.
            // 추후 Tree 를 선택할 때 전처리로 모드를 전환한다.
            m_bPostMode = true;

            //------------------------ 자속밀도 이미지 로딩 ------------------------------------
            try
            {

                bCheck = m_manageFile.isExistFile(strImageFileFullName);

                if (bCheck == true)
                {
                    // 파일을 잡고 있지 않기 위해서 임시 이미지를 사용하고 Dispose 한다.
                    Image tmpImage = Image.FromFile(strImageFileFullName);

                    pictureBoxForce.Image = new Bitmap(tmpImage);
                    pictureBoxForce.SizeMode = PictureBoxSizeMode.StretchImage;

                    // 이미지이 연결을 끊어서 사용 가능하게 한다.
                    tmpImage.Dispose();
                }
                else
                {
                    CNotice.noticeWarningID("TINR");
                    return;
                }

            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        private void plotStrokeResult()
        {
            // 기존 이미지를 숨기로 결과를 표시할 Chart 를 보이게 한다.
            chartStrokeResult.Visible = true;
            pictureBoxStroke.Visible = false;

            try
            {
                CStrokeTest strokeTest = (CStrokeTest)propertyGridMain.SelectedObject;

                if (strokeTest == null) return;

                //// 현재 표시되고 있는 PropertyGird 창에서 Test 이름을 찾아 낸다
                string strTestName = strokeTest.NodeName;
                string strTestDirName = Path.Combine(m_design.m_strDesignDirPath, strTestName);

                string strStrokeFileFullName = Path.Combine(strTestDirName, strTestName + ".csv");

                if (false == m_manageFile.isExistFile(strStrokeFileFullName))
                {
                    CNotice.noticeWarningID("TROA");
                    return;
                }

                List<double> listDataX = new List<double>();
                List<double> listDataY = new List<double>();

                CReadFile readFile = new CReadFile();

                readFile.readCSVColumnData2(strStrokeFileFullName, ref listDataX, 1);
                readFile.readCSVColumnData2(strStrokeFileFullName, ref listDataY, 2);

                if (listDataX.Count != listDataY.Count)
                {
                    CNotice.printLogID("TNOR");
                    return;
                }

                double dXMin, dXMax, dYMin, dYMax;

                dXMin = strokeTest.InitialStroke;
                dXMax = strokeTest.FinalStroke;

                //dYMin = listDataY.Min();
                //dYMax = listDataY.Max();

                dYMin = Double.NaN;
                dYMax = Double.NaN;

                // X 시간축면 스케일을 설정한다.
                drawXYChart(chartStrokeResult, listDataX, listDataY, "Stroke [mm]", "Force [N]", dXMin, dXMax, dYMin, dYMax);

            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);
                CNotice.printLogID("AEOI");

                return;
            }
        }

        private void plotCurrentResult()
        {
            // 기존 이미지를 숨기로 결과를 표시할 Chart 를 보이게 한다.
            chartCurrentResult.Visible = true;
            pictureBoxCurrent.Visible = false;

            try
            {
                CCurrentTest currentTest = (CCurrentTest)propertyGridMain.SelectedObject;

                if (currentTest == null) return;

                //// 현재 표시되고 있는 PropertyGird 창에서 Test 이름을 찾아 낸다
                string strTestName = currentTest.NodeName;
                string strTestDirName = Path.Combine(m_design.m_strDesignDirPath, strTestName);

                string strCurrentFileFullName = Path.Combine(strTestDirName, strTestName + ".csv");

                if (false == m_manageFile.isExistFile(strCurrentFileFullName))
                {
                    CNotice.noticeWarningID("TROA2");
                    return;
                }

                List<double> listDataX = new List<double>();
                List<double> listDataY = new List<double>();

                CReadFile readFile = new CReadFile();

                readFile.readCSVColumnData2(strCurrentFileFullName, ref listDataX, 1);
                readFile.readCSVColumnData2(strCurrentFileFullName, ref listDataY, 2);

                if (listDataX.Count != listDataY.Count)
                {
                    CNotice.printLogID("TNOR");
                    return;
                }

                double dXMin, dXMax, dYMin, dYMax;

                dXMin = currentTest.InitialCurrent;
                dXMax = currentTest.FinalCurrent;

                //dYMin = listDataY.Min();
                //dYMax = listDataY.Max();

                dYMin = Double.NaN;
                dYMax = Double.NaN;


                // X 시간축면 스케일을 설정한다.
                drawXYChart(chartCurrentResult, listDataX, listDataY, "Current [A]", "Force [N]", dXMin, dXMax, dYMin, dYMax);

            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);
                CNotice.printLogID("AEOI");

                return;
            }
        }

        #endregion        

        #region-------------------------- Save & Load Data -------------------------

        private bool saveDesignFile()
        {
            if (m_design.m_strDesignName.Length == 0)
            {
                CNotice.printLogID("YATT9");
                return false;
            }

            try
            {
                /// New 에서 생성할 때 바로 디렉토리를 생성하면 만약, 프로젝트를 저장하지 않으면 디렉토리만 남는다.
                /// 따라서 저장할 때 없으면 디렉토리를 생성하는 것으로 바꾸었다.
                string strDesignDirPath = m_design.m_strDesignDirPath;

                if (false == m_manageFile.isExistDirectory(strDesignDirPath))
                {
                    // 다지인 디렉토리를 생성한다.
                    m_manageFile.createDirectory(strDesignDirPath);
                }

                string strActuatorDesignFileFullName = Path.Combine(strDesignDirPath, m_design.m_strDesignName + ".dsa");

                StreamWriter writeStream = new StreamWriter(strActuatorDesignFileFullName);
                CWriteFile writeFile = new CWriteFile();

                // Project 정보를 기록한다.
                writeFile.writeBeginLine(writeStream, "DoSA_Project", 0);

                writeFile.writeDataLine(writeStream, "Writed", DateTime.Now, 1);
                writeFile.writeDataLine(writeStream, "DoSA_Version", Assembly.GetExecutingAssembly().GetName().Version, 1);
                writeFile.writeDataLine(writeStream, "File_Version", FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion, 1);

                m_design.writeObject(writeStream, 1);

                writeFile.writeEndLine(writeStream, "DoSA_Project", 0);


                // 사용자 정보를 기록한다.
                //writeFile.writeBeginLine(writeStream, "Check", 0);
                //writeFile.writeEndLine(writeStream, "Check", 0);

                writeStream.Close();

                // 저장을 하고 나면 초기화 한다.
                m_design.m_bChanged = false;

            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return false;
            }

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
                readFile.getAllLines(strDesignFileFullName, ref listStringLines);

                ///-----------------------------------------------------
                /// DoSA-2D 와 DoSA-3D 의 구분 기호가 아래와 같음을 유의하라
                ///  - DoSA-3D : DoSA_3D_Project
                ///  - DoSA-2D : DoSA_Project
                ///-----------------------------------------------------
                if (listStringLines[0].Contains("DoSA_3D_Project") == true)
                {
                    if (CSettingData.m_emLanguage == EMLanguage.Korean)
                        CNotice.noticeWarning("DoSA-3D 파일은 열 수 없습니다.");
                    else
                        CNotice.noticeWarning("DoSA-3D files cannot be opened.");

                    return false;
                }
                else if (listStringLines[0].Contains("DoSA_Project") == false)
                {
                    if (CSettingData.m_emLanguage == EMLanguage.Korean)
                        CNotice.noticeWarning("DoSA-2D 파일에 문제가 있습니다.");
                    else
                        CNotice.noticeWarning("There is a problem with the DoSA-2D file.");

                    return false;
                }

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
                CNotice.printLog(ex.Message);

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

            try
            {
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
                                    m_design.GetNodeList.Add(coil);
                                break;

                            case "Steel":
                                CSteel steel = new CSteel();
                                if (true == steel.readObject(listStringNode))
                                    m_design.GetNodeList.Add(steel);
                                break;

                            case "Magnet":
                                CMagnet magnet = new CMagnet();
                                if (true == magnet.readObject(listStringNode))
                                    m_design.GetNodeList.Add(magnet);
                                break;

                            case "Non-Kind":
                                CNonKind nonKind = new CNonKind();
                                if (true == nonKind.readObject(listStringNode))
                                    m_design.GetNodeList.Add(nonKind);
                                break;

                            // CExpriment 하위 객체
                            case "ForceExperiment":     // 하위 버전 호환 유지 ver(0.9.15.6)
                            case "ForceTest":
                                CForceTest forceTest = new CForceTest();
                                if (true == forceTest.readObject(listStringNode))
                                    m_design.GetNodeList.Add(forceTest);
                                break;

                            case "StrokeExperiment":    // 하위 버전 호환 유지 ver(0.9.15.6)
                            case "StrokeTest":
                                CStrokeTest strokeTest = new CStrokeTest();
                                if (true == strokeTest.readObject(listStringNode))
                                    m_design.GetNodeList.Add(strokeTest);
                                break;

                            case "CurrentExperiment":   // 하위 버전 호환 유지 ver(0.9.15.6)
                            case "CurrentTest":
                                CCurrentTest currentTest = new CCurrentTest();
                                if (true == currentTest.readObject(listStringNode))
                                    m_design.GetNodeList.Add(currentTest);
                                break;

                            default:
                                // 해당사항이 없는 항목은 아무것도 하지 않는다. foreach 가 동작하기 때문에 return 해서는 않된다.
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
                                    // 해당사항이 없는 항목은 아무것도 하지 않는다. foreach 가 동작하기 때문에 return 해서는 않된다.
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        #endregion

        #region------------------------- TreeView 관련 -------------------------


        //트리뷰에서 삭제 한다
        private void treeView_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                // Delete 키에서 Tree 를 삭제한다.
                if (e.KeyCode == Keys.Delete)
                {
                    // [주의] 
                    // Node Name 이 SelectedNode.Name 아니라 SelectedNode.Text 에 들어 있다
                    string selectedNodeText = this.treeViewMain.SelectedNode.Text;

                    if (selectedNodeText == "Parts" || selectedNodeText == "Tests")
                    {
                        return;
                    }

                    CDataNode node = m_design.getNode(selectedNodeText);

                    if (node == null)
                    {
                        CNotice.printLogID("TDNI");
                        return;
                    }

                    // 가상 시험 Node 의 경우는 결과 디렉토리와 연결이 되기 때문에
                    // 해석 결과 디렉토리가 있는 경우는 해석결과를 삭제할지를 물어보고 같이 삭제한다.
                    if (node.GetType().BaseType.Name == "CTest")
                    {
                        string strTestDirName = Path.Combine(m_design.m_strDesignDirPath, node.NodeName);

                        if (m_manageFile.isExistDirectory(strTestDirName) == true)
                        {
                            DialogResult ret = CNotice.noticeWarningYesNoID("TTHR", "W");

                            if (ret == DialogResult.No)
                                return;

                            m_manageFile.deleteDirectory(strTestDirName);

                            // 삭제되는 시간이 필요한 듯 한다.
                            Thread.Sleep(1000);
                        }
                    }

                    // 수정 되었음을 기록한다.
                    m_design.m_bChanged = true;

                    this.treeViewMain.SelectedNode.Remove();
                    deleteDataNode(selectedNodeText);

                    m_design.redrawDesign(m_femm);

                    // 혹시 FEMM 의 화면이 닫힌 경우 FEMM 의 화면을 복원합니다.
                    reopenFEMM();
                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        // 형상 수정창을 띄운다
        private void treeViewMain_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                // [주의] 
                // Node Name 이 SelectedNode.Name 아니라 SelectedNode.Text 에 들어 있다
                string selectedNodeText = this.treeViewMain.SelectedNode.Text;

                if (selectedNodeText == "Parts" || selectedNodeText == "Tests")
                {
                    return;
                }

                CDataNode node = m_design.getNode(selectedNodeText);

                if (node == null)
                {
                    CNotice.printLogID("TDNI");
                    return;
                }

                if (node.GetType().BaseType.Name == "CShapeParts")
                    changePartsShapeInPopup((CShapeParts)node);

                // 수정 되었음을 기록한다.
                m_design.m_bChanged = true;
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        //트리 노드를 선택
        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                string selectedNodeText = this.treeViewMain.SelectedNode.Text;

                if (selectedNodeText == "Parts" || selectedNodeText == "Tests")
                {
                    return;
                }

                // 신기하게 treeViewMain_Click 나 treeViewMain_MouseUp 이벤트에서 동작시키면 이상하게 동작한다.
                // 그래서 중복 호출이 되더라도 AfterSelect 을 사용한다.
                showDataNode(selectedNodeText);
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }


        //노드 추가를 위한 유효성검사
        private void addDataNode(EMKind emKind)
        {
            string strName = string.Empty;
            bool bRet = false;

            CDataNode Node = null;

            try
            {
                if (m_design.m_strDesignName.Length == 0)
                {
                    CNotice.noticeWarningID("TIND");
                    return;
                }

                if (emKind == EMKind.COIL || emKind == EMKind.MAGNET || emKind == EMKind.STEEL)
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
                    //popupShape.Owner = this;

                    if (DialogResult.Cancel == popupShape.ShowDialog(this))
                    {
                        // 삽입 동안 화면에 그렸던 형상을 제거한다.
                        m_design.redrawDesign(m_femm);

                        return;
                    }

                    strName = popupShape.PartName;

                    //==================================================================
                    // 결과리포트에서 지원하는 만큼 복수개의 Coil 과 Spring 을 지원한다.
                    //
                    // Coil : 2, MAGNET : 4, STEEL : 6
                    //==================================================================
                    switch (emKind)
                    {
                        case EMKind.COIL:
                            CCoil coil = new CCoil();
                            Node = coil;         /// for adding a face
                            coil.NodeName = strName;
                            coil.KindKey = emKind;

                            bRet = m_design.addDataNode(coil);
                            break;

                        case EMKind.MAGNET:
                            CMagnet magnet = new CMagnet();
                            Node = magnet;         /// for adding a face
                            magnet.NodeName = strName;
                            magnet.KindKey = emKind;

                            bRet = m_design.addDataNode(magnet);
                            break;

                        case EMKind.STEEL:
                            CSteel steel = new CSteel();
                            Node = steel;         /// for adding a face
                            steel.NodeName = strName;
                            steel.KindKey = emKind;

                            bRet = m_design.addDataNode(steel);
                            break;

                        default:
                            CNotice.printLogID("TWKO");

                            // 해당사항이 없는 항목이 넘어 왔기 때문에 바로 retrun 해서 아래의 동작을 하지 않는다.
                            return;

                    }

                    CFace face = popupShape.makeFaceInPopup();

                    if (null != face)
                    {
                        ((CShapeParts)Node).Face = face;

                        /// 형상에 맞추어 코일 설계 사양정보를 초기화 한다.
                        if (emKind == EMKind.COIL)
                            ((CCoil)Node).initialShapeDesignValue();

                        CNotice.printUserMessage(strName + m_resManager.GetString("_PHBC"));
                    }
                    else
                    {
                        CNotice.noticeWarningID("TGWN");

                        CNotice.printLogID("TGWN");

                        return;
                    }
                }
                else
                {
                    string strKind;

                    switch (emKind)
                    {
                        case EMKind.FORCE_TEST:
                            strKind = "Force Test";
                            break;

                        case EMKind.STROKE_TEST:
                            strKind = "Stroke Test";
                            break;

                        case EMKind.CURRENT_TEST:
                            strKind = "Current Test";
                            break;

                        default:
                            CNotice.printLogID("YATT4");

                            // 해당사항이 없는 항목이 넘어 왔기 때문에 바로 retrun 해서 아래의 동작을 하지 않는다.
                            return;

                    }

                    PopupNew popupNodeName = new PopupNew(strKind);
                    popupNodeName.StartPosition = FormStartPosition.CenterParent;

                    /// 이해할 수 없지만, 자동으로 Owner 설정이 되는 경우도 있고 아닌 경우도 있기 때문에
                    /// Shape 창에서 MainForm 을 접근할 수 있도록 미리 설정을 한다.
                    popupNodeName.Owner = this;

                    if (DialogResult.Cancel == popupNodeName.ShowDialog())
                        return;

                    strName = popupNodeName.m_strName;

                    switch (emKind)
                    {
                        case EMKind.FORCE_TEST:
                            CForceTest forceTest = new CForceTest();
                            forceTest.NodeName = strName;
                            forceTest.KindKey = emKind;

                            // 생성될 때 환경설정의 조건으로 초기화한다.
                            forceTest.MeshSizePercent = CSettingData.m_dMeshLevelPercent;

                            bRet = m_design.addDataNode(forceTest);
                            break;

                        case EMKind.STROKE_TEST:
                            CStrokeTest strokeTest = new CStrokeTest();
                            strokeTest.NodeName = strName;
                            strokeTest.KindKey = emKind;

                            // 생성될 때 환경설정의 조건으로 초기화한다.
                            strokeTest.MeshSizePercent = CSettingData.m_dMeshLevelPercent;

                            bRet = m_design.addDataNode(strokeTest);
                            break;

                        case EMKind.CURRENT_TEST:
                            CCurrentTest currentTest = new CCurrentTest();
                            currentTest.NodeName = strName;
                            currentTest.KindKey = emKind;

                            // 생성될 때 환경설정의 조건으로 초기화한다.
                            currentTest.MeshSizePercent = CSettingData.m_dMeshLevelPercent;

                            bRet = m_design.addDataNode(currentTest);
                            break;

                        default:
                            CNotice.printLogID("TWKO");

                            // 해당사항이 없는 항목이 넘어 왔기 때문에 바로 retrun 해서 아래의 동작을 하지 않는다.
                            return;

                    }

                    CNotice.printUserMessage(strName + m_resManager.GetString("_THBC1"));
                }

                // 수정 되었음을 기록한다.
                m_design.m_bChanged = true;

                if (bRet == true)
                {
                    // Treeview 에 추가한다
                    addTreeNode(strName, emKind);

                    // 해당 Node 의 Properies View 와 Information Windows 를 표시한다
                    showDataNode(strName);
                }
                else
                    CNotice.noticeWarningID("TNIA");
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        //노드추가
        private void addTreeNode(string strName, EMKind kind)
        {
            try
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
                    case EMKind.NON_KIND:
                        treeViewMain.Nodes[FIRST_PARTS_INDEX].Nodes.Add(treeNode);
                        break;

                    case EMKind.FORCE_TEST:
                    case EMKind.STROKE_TEST:
                    case EMKind.CURRENT_TEST:
                        treeViewMain.Nodes[FIRST_ANALYSIS_INDEX].Nodes.Add(treeNode);
                        break;

                    default:

                        // 해당사항이 없는 항목이 넘어 왔기 때문에 바로 retrun 해서 아래의 동작을 하지 않는다.
                        return;

                }

                // 추가후 노드를 선택한다
                treeViewMain.SelectedNode = treeNode;
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        // 선택한 노드를 Information Window 와 Property View 에 보여준다
        private void showDataNode(string nodeName)
        {
            CDataNode node = m_design.getNode(nodeName);

            if (node == null)
            {
                CNotice.printLogID("TDNI");
                return;
            }

            string strTestDirName = string.Empty;

            try
            {
                // 선택된 Node 를 다시 클릭한 경우고 Node 의 프로퍼티가 보이고 있는 경우라면
                // 중복동작을 막기 위해 바로 리턴한다.
                if (m_strSelectedNodeName == node.NodeName && propertyGridMain.SelectedObject != null)
                    return;
                else
                    /// 중복선택을 확인하기 위해서 선택된 이름을 보관한다.
                    m_strSelectedNodeName = node.NodeName;

                // 순서 주의 
                // - Node 를 속성창에 표시하기 전에 호출을 해야한다.
                //
                // 의문 사항
                // - CDataNode 의 표시항목인 NodeName 만 설정해도 CShapeParts 의 MovingPart 항목까지 숨겨지고 있다. 
                PropertyDescriptorCollection propCollection = TypeDescriptor.GetProperties(typeof(CDataNode));
                PropertyDescriptor descriptor = propCollection["NodeName"];

                BrowsableAttribute attrib = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
                FieldInfo isBrow = attrib.GetType().GetField("browsable", BindingFlags.NonPublic | BindingFlags.Instance);

                // Non Kind 는 속성 창을 표시하지 않도록 설정한다.
                if (node.KindKey == EMKind.NON_KIND)
                    isBrow.SetValue(attrib, false);
                else
                    isBrow.SetValue(attrib, true);

                // Node 를 속성창에 표시한다.
                propertyGridMain.SelectedObject = node;

                // 프로퍼티창의 첫번째 Column 의 폭을 변경한다. (사용 포기함)
                //setLabelColumnWidth(propertyGridMain, 160);

                /// 프로퍼티창에서 이름을 변경할 때 기존에 이미 있는 이름을 선택하는 경우 복구를 위해 저장해 둔다.
                m_strBackupNodeName = node.NodeName;

                // Expand Treeview when starting
                foreach (TreeNode tn in treeViewMain.Nodes)
                    tn.Expand();

                // FEMM 의 화면을 처리한다.
                if (m_femm != null)
                {
                    /// 부품이 선택되면 FEMM 에 선택 표시를 한다
                    if (node.GetType().BaseType.Name == "CShapeParts")
                    {
                        // 혹시 FEMM 의 화면이 닫힌 경우 FEMM 의 화면을 복원합니다.
                        reopenFEMM();

                        CShapeParts parts = (CShapeParts)node;

                        m_femm.clearSelected();

                        selectFaceOnFemm(parts);
                    }
                    /// 부품이 아닌 경우는 선택을 해지한다
                    else
                        m_femm.clearSelected();

                    // 혹시 후처리 모드라면 전처리로 복구한다.
                    if (m_bPostMode == true)
                    {
                        // 순서 주의 할 것
                        closePostView();

                        resizePrograms();

                        m_bPostMode = false;
                    }
                }

                splitContainerRight.Panel1.Controls.Clear();

                strTestDirName = Path.Combine(m_design.m_strDesignDirPath, node.NodeName);

                // 종류에 따라 우측창을 처리한다.
                switch (node.KindKey)
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
                        drawBHCurve(steel.MaterialName);
                        break;

                    case EMKind.NON_KIND:
                        splitContainerRight.Panel1.Controls.Add(this.panelNonKind);
                        break;

                    case EMKind.FORCE_TEST:

                        string strPostDataFullName = Path.Combine(strTestDirName, node.NodeName + ".ans");

                        // 해석결과가 존재하지 않으면 Result 와 Report 버튼을 비활성화 한다.
                        if (m_manageFile.isExistFile(strPostDataFullName) == true)
                        {
                            buttonLoadForceAndMagnitudeB.Enabled = true;
                            buttonLoadForceAndVectorB.Enabled = true;
                        }
                        else
                        {
                            buttonLoadForceAndMagnitudeB.Enabled = false;
                            buttonLoadForceAndVectorB.Enabled = false;
                        }

                        splitContainerRight.Panel1.Controls.Add(this.panelForce);

                        // 초기이미지가 없어서 이미지를 비우고 있다.
                        loadDefaultImage(EMKind.FORCE_TEST);
                        textBoxForce.Text = "0.0";

                        // 트리로 선택할 때도 가상실험 내부 전류를 재계산한다.
                        setCurrentInTest(node);

                        break;

                    case EMKind.STROKE_TEST:

                        string strResultForceStrokeFileFullName = Path.Combine(strTestDirName, node.NodeName + ".csv");

                        progressBarStroke.Value = 0;

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
                        loadDefaultImage(EMKind.STROKE_TEST);

                        // 트리로 선택할 때도 가상실험 내부 전류를 재계산한다.
                        setCurrentInTest(node);

                        break;

                    case EMKind.CURRENT_TEST:

                        string strResultForceCurrentFileFullName = Path.Combine(strTestDirName, node.NodeName + ".csv");

                        progressBarCurrent.Value = 0;

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
                        loadDefaultImage(EMKind.CURRENT_TEST);
                        break;

                    default:
                        // 해당사항이 없는 항목이 넘어 왔기 때문에 바로 retrun 해서 아래의 동작을 하지 않는다.
                        return;


                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        public class CSortDataSet
        {
            public double m_dLength;
            public int m_iIndex;

            public CSortDataSet(double length, int index)
            {
                m_dLength = length;
                m_iIndex = index;
            }
        }
        
        /// <summary>
        /// FEMM 의 Parts Face 색상을 변경하여 선택됨을 표시한다.
        /// 
        /// FEMM 에 표시함으로 절대좌표를 사용해야 한다
        /// </summary>
        private void selectFaceOnFemm(CShapeParts parts)
        {
            try
            {
                /// 매번 생성하는 Property 이기 때문에 
                /// LineList 는 새로운 List에  담는 동작 한번만 호출하고, 사용은 새로운 List 를 사용한다.
                List<CPoint> listAbsolutePoints = new List<CPoint>();
                listAbsolutePoints = parts.Face.AbsolutePointList;

                /// 디자인의 모든 포인트를 담는다.
                List<CPoint> listAbsoluteAllPoint = new List<CPoint>();

                foreach (CDataNode node in m_design.GetNodeList)
                {
                    /// 부품이 선택되면 FEMM 에 선택 표시를 한다
                    if (node.GetType().BaseType.Name == "CShapeParts")
                    {
                        foreach (CPoint point in ((CShapeParts)node).Face.AbsolutePointList)
                        {
                            listAbsoluteAllPoint.Add(point);
                        }
                    }
                }

                CPoint selectPoint = new CPoint();
                CPoint startPoint = new CPoint();
                CPoint endPoint = new CPoint();
                CLine line = new CLine();

                List<CPoint> listPointOnLine = new List<CPoint>();
                List<CSortDataSet> listDataSet = new List<CSortDataSet>();
                double dLength;
                int index;

                CShapeTools shapeTools = new CShapeTools();

                for (int i = 0; i < listAbsolutePoints.Count; i++)
                {
                    if (listAbsolutePoints[i].LineKind == EMLineKind.STRAIGHT)
                    {
                        //// 마지막 라인만 다르게 처리한다.
                        if (i < listAbsolutePoints.Count - 1)
                        {
                            startPoint = listAbsolutePoints[i];
                            endPoint = listAbsolutePoints[i + 1];
                        }
                        else
                        {
                            startPoint = listAbsolutePoints[i];
                            endPoint = listAbsolutePoints[0];
                        }

                        line.m_startPoint = startPoint;
                        line.m_endPoint = endPoint;

                        listPointOnLine.Clear();
                        listDataSet.Clear();
                        index = 0;

                        /// 선택한 라인 위에 있는 모든 점들을 하나의 List 에 담는다.
                        foreach (CPoint point in listAbsoluteAllPoint)
                        {
                            if (true == shapeTools.isPerchedOnLine(line, point))
                            {
                                listPointOnLine.Add(point);
                            }
                        }

                        // 양단에 두점에 라인위에 있는 경우는 라인을 분리하는 점이 없음을 의미한다.
                        if (listPointOnLine.Count == 2)
                        {
                            selectPoint.X = (startPoint.X + endPoint.X) / 2.0f;
                            selectPoint.Y = (startPoint.Y + endPoint.Y) / 2.0f;

                            m_femm.selectLine(selectPoint);
                        }
                        else
                        {
                            /// 시작점과 라인위에 모든점의 길이를 계산하고 
                            /// 인덱스와 같이 클래스로 만들어서 CSortDataSet 형태로 List 에 저장한다.
                            foreach (CPoint point in listPointOnLine)
                            {
                                dLength = Math.Sqrt(Math.Pow(startPoint.X - point.X, 2) +
                                                        Math.Pow(startPoint.Y - point.Y, 2));

                                listDataSet.Add(new CSortDataSet(dLength, index));
                                index++;
                            }

                            /// List Sort 함수를 정렬 동작을 delegate 함수로 표현하면서 바로 정렬한다.
                            ///
                            /// CSortDataSet 의 Length 로 CSortDataSet 을 정렬 한다.
                            listDataSet.Sort(delegate (CSortDataSet A, CSortDataSet B)
                            {
                                if (A.m_dLength > B.m_dLength) return 1;
                                else if (A.m_dLength < B.m_dLength) return -1;
                                return 0;
                            });

                            /// 길이로 정렬된 CSortDataSet 의 인덱스를 사용하여
                            /// 라인위의 점들로 분리된 라인들을 하나씩 선택한다.
                            for (int j = 0; j < listPointOnLine.Count - 1; j++)
                            {
                                startPoint = listPointOnLine[listDataSet[j].m_iIndex];
                                endPoint = listPointOnLine[listDataSet[j + 1].m_iIndex];

                                selectPoint.X = (startPoint.X + endPoint.X) / 2.0f;
                                selectPoint.Y = (startPoint.Y + endPoint.Y) / 2.0f;

                                m_femm.selectLine(selectPoint);
                            }
                        }

                    }
                    else if (listAbsolutePoints[i].LineKind == EMLineKind.ARC)
                    {
                        selectPoint.X = listAbsolutePoints[i].X;
                        selectPoint.Y = listAbsolutePoints[i].Y;

                        m_femm.selectArc(selectPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        /// 트리를 삭제 한다
        private void deleteDataNode(string nodeName)
        {
            // 내부적으로 명칭배열까지도 모두 삭제한다.
            m_design.deleteNode(nodeName);

            // 정보창을 초기화 한다
            splitContainerRight.Panel1.Controls.Clear();
            splitContainerRight.Panel1.Controls.Add(this.panelEmpty);

            // PropertyGrid 창을 초기화 한다.
            propertyGridMain.SelectedObject = null;
        }

        private void deleteTreeNode(string nodeName)
        {
            int iRemoveIndex = -1;

            int nodeCount = treeViewMain.Nodes[0].Nodes.Count;

            for (int i = 0; i < nodeCount; i++)
                if (treeViewMain.Nodes[0].Nodes[i].Text == nodeName)
                    iRemoveIndex = i;

            // 루프안에 삭제를 하면 인덱스 에러가 발생한다.							
            if (iRemoveIndex != -1)
                treeViewMain.Nodes[0].Nodes[iRemoveIndex].Remove();
        }

        #endregion

        #region------------------------ PropertyView 관련 ------------------------------

        //property 수정
        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            CDataNode node = (CDataNode)propertyGridMain.SelectedObject;

            if (node == null) return;

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
                        CNotice.noticeWarningID("TNIA");

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
                
                switch (node.KindKey)
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

                    case EMKind.FORCE_TEST:

                        if (e.ChangedItem.Label == "Voltage [V]")
                        {
                            setCurrentInTest(node);
                        }

                        break;

                    case EMKind.STROKE_TEST:

                        if (e.ChangedItem.Label == "Voltage [V]")
                        {
                            setCurrentInTest(node);
                        }

                        break;

                    default:
                        // 해당사항이 없는 항목이 넘어 왔기 때문에 바로 retrun 해서 아래의 동작을 하지 않는다.
                        return;

                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }

            // 수정 되었음을 기록한다.
            m_design.m_bChanged = true;

            propertyGridMain.Refresh();
        }

        private void setCurrentInTest(CDataNode node)
        {
            // 총 저항은 합산이 필요함으로 0.0f 로 초기화 한다.
            double total_resistance = 0.0f;
            try
            {

                // 총 저항
                foreach (CDataNode nodeTemp in m_design.GetNodeList)
                    if (nodeTemp.KindKey == EMKind.COIL)
                    {
                        total_resistance += ((CCoil)nodeTemp).Resistance;
                    }

                switch (node.KindKey)
                {
                    case EMKind.FORCE_TEST:

                        CForceTest forceTest = (CForceTest)node;

                        // 전류
                        if (total_resistance != 0.0f)
                            forceTest.Current = (forceTest.Voltage / total_resistance);
                        else
                            forceTest.Current = 0.0f;

                        break;

                    case EMKind.STROKE_TEST:

                        CStrokeTest strokeTest = (CStrokeTest)node;

                        // 전류
                        if (total_resistance != 0.0f)
                            strokeTest.Current = (strokeTest.Voltage / total_resistance);
                        else
                            strokeTest.Current = 0.0f;

                        break;

                    default:
                        // 해당사항이 없는 항목이 넘어 왔기 때문에 바로 retrun 해서 아래의 동작을 하지 않는다.
                        return;

                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        //property Category구성
        private void CollapseOrExpandCategory(PropertyGrid propertyGrid, string categoryName, bool bExpand = false)
        {
            try
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
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }

        #endregion

        #region----------------------- Information Window 관련 -----------------------

        // DoSA.dmat 는 
        // 해석모델을 제작할 때 연자성체 재질을 생성하는 목적이 아니라 S/W 안에서 BH Graph 출력하는 용으로 사용하고 있다.
        // 따라서 자성 재질은 FEMM 의 재질명을 의존하고 있다.
        private void drawBHCurve(String strMaterialName)
        {
            try
            {
                List<double> listH = new List<double>();
                List<double> listB = new List<double>();

                //string strMaxwellMaterialDirName = CSettingData.m_strMaxwellMaterialDirName;
                string strProgramMaterialDirName = Path.Combine(CSettingData.m_strProgramDirPath, "Materials");

                // 내장 비자성 재료
                if (strMaterialName == "Aluminum, 1100" || strMaterialName == "Copper" ||
                    strMaterialName == "316 Stainless Steel" || strMaterialName == "304 Stainless Steel" || strMaterialName == "Air")
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
                    string strMaterialFileFullName = Path.Combine(strProgramMaterialDirName, "DoSA.dmat");

                    getMaterialBHData(strMaterialFileFullName, strMaterialName, ref listH, ref listB);

                    if (listH.Count != listB.Count)
                    {
                        CNotice.printLogID("TSOT");
                        return;
                    }

                    // getMaterialBHData() 에서 오류가 발생할 경우 별도의 처리를 하지 않고,
                    // 비어 있는 listH과 listB 를 출력하여 Graph 화면의 데이터를 지움으로써 사용자에게 오류를 알려 주고 있다.
                    drawXYChart(chartBHCurve, listH, listB, "H [A/m]", "B [T]", 0.0f, 30000.0f, 0.0f, 2.5f);
                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
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
            bool bFoundMaterial = false;

            // 이전에 사용하던 List 데이터를 우선 삭제한다.
            listH.Clear();
            listB.Clear();

            if (false == m_manageFile.isExistFile(strFileFullName))
            {
                CNotice.printLogID("TMFD");
                return false;
            }

            try
            {
                readFile.getAllLines(strFileFullName, ref listString);


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
                        {
                            bBHDataGathering = true;
                            bFoundMaterial = true;
                        }
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

                if(bFoundMaterial == false)
                {
                    CNotice.printLogID("TDFT");
                    return false;
                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);
                CNotice.printLogID("AETT");

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
                    case EMKind.FORCE_TEST:
                        // 이미지를 비운다
                        pictureBoxForce.Image = null;
                        break;

                    case EMKind.STROKE_TEST:

                        // Chart 를 대신해서 이미지가 보이게 한다.
                        chartStrokeResult.Visible = false;
                        pictureBoxStroke.Visible = true;

                        strImageFullFileName = Path.Combine(CSettingData.m_strProgramDirPath, "Images", "Stroke_Information.png");
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
                            CNotice.noticeWarningID("TIII");
                            return;
                        }

                        break;

                    case EMKind.CURRENT_TEST:

                        // Chart 를 대신해서 이미지가 보이게 한다.
                        chartCurrentResult.Visible = false;
                        pictureBoxCurrent.Visible = true;

                        strImageFullFileName = Path.Combine(CSettingData.m_strProgramDirPath, "Images", "Current_Information.png");
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
                            CNotice.noticeWarningID("TIII");
                            return;
                        }

                        break;

                    case EMKind.COIL:
                    case EMKind.MAGNET:
                    case EMKind.STEEL:
                        break;

                    default:
                        // 해당사항이 없는 항목이 넘어 왔기 때문에 바로 retrun 해서 아래의 동작을 하지 않는다.
                        return;

                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

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
                    CNotice.printLogID("TMVI");
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
                CNotice.printLog(ex.Message);
                CNotice.printLogID("AEOI1");

                return;
            }
        }

        #endregion

        #region---------------------- 기타 기능함수 -----------------------------

        private bool checkFramework451()
        {
            int iReleaseKey;

            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                iReleaseKey = Convert.ToInt32(ndpKey.GetValue("Release"));

                if (iReleaseKey >= 378675)
                    return true;
                else
                    return false;

                // 393273 : "4.6 RC or later"
                // 379893 : "4.5.2 or later"
                // 378675 : "4.5.1 or later"
                // 378389 : "4.5 or later"
            }
        }

        private void openWebsite(string strWebAddress)
        {
            System.Diagnostics.Process.Start(strWebAddress);
        }

        private void changePartsShapeInPopup(CShapeParts shapePart)
        {
            try
            {
                // 형상이 설정되지 않는 경우는
                // Part 별로 다른 형상을 기본값으로 PopupShape 객체를 생성한다.
                if (shapePart.Face == null)
                {
                    CNotice.printLogID("APWT");
                    return;
                }

                // 변경전 정보를 저장해 둔다.
                string strOriginalNodeName = shapePart.NodeName;

                PopupShape popupShape = new PopupShape(shapePart.NodeName, shapePart.Face, shapePart.KindKey);
                popupShape.StartPosition = FormStartPosition.CenterParent;

                if (DialogResult.OK == popupShape.ShowDialog(this))
                {
                    if (shapePart.KindKey == EMKind.NON_KIND && popupShape.PartType != EMKind.NON_KIND)
                    {
                        CShapeParts newShapePart = new CShapeParts();

                        switch (popupShape.PartType)
                        {
                            case EMKind.COIL:
                                newShapePart = (CShapeParts)new CCoil();
                                break;

                            case EMKind.MAGNET:
                                newShapePart = (CShapeParts)new CMagnet();
                                break;

                            case EMKind.STEEL:
                                newShapePart = (CShapeParts)new CSteel();
                                break;

                            default:
                                CNotice.printLog("선택할 수 없는 종류가 선택 되었다.");
                                // 해당사항이 없는 항목이 넘어 왔기 때문에 바로 retrun 해서 아래의 동작을 하지 않는다.
                                return;

                        }

                        ///???????????????????????????????????????????????????????
                        /// C# 은 아직도 애매하다. Call by Reference 에 대해서.
                        /// ??????????????????????????????????????????????????????
                        /// Face 의 생성을 해서 교체는 가능한데 종류를 가진 객체는 교체가 불가능하기 때문에
                        /// 직접 해당 객체를 삭제하고 추가하는 방식을 사용하고 있다.
                        deleteDataNode(strOriginalNodeName);
                        deleteTreeNode(strOriginalNodeName);

                        // CFormMain 의 addDataNode() 는 Dialog 를 띠워서 추가할 때만 사용이 가능하다.
                        // 따라서 직접 추가를 위해서는 m_design 안의 addDataNode() 를 사용한다.
                        m_design.addDataNode((CDataNode)newShapePart);
                        addTreeNode(strOriginalNodeName, popupShape.PartType);

                        /// 수정이 없을때도 아래의 코드를 동일하게 사용하기 위해 객체의 교체작업도 같이 한다.
                        shapePart = newShapePart;
                    }

                    // PopupShade 창에서 수정된 좌표값으로 face 를 다시 생성한다.
                    CFace face = popupShape.makeFaceInPopup();

                    // Face 조건에 맞지 않으면 null 이 넘어 온다.
                    if (null == face)
                    {
                        CNotice.noticeWarningID("TGWN");
                        CNotice.printLogID("TGWN");
                        return;
                    }

                    ///------------------------------- 1. Node Face 교체 -------------------------
                    /// node 의 Face 를 수정된 face 로 교체한다.
                    shapePart.Face = face;

                    /// 형상에 맞추어 코일 설계 사양정보를 초기화 한다.
                    if (shapePart.KindKey == EMKind.COIL)
                        ((CCoil)shapePart).initialShapeDesignValue();

                    ///------------------------------- 2. Node Name 교체 -------------------------
                    // 노드명칭은 수정과 상관없이 무조건 Dialog 의 값으로 수정한다.                    
                    shapePart.NodeName = popupShape.PartName;

                    treeViewMain.SelectedNode.Text = shapePart.NodeName;

                    showDataNode(shapePart.NodeName);

                    // 혹시 FEMM 의 화면이 닫힌 경우 FEMM 의 화면을 복원합니다.
                    reopenFEMM();

                    // 형상변경이 있는 경우 FEMM 을 다시 실행하는데 선택을 유지하기 위해 추가한다.
                    if (m_strSelectedNodeName != string.Empty)
                        selectFaceOnFemm(shapePart);
                }
                else
                {
                    // 삽입 동안 화면에 그렸던 형상을 제거한다.
                    m_design.redrawDesign(m_femm);

                    return;
                }
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);
                CNotice.printLogID("AEOD");

                return;
            }

            // 수정 되었음을 기록한다.
            m_design.m_bChanged = true;

            CNotice.printUserMessage(shapePart.NodeName + m_resManager.GetString("_PHBM"));

            /// 수정된 코일형상을 프로퍼티에 표시한다.
            propertyGridMain.Refresh();
        }

        #endregion

    }
}
