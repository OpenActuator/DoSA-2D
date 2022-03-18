using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

using System.Runtime.InteropServices;

using Femm;
using Shapes;
using Nodes;
using Parts;
using gtLibrary;
using System.IO;
using DoSA;

namespace Scripts
{

    public static class CProgramFEMM
    {
        private static IActiveFEMM m_FEMM = null;

        #region Constants

        private const int SW_HIDE = 0;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_SHOW = 5;

        #endregion Constants

        #region APIs
        //-----------------------------------------------------------------------------
        // API 함수 사용
        //-----------------------------------------------------------------------------
        // [주의사항] 꼭 Class 안에 존재해야 함
        //

        [DllImport("user32.dll", EntryPoint = "MoveWindow")]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        /// 동작하지 SW_SHOW 가 동작하지 않아서 아래의 두 함수로 SHOW 를 동작시키고 있다.
        //[DllImport("user32.dll")]
        //public static extern Boolean ShowWindow(IntPtr hWnd, Int32 nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        //-----------------------------------------------------------------------------

        #endregion APIs
        
        public static int getYearFEMM()
        {
            if (CSettingData.m_strFemmExeFileFullName == string.Empty)
                return 2019;

            FileInfo femmInfo = new FileInfo(CSettingData.m_strFemmExeFileFullName);

            return femmInfo.LastWriteTime.Year;
        }

        public static bool checkPreviousFEMM()
        {
            try
            {
                // FEMM 설치 메인 디렉토리를 얻어낸다.
                string strFEMMDirName = Path.GetDirectoryName(CSettingData.m_strFemmExeFileFullName);
                strFEMMDirName = strFEMMDirName.Remove(strFEMMDirName.IndexOf("bin"));

                // readme.txt 의 첫 줄을 읽어낸다.
                string strReadmeFileFullName = Path.Combine(strFEMMDirName, "readme.txt");

                CReadFile readFile = new CReadFile();
                string strVersionFEMM = readFile.getLine(strReadmeFileFullName, 1);         // 내용 : FEMM 4.2 12Jan2016

                // readme.txt 에서 FEMM 4.2 버전의 Build 날짜를 읽어낸다.
                char[] separators = { ' ' };
                string[] strArray;
                strArray = strVersionFEMM.Split(separators, StringSplitOptions.None);
                string strVersionDate = strArray[2];                                        // 내용 : 12Jan2016

                if (strVersionDate.Length < 9)
                {
                    CNotice.printTraceID("TWAP4");
                    return false;
                }

                DateTime currentDataTime = new DateTime();
                DateTime limitDataTime = new DateTime();

                limitDataTime = Convert.ToDateTime("24Sep2017");
                currentDataTime = Convert.ToDateTime(strVersionDate);

                // 24Sep2017 보다 이전 버전이면 true 를 리턴한다.
                if (currentDataTime < limitDataTime)
                    return true;
                else
                    return false;

            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return false;
            }
        }

        private static bool checkFEMMInMemory()
        {
            try
            {
                Process[] processList = Process.GetProcessesByName("femm");

                if (processList.Length < 1)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return false;
            }
        }

        public static IActiveFEMM myFEMM
        {

            get
            {
                if (checkFEMMInMemory() != true)
                {
                    m_FEMM = new ActiveFEMMClass();
                }

                return m_FEMM;
            }

            private set
            {
                m_FEMM = value;
            }
        }

        public static bool loadProcessOfFEMM()
        {
            try
            {
                if (checkFEMMInMemory() != true)
                {
                    m_FEMM = new ActiveFEMMClass();

                    if (m_FEMM == null)
                        return false;
                    else
                        return true;
                }

                return true;
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return false;
            }
        }

        public static bool isOpenedWindow()
        {
            try
            {
                Process[] processList = Process.GetProcessesByName("femm");

                if (processList.Length > 1)
                {
                    CNotice.noticeWarningID("OOFP");
                    return false;
                }

                if (processList.Length != 1)
                    return false;

                Process femmProcess = processList[0];

                // Window 닫혀 있으면 Main Window Handle 값이 null 이다.
                if (femmProcess.MainWindowTitle != "")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return false;
            }
        }

        public static void moveFEMM(int iPosX, int iPosY, int iSizeX = 500, int iSizeY = 900)
        {
            try
            {
                Process[] processList = Process.GetProcessesByName("femm");
               
                if (processList.Length > 1)
                {
                    CNotice.noticeWarningID("OOFP");
                    return;
                }

                if (processList.Length != 1)
                    return;

                Process femmProcess = processList[0];

                Thread.Sleep(100);
                MoveWindow(femmProcess.MainWindowHandle, iPosX, iPosY, iSizeX, iSizeY, true);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }
        }

        public static void showFEMM()
        {
            try
            {
                Process[] processList = Process.GetProcessesByName("femm");

                if (processList.Length > 1)
                {
                    CNotice.noticeWarningID("OOFP");
                    return;
                }

                if (processList.Length != 1)
                    return;

                Process femmProcess = processList[0];

                Thread.Sleep(100);

                // 윈도우가 최소화 되어 있다면 활성화 시킨다
                ShowWindowAsync(femmProcess.MainWindowHandle, SW_SHOWNORMAL);

                // 윈도우에 포커스를 줘서 최상위로 만든다
                SetForegroundWindow(femmProcess.MainWindowHandle);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }
        }

        public static void killProcessOfFEMMs()
        {
            int nCount = 0;

            try
            {

                Process[] processList = null;

                // 실행되어 있는 모든 FEMM 을 종료시킨다.
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

                myFEMM = null;
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }
        }

    }
   
    /// <summary>
    /// 주의사항
    ///  - Script 객체는 스크립트 동작에 대해서만 그룹화하는 데 목적이있다.
    ///  - 내부에서 Script 를 제외한 Design 이나 Face 를 호출하는 동작은 추가하지 말아라.
    /// </summary>
    public class CScriptFEMM
    {
        const int MOVING_GROUP_NUM = 1;

        private string m_strBC;

        //public string prompt(string TextPrompt)
        //{
        //    return sendCommand("prompt (\"" + TextPrompt + "\")");
        //}

        //public bool msgBox(string TextMsgBox)
        //{
        //    sendCommand("messagebox(\"" + TextMsgBox + "\")");
        //    return true;
        //}

        private string sendCommand(string strCommand)
        {
            try
            {
                // ProgramFEMM 은 Static Class 라서 생성없이 바로 사용한다
                string strReturn = CProgramFEMM.myFEMM.call2femm(strCommand);

                if (strReturn.Contains("error"))
                {
                    CNotice.printTrace(strCommand);
                    CNotice.printTrace(strReturn);
                    return "error";
                }

                return strReturn;
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return "error";
            }

        }

        public CScriptFEMM()
        {
            try
            {
                m_strBC = "\"" + "BC" + "\"";

                string strCommand;

                /// FEMM 이 실행되지 않은 상태에서 sendCommand 만 호출하면 FEMM 이 실행된다.
                /// 그런데 newdocument(0) 란 명령어로 FEMM 을 실행 시키면 실행과 동시에 Document 가 만들어질 때 문제가 발생한다.
                /// 이를 해결하기 위해서 아래와 같이 FEMM 프로세스를 먼저 호출하고 Document 를 연다 
                CProgramFEMM.loadProcessOfFEMM();

                // 스크립트 생성과 동시에 전자기장 모델를 시작한다.
                strCommand = "newdocument(0)";
                sendCommand(strCommand);

                strCommand = "mi_probdef(0,\"millimeters\",\"axi\")";
                sendCommand(strCommand);
            
                //strCommand = "mi_setgrid(0.5,\"cart\")";
                strCommand = "mi_hidegrid()";
                sendCommand(strCommand);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }
        }

        //public void settingPost()
        //{
        //    string strCommand;

        //    try
        //    {

        //    }
        //    catch (Exception ex)
        //    {
        //        CNotice.printTrace(ex.Message);
        //        return;
        //    }
        //}

        //public void settingPre()
        //{
        //    string strCommand;

        //    try
        //    {
        //        strCommand = "mi_hidegrid()";
        //        sendCommand(strCommand);
        //    }
        //    catch (Exception ex)
        //    {
        //        CNotice.printTrace(ex.Message);
        //        return;
        //    }
        //}

        public void zoomFit(bool bExceptionZoomout = false)
        {
            string strCommand;

            try
            {
                strCommand = "mi_zoomnatural()";
                sendCommand(strCommand);

                if(bExceptionZoomout != true)
                {
                    strCommand = "mi_zoomout()";
                    sendCommand(strCommand);
                }
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }
        }

        public void drawPoint(double x1, double y1)
        {
            string strCommand;

            float fX1, fY1;

            fX1 = (float)x1;
            fY1 = (float)y1;

            try
            {
                /// Point 을 추가한다
                strCommand = "mi_addnode(" + fX1.ToString() + "," + fY1.ToString() + ")";
                sendCommand(strCommand);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }
        }

        public void drawLine(double x1, double y1, double x2, double y2, EMMoving emMoving = EMMoving.FIXED)
        {
            string strCommand;

            float fX1, fY1, fX2, fY2;

            fX1 = (float)x1;
            fY1 = (float)y1;
            fX2 = (float)x2;
            fY2 = (float)y2;

            try
            {
                /// Line 을 추가한다
                strCommand = "mi_addnode(" + fX1.ToString() + "," + fY1.ToString() + ")";
                sendCommand(strCommand);

                strCommand = "mi_addnode(" + fX2.ToString() + "," + fY2.ToString() + ")";
                sendCommand(strCommand);

                strCommand = "mi_addsegment(" + fX1.ToString() + "," + fY1.ToString() + "," + fX2.ToString() + "," + fY2.ToString() + ")";
                sendCommand(strCommand);

                /// 그룹을 지정하는 경우만 변경을 한다.
                if (emMoving == EMMoving.MOVING)
                {
                    /// 그룹 설정
                    ///  - Point 의 선택이 좌표계산 없이 바로 가능함
                    ///  - 또한 Point 만 그룹을 지정해도 이동이 가능하기 때문에 Point 만 설정함
                    strCommand = "mi_selectnode(" + fX1.ToString() + "," + fY1.ToString() + ")";
                    sendCommand(strCommand);

                    strCommand = "mi_selectnode(" + fX2.ToString() + "," + fY2.ToString() + ")";
                    sendCommand(strCommand);

                    strCommand = "mi_setgroup(" + MOVING_GROUP_NUM.ToString() + ")";
                    sendCommand(strCommand);

                    strCommand = "mi_clearselected()";
                    sendCommand(strCommand);      
                }
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }

        }

        public void drawLine(CLine line, EMMoving emMoving = EMMoving.FIXED)
        {
            string strCommand;

            float fX1, fY1, fX2, fY2;

            fX1 = (float)line.m_startPoint.m_dX;
            fY1 = (float)line.m_startPoint.m_dY;
            fX2 = (float)line.m_endPoint.m_dX;
            fY2 = (float)line.m_endPoint.m_dY;

            try
            {
                strCommand = "mi_addnode(" + fX1.ToString() + "," + fY1.ToString() + ")";
                sendCommand(strCommand);

                strCommand = "mi_addnode(" + fX2.ToString() + "," + fY2.ToString() + ")";
                sendCommand(strCommand);

                strCommand = "mi_addsegment(" + fX1.ToString() + "," + fY1.ToString() + "," + fX2.ToString() + "," + fY2.ToString() + ")";
                sendCommand(strCommand);

                /// 그룹을 지정하는 경우만 변경을 한다.
                if (emMoving == EMMoving.MOVING)
                {
                    /// 그룹 설정
                    ///  - Point 의 선택이 좌표계산 없이 바로 가능함
                    ///  - 또한 Point 만 그룹을 지정해도 이동이 가능하기 때문에 Point 만 설정함
                    strCommand = "mi_selectnode(" + fX1.ToString() + "," + fY1.ToString() + ")";
                    sendCommand(strCommand);

                    strCommand = "mi_selectnode(" + fX2.ToString() + "," + fY2.ToString() + ")";
                    sendCommand(strCommand);

                    strCommand = "mi_setgroup(" + MOVING_GROUP_NUM.ToString() + ")";
                    sendCommand(strCommand);

                    strCommand = "mi_clearselected()";
                    sendCommand(strCommand);      
                }
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }

        }

        public void drawArc(double x1, double y1, double x2, double y2, bool bDirectionArcBackword, EMMoving emMoving = EMMoving.FIXED)
        {
            string strCommand;

            float fX1, fY1, fX2, fY2;

            fX1 = (float)x1;
            fY1 = (float)y1;
            fX2 = (float)x2;
            fY2 = (float)y2;

            try
            {
                strCommand = "mi_addnode(" + fX1.ToString() + "," + fY1.ToString() + ")";
                sendCommand(strCommand);

                strCommand = "mi_addnode(" + fX2.ToString() + "," + fY2.ToString() + ")";
                sendCommand(strCommand);

                if (bDirectionArcBackword == true)
                {
                    strCommand = "mi_addarc(" + fX2.ToString() + "," + fY2.ToString() + "," + fX1.ToString() + "," + fY1.ToString() + "," + "90, 1)";
                    sendCommand(strCommand);
                }
                else
                {
                    strCommand = "mi_addarc(" + fX1.ToString() + "," + fY1.ToString() + "," + fX2.ToString() + "," + fY2.ToString() + "," + "90, 1)";
                    sendCommand(strCommand);
                }

                /// 그룹을 지정하는 경우만 변경을 한다.
                if (emMoving == EMMoving.MOVING)
                {
                    /// 그룹 설정
                    ///  - Point 의 선택이 좌표계산 없이 바로 가능함
                    ///  - 또한 Point 만 그룹을 지정해도 이동이 가능하기 때문에 Point 만 설정함
                    strCommand = "mi_selectnode(" + fX1.ToString() + "," + fY1.ToString() + ")";
                    sendCommand(strCommand);

                    strCommand = "mi_selectnode(" + fX2.ToString() + "," + fY2.ToString() + ")";
                    sendCommand(strCommand);

                    strCommand = "mi_setgroup(" + MOVING_GROUP_NUM.ToString() + ")";
                    sendCommand(strCommand);

                    strCommand = "mi_clearselected()";
                    sendCommand(strCommand);      
                }
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }

        }

        public void addCircuitProp(string strCircuit, double dCurrent)
        {

            string strCommand;

            try
            {
                strCircuit = "\"" + strCircuit + "\"";

                // 마지막 파라메타 1 은 Serial 방식으로 추후 Turns 입력이 필요한다.
                strCommand = "mi_addcircprop(" + strCircuit + "," + dCurrent.ToString() + ",1)";
                sendCommand(strCommand);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }

        }

        public void setBlockProp(CPoint point,
                                string strMaterial, 
                                double dMeshsize, 
                                string strCircuit, 
                                double dMagnetAngle, 
                                EMMoving emMoving, 
                                int nTurns)
        {
            string strCommand;
            
            try
            {
                /// mode 변경 없이도 동작은 한다.
                strCommand = "mi_seteditmode(\"blocks\")";
                sendCommand(strCommand);

                strCommand = "mi_addblocklabel(" + point.m_dX + "," + point.m_dY + ")";
                sendCommand(strCommand);

                strCommand = "mi_selectlabel(" + point.m_dX + "," + point.m_dY + ")";
                sendCommand(strCommand);

                int nGroup;

                if (emMoving == EMMoving.MOVING)
                    nGroup = MOVING_GROUP_NUM;
                else
                    nGroup = 0;

                strMaterial = "\"" + strMaterial + "\"";
                strCircuit = "\"" + strCircuit + "\"";

                strCommand = "mi_setblockprop(" + strMaterial
                             + ",0," + dMeshsize.ToString() + "," + strCircuit
                             + "," + dMagnetAngle.ToString() + ","
                             + nGroup.ToString() + "," + nTurns.ToString() + ")";
                sendCommand(strCommand);

                strCommand = "mi_clearselected()";
                sendCommand(strCommand);

                // editmode 를 group 으로 바꾸어서 FEMM 마우스 동작을 막는다.
                lockEdit();
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }

        }

        /// <summary>
        /// 주의사항
        ///  - 형상과 Group 이 지정된 후에 호출되어야 한다.
        /// </summary>
        public void moveMovingParts(double dMovingStroke)
        {
            string strCommand;

            try
            {
                /// mode 변경 없이도 동작은 한다.
                strCommand = "mi_seteditmode(\"group\")";
                sendCommand(strCommand);

                strCommand = "mi_selectgroup(" + MOVING_GROUP_NUM.ToString() + ")";
                sendCommand(strCommand);

                strCommand = "mi_movetranslate(" + "0," + dMovingStroke.ToString() + ")";
                sendCommand(strCommand);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }

        }

        public void addMaterial(string strMaterial)
        {
            string strCommand;

            strMaterial = "\"" + strMaterial + "\"";

            try
            {
                strCommand = "mi_getmaterial(" + strMaterial + ")";
                sendCommand(strCommand);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }

        }

        public void addBoundaryConditon()
        {
            string strCommand;

            try
            {
                strCommand = "mi_addboundprop(" + m_strBC + ",0,0,0,0,0,0,0,0,3)";
                sendCommand(strCommand);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }
        }

        public void drawBoundaryLine(double x1, double y1, double x2, double y2)
        {
            string strCommand;

            float fX1, fY1, fX2, fY2, fCenterX, fCenterY;

            fX1 = (float)x1;
            fY1 = (float)y1;
            fX2 = (float)x2;
            fY2 = (float)y2;
            fCenterX = (fX1 + fX2) / 2.0f;
            fCenterY = (fY1 + fY2) / 2.0f;

            try
            {
                /// Line 을 추가한다
                strCommand = "mi_addnode(" + fX1.ToString() + "," + fY1.ToString() + ")";
                sendCommand(strCommand);

                strCommand = "mi_addnode(" + fX2.ToString() + "," + fY2.ToString() + ")";
                sendCommand(strCommand);

                strCommand = "mi_addsegment(" + fX1.ToString() + "," + fY1.ToString() + "," + fX2.ToString() + "," + fY2.ToString() + ")";
                sendCommand(strCommand);


                /// 경계조건을 부여한다.
                strCommand = "mi_selectsegment(" + fCenterX.ToString() + "," + fCenterY.ToString() + ")";
                sendCommand(strCommand);

                strCommand = "mi_setsegmentprop(" + m_strBC + ",0,0)";
                sendCommand(strCommand);

                strCommand = "mi_clearselected()";
                sendCommand(strCommand);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }

        }

        public void setRegionBlockProp(CPoint blockPoint, double dMeshSize)
        {
            string strCommand;

            try
            {
                strCommand = "mi_addblocklabel(" + blockPoint.m_dX + "," + blockPoint.m_dY + ")";
                sendCommand(strCommand);

                strCommand = "mi_selectlabel(" + blockPoint.m_dX + "," + blockPoint.m_dY + ")";
                sendCommand(strCommand);

                /// Region 의 물성치를 Default 물성치로 지정하여 Block 이 추가되지 않은 영역을 설정 한다.
                /// 
                strCommand = "mi_attachdefault()";
                sendCommand(strCommand);

                string strMaterial = "\"" + "Air" + "\"";

                if(dMeshSize == 0)
                    strCommand = "mi_setblockprop(" + strMaterial + ",1,0,\"none\",0,0,0)";
                else
                    strCommand = "mi_setblockprop(" + strMaterial + ",0," + dMeshSize.ToString() + ",\"none\",0,0,0)";
                sendCommand(strCommand);

                strCommand = "mi_clearselected()";
                sendCommand(strCommand);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }

        }

        public void saveAs(string strExperimentFullName)
        {
            string strCommand;

            try
            {
                //-------------------------------------------------------------
                // 아주 중요
                //-------------------------------------------------------------
                //
                // 디렉토리에 들어있는 \\ 기호는 FEMM 에서 인식하지 못한다.
                // 따라서 디렉토리안의 \\ 기호를 / 기호로 변경한다
                strExperimentFullName = strExperimentFullName.Replace("\\", "/");
                //-------------------------------------------------------------

                strExperimentFullName = "\"" + strExperimentFullName + "\"";

                strCommand = "mi_saveas(" + strExperimentFullName + ")";
                sendCommand(strCommand);    
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }
        }

        /// 해석 중에 전체 해석영역이 표시되는 것을 막기 위해 제품영역 정보를 송부한다. 
        public double solveForce(double minX, double maxX, double minY, double maxY)
        {
            string strCommand = string.Empty;
            double dForce;

            try
            {
                // mi_loadsolution() 에서 다시 크기가 초기화 되기 때문에 굳이 필요없다.
                //
                //strCommand = "mi_zoom(" + minX.ToString() + "," + minY.ToString() + "," + maxX.ToString() + "," + maxY.ToString() + ")";
                //sendCommand(strCommand);

                strCommand = "mi_analyze()";
                sendCommand(strCommand);

                strCommand = "mi_loadsolution()";
                sendCommand(strCommand);

                strCommand = "mo_zoom(" + minX.ToString() + "," + minY.ToString() + "," + maxX.ToString() + "," + maxY.ToString() + ")";
                sendCommand(strCommand);

                strCommand = "mi_seteditmode(\"group\")";
                sendCommand(strCommand);

                strCommand = "mo_groupselectblock(" + MOVING_GROUP_NUM.ToString() + ")";
                sendCommand(strCommand);

                strCommand = "mo_blockintegral(19)";
                dForce = Double.Parse(sendCommand(strCommand));

                strCommand = "mo_clearblock()";
                sendCommand(strCommand);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return 0;
            }
            
            return dForce;
        }

        public bool writeDensityMagnitudeImage( double minX, double minY, 
                                                double maxX, double maxY, 
                                                string strFieldImageName, 
                                                string strPostDataFullName)
        {
            string strCommand = string.Empty;

            //-------------------------------------------------------------
            // 아주 중요
            //-------------------------------------------------------------
            //
            // 디렉토리에 들어있는 \\ 기호는 FEMM 에서 인식하지 못한다.
            // 따라서 디렉토리안의 \\ 기호를 / 기호로 변경한다
            strPostDataFullName = strPostDataFullName.Replace("\\", "/");
            strFieldImageName = strFieldImageName.Replace("\\", "/");
            //-------------------------------------------------------------

            strPostDataFullName = "\"" + strPostDataFullName + "\"";

            //if(bForceExperiment == true)
            //{
            //    strCommand = "mi_loadsolution()";
            //    sendCommand(strCommand);
            //}
            //else
            //{
                // 해석 후 바로 출력은 상관 없지만
                // 프로그램을 다시 실행한 다음에 해석결과로 이미지를 만들때는 ans 파일을 읽어드리고 진행해야 한다.
                strCommand = "open(" + strPostDataFullName + ")";
                sendCommand(strCommand);
            //}

            strCommand = "mo_zoom(" + minX.ToString() + "," + minY.ToString() + "," + maxX.ToString() + "," + maxY.ToString() + ")";
            sendCommand(strCommand);


            //------------------ 자속밀도 Magnitude 저장 -------------------
            strCommand = "mo_showdensityplot(1, 0, 1.7, 0, \"bmag\")";
            sendCommand(strCommand);

            strFieldImageName = "\"" + strFieldImageName + "\"";

            strCommand = "mo_savebitmap(" + strFieldImageName + ")";
            sendCommand(strCommand);

            return true;
        }

        public bool writeDensityVectorImage(    double minX, double minY, 
                                                double maxX, double maxY, 
                                                string strFieldImageName,
                                                string strPostDataFullName,
                                                //bool bForceTest,
                                                double gridSize, 
                                                double vectorScale)
        {
            string strCommand = string.Empty;

            //-------------------------------------------------------------
            // 아주 중요
            //-------------------------------------------------------------
            //
            // 디렉토리에 들어있는 \\ 기호는 FEMM 에서 인식하지 못한다.
            // 따라서 디렉토리안의 \\ 기호를 / 기호로 변경한다
            strPostDataFullName = strPostDataFullName.Replace("\\", "/");
            strFieldImageName = strFieldImageName.Replace("\\", "/");
            //-------------------------------------------------------------

            strPostDataFullName = "\"" + strPostDataFullName + "\"";

            //if (bForceTest == true)
            //{
            //    strCommand = "mi_loadsolution()";
            //    sendCommand(strCommand);
            //}
            //else
            //{
                // 해석 후 바로 출력은 상관 없지만
                // 프로그램을 다시 실행한 다음에 해석결과로 이미지를 만들때는 ans 파일을 읽어드리고 진행해야 한다.
                strCommand = "open(" + strPostDataFullName + ")";
                sendCommand(strCommand);
            //}


            strCommand = "mo_zoom(" + minX.ToString() + "," + minY.ToString() + "," + maxX.ToString() + "," + maxY.ToString() + ")";
            sendCommand(strCommand);

            //------------------ 자속밀도 Magnitude 저장 -------------------
            strCommand = "mo_setgrid(" + gridSize.ToString() + ",\"cart\")";
            sendCommand(strCommand);

            strCommand = "mo_showvectorplot(1, " + vectorScale.ToString() + ")";
            sendCommand(strCommand);

            strFieldImageName = "\"" + strFieldImageName + "\"";

            strCommand = "mo_savebitmap(" + strFieldImageName + ")";
            sendCommand(strCommand);

            return true;
        }

        public void lockEdit()
        {
            string strCommand;

            // edit 모드를 group 으로 지정해서 마우스 동작을 막는다.
            try
            {
                /// nodes, segments, arcsegments, blocks, group
                strCommand = "mi_seteditmode(\"group\")";
                sendCommand(strCommand);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }
        }

        public void selectLine(CPoint selectPoint)
        {
            string strCommand;

            try
            {
                /// nodes, segments, arcsegments, blocks, group
                strCommand = "mi_seteditmode(\"segments\")";
                sendCommand(strCommand);

                strCommand = "mi_selectsegment(" + selectPoint.m_dX + "," + selectPoint.m_dY + ")";
                sendCommand(strCommand);

                /// editmode 를 group 으로 바꾸어서 FEMM 마우스 동작을 막는다.
                /// - refreshView() 전에 실행해야 한다. 
                lockEdit();

                /// refresh 를 꼭 해야 색상이 변한다
                refreshView();
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }
        }

        public void selectPoint(CPoint selectPoint)
        {
            string strCommand;

            try
            {
                /// nodes, segments, arcsegments, blocks, group
                strCommand = "mi_seteditmode(\"nodes\")";
                sendCommand(strCommand);

                strCommand = "mi_selectnode(" + selectPoint.m_dX + "," + selectPoint.m_dY + ")";
                sendCommand(strCommand);

                /// editmode 를 group 으로 바꾸어서 FEMM 마우스 동작을 막는다.
                /// - refreshView() 전에 실행해야 한다. 
                lockEdit();

                /// refresh 를 꼭 해야 색상이 변한다
                refreshView();
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }
        }

        /// <summary>
        /// 1. 문제점
        /// FEMM 프로세스로 FEMM 의 실행여부를 확인할 수 있으나,
        /// ActiveFEMMClass() 로 생성된 FEMM 을 사용자가 종료하면 FEMM 프로세스가 정리되지 않는 문제가 있다.
        /// 
        /// 2. 해결방안
        /// FEMM 이 살아있는지를 확인하기 위하여 임의의 한점을 생성하고 위치를 확인하는 방법을 사용한다.
        /// </summary>
        //public bool isLiveFEMM()
        //{
        //    string strCommand;
        //    string strReturn;

        //    double farX = 1e10;
        //    double farY = 1e10;

        //    try
        //    {
        //        // 확인점을 추가, 선택, 삭제하기 전에 기존 선택된 객체들은 선택을 해제 해주어야 한다.
        //        strCommand = "mi_clearselected()";
        //        sendCommand(strCommand);

        //        /// nodes, segments, arcsegments, blocks, group
        //        strCommand = "mi_seteditmode(\"nodes\")";
        //        sendCommand(strCommand);

        //        /// 아주 먼곳에 임의의 한점을 생성한다
        //        strCommand = "mi_addnode(" + farX.ToString() + "," + farY.ToString() + ")";
        //        sendCommand(strCommand);
                        
        //        strCommand = "mi_selectnode(" + farX.ToString() + "," + farY.ToString() + ")";
        //        strReturn = sendCommand(strCommand);

        //        /// 확인 후 삭제한다
        //        strCommand = "mi_deleteselectednodes()";
        //        sendCommand(strCommand);

        //        /// editmode 를 group 으로 바꾸어서 FEMM 마우스 동작을 막는다.
        //        /// - refreshView() 전에 실행해야 한다. 
        //        lockEdit();

        //        refreshView();
        //    }
        //    catch (Exception ex)
        //    {
        //        CNotice.printTrace(ex.Message);
        //        return false;
        //    }

        //    if (strReturn == "error")
        //        return false;
        //    else
        //        return true;
        //}

        public void clearSelected()
        {
            string strCommand;

            try
            {
                strCommand = "mi_clearselected()";
                sendCommand(strCommand);

                /// refresh 를 꼭 해야 색상이 변한다
                refreshView();
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }

        }

        public void refreshView()
        {
            string strCommand;

            try
            {
                /// refresh 를 꼭 해야 색상이 변한다
                strCommand = "mi_refreshview()";
                sendCommand(strCommand);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }
        }

        /// <summary>  
        /// 그룹 선택을 통해 모든 객체들을 삭제한다.
        /// </summary>
        public void deleteAll()
        {
            string strCommand;

            try
            {
                /// 기본 그룹을 삭제한다.
                strCommand = "mi_selectgroup(" + 0.ToString() + ")";
                sendCommand(strCommand);

                strCommand = "mi_deleteselected()";
                sendCommand(strCommand);

                /// 이동 그룹을 삭제한다.
                strCommand = "mi_selectgroup(" + MOVING_GROUP_NUM.ToString() + ")";
                sendCommand(strCommand);

                strCommand = "mi_deleteselected()";
                sendCommand(strCommand);

                refreshView();
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }

        }

        public void closePost()
        {
            string strCommand;

            try
            {
                /// refresh 를 꼭 해야 색상이 변한다
                strCommand = "mo_close()";
                sendCommand(strCommand);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }
        }

        public void selectArc(CPoint selectPoint)
        {
            string strCommand;

            try
            {
                /// nodes, segments, arcsegments, blocks, group
                strCommand = "mi_seteditmode(\"arcsegments\")";
                sendCommand(strCommand);

                strCommand = "mi_selectarcsegment(" + selectPoint.m_dX + "," + selectPoint.m_dY + ")";
                sendCommand(strCommand);

                /// editmode 를 group 으로 바꾸어서 FEMM 마우스 동작을 막는다.
                /// - refreshView() 전에 실행해야 한다. 
                lockEdit();

                /// refresh 를 꼭 해야 색상이 변한다
                refreshView();
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }
        }

        public void closePre()
        {
            string strCommand;

            try
            {
                /// nodes, segments, arcsegments, blocks, group
                strCommand = "mi_close()";
                sendCommand(strCommand);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }
        }

        public bool attachDefault(string strExperimentFullName, CPoint pointBoundaryBlock)
        {
            CReadFile readFile = new CReadFile();
            CManageFile manageFile = new CManageFile();

            List<string> listString = new List<string>();
            string strLine = string.Empty;
            char[] separators = { ' ', '\t' };
            string[] strArray;
 
            string strFileName = Path.GetFileNameWithoutExtension(strExperimentFullName);
            string strTempFileFullName = Path.Combine(Path.GetDirectoryName(strExperimentFullName), strFileName + "_temp.fem");

            if (manageFile.isExistFile(strExperimentFullName) == false)
            {
                CNotice.printTraceID("NFFF");
                return false;
            }
            else
            {
                File.Move(strExperimentFullName, strTempFileFullName);
            }

            StreamWriter writeFile = new StreamWriter(strExperimentFullName);
            int iNumBlock = 0;
            int nCountBlock = 0;
            bool bBlockLabels = false;

            try
            {
                readFile.getAllLines(strTempFileFullName, ref listString);
  
                for (int i = 0; i < listString.Count; i++)
                {
                    strLine = listString[i];

                    strArray= strLine.Split(separators, StringSplitOptions.None);

                    if (strArray[0] == "[NumBlockLabels]")
                    {
                        iNumBlock = Int32.Parse(strArray[2]);
                        nCountBlock = 0;
                        bBlockLabels = true;

                        writeFile.WriteLine(strLine);

                        /// 구분 Label 행은 건너 뛴다.
                        continue;
                    }

                    if(bBlockLabels == true)
                    {
                        if(pointBoundaryBlock.m_dX == Double.Parse(strArray[0]) && pointBoundaryBlock.m_dY == Double.Parse(strArray[1]))
                        {
                            if(strArray.Length != 9)
                            {
                                CNotice.printTraceID("TWAP2");
                                return false;
                            }

                            /// dettach block setting
                            strArray[8] = "2";
                            strLine = string.Empty;

                            foreach(string str in strArray)
                            {
                                strLine += str + '\t';
                            }
                        }

                        nCountBlock++;

                        if(nCountBlock >= iNumBlock)
                            bBlockLabels = false;
                    }

                    writeFile.WriteLine(strLine);
                }

                File.Delete(strTempFileFullName);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                writeFile.Close();
                return false;
            }

            writeFile.Close();
            return true;
        }

        public void openDesign(string strExperimentFullName)
        {
            string strCommand;

            //-------------------------------------------------------------
            // 아주 중요
            //-------------------------------------------------------------
            //
            // 디렉토리에 들어있는 \\ 기호는 FEMM 에서 인식하지 못한다.
            // 따라서 디렉토리안의 \\ 기호를 / 기호로 변경한다
            strExperimentFullName = strExperimentFullName.Replace("\\", "/");
            //-------------------------------------------------------------

            strExperimentFullName = "\"" + strExperimentFullName + "\"";

            try
            {
                /// nodes, segments, arcsegments, blocks, group
                strCommand = "open(" + strExperimentFullName + ")";
                sendCommand(strCommand);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }
        }

        internal void restoreMainWindow()
        {
            string strCommand;

            try
            {
                strCommand = "main_restore()";
                sendCommand(strCommand);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return;
            }
        }
    } 
}
