using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using gtLibrary;

// 다국어 언어 지원
using System.Globalization;
using System.Threading;

namespace DoSA
{
    public enum EMLanguage
    {
        Korean,
        English        
    };

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FormMain formMain = null;

            if (args.Length == 1)
                formMain = new FormMain(args[0]);
            else if (args.Length == 2)
                formMain = new FormMain(args[0], args[1]);
            else
                formMain = new FormMain();

            Application.Run(formMain);
        }
    }

    public class CSettingData
    {
        static CManageFile m_manageFile = new CManageFile();

        public static string m_strBaseWorkingDirPath { get; set; }

        public static string m_strCurrentWorkingDirPath { get; set; }

        //----------------------------------------------------------------------------------------
        // FEMM 의 실행은 아래의 파일명을 사용하지 않고 
        // CProgramFEMM.loadProcessOfFEMM() 에서 메모리에 떠 있는 ActiveFEMMClass() 를 사용하여 실행한다.
        // 따라서 m_strFemmExeFileFullName 는 FEMM 실행에는 필요가 없다.
        //
        // 하지만 ActiveFEMMClass() 의 m_FEMM 은 버전 확인과 설치여부 확인을 할 수 없어서
        // 사용자가 설치한 FEMM 을 m_strFemmExeFileFullName 로 직접 설정하게 하여 설치여부 및 버전을 확인하고 있다.
        //
        // 문제는 사용자가 두개의 FEMM 을 설치하고 
        // 사용자가 m_strFemmExeFileFullName 을 바꾸면서 다른 버전의 FEMM 을 사용하고 싶어하는 경우는
        // 사용자의 선택과 상관 없이 최종 설치 FEMM 이 동작하는 문제가 남아 있다.
        //----------------------------------------------------------------------------------------
        public static string m_strFemmExeFileFullName { get; set; }

        // m_dMeshLevelPercent 이름을 변경하지말라. 다시 환경설정을 해야한다.
        public static double m_dMeshLevelPercent { get; set; }
        public static EMLanguage m_emLanguage { get; set; }

		// 내부 사용변수
		// - 프로그램이 실행될때 초기화하여 내부에서 사용한다.
		public static string m_strProgramDirPath { get; set; }

        public static void updataLanguage()
        {
            if (m_emLanguage == EMLanguage.English)
            {
                CultureInfo ctInfo = new CultureInfo("en-US");

                Thread.CurrentThread.CurrentCulture = ctInfo;
                Thread.CurrentThread.CurrentUICulture = ctInfo;
            }
            else if (m_emLanguage == EMLanguage.Korean)
            {
                CultureInfo ctInfo = new CultureInfo("ko-KR");

                Thread.CurrentThread.CurrentCulture = ctInfo;
                Thread.CurrentThread.CurrentUICulture = ctInfo;
            }

        }

        public static bool isSettingDataOK(bool bOpenNoticeDialog = true)
        {
            bool bCheck = false;

            try
            {
                bCheck = m_manageFile.isExistFile(m_strFemmExeFileFullName);
                if (bCheck == false)
                {
                    if (bOpenNoticeDialog == true)
                        CNotice.noticeWarningID("TEFD");
                    else
                        CNotice.printLogID("TEFD");

                    return false;
                }

                bCheck = m_manageFile.isExistDirectory(m_strBaseWorkingDirPath);

                if (bCheck == false)
                {
                    if (bOpenNoticeDialog == true)
                        CNotice.noticeWarningID("TDWD");
                    else
                        CNotice.printLogID("TDWD");

                    return false;
                }

                bCheck = m_manageFile.isExistDirectory(m_strProgramDirPath);

                if (bCheck == false)
                {
                    if (bOpenNoticeDialog == true)
                        CNotice.noticeWarningID("TIAP2");
                    else
                        CNotice.printLogID("TIAP2");

                    return false;
                }

                if (m_dMeshLevelPercent <= 0.05f)
                {
                    if (bOpenNoticeDialog == true)
                        CNotice.noticeWarningID("TMSL");
                    else
                        CNotice.printLogID("TMSL");

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
    }

    // Static 객체인 환경설정 객체를 XML Serialization 하기 위해 임시로 사용하는 일반 클래스를 만들었다.
    public class CSettingDataClone
    {
        // 저장 변수들
        public string m_strWorkingDirName { get; set; }
        public string m_strFemmExeFileFullName { get; set; }

        // m_dMeshLevelPercent 이름을 변경하지말라. 다시 환경설정을 해야한다.
        public double m_dMeshLevelPercent { get; set; }
        public EMLanguage m_emLanguage { get; set; }

        public void copyCloneToSettingData()
        {
            CSettingData.m_strBaseWorkingDirPath = m_strWorkingDirName;
            CSettingData.m_strFemmExeFileFullName = m_strFemmExeFileFullName;
            CSettingData.m_dMeshLevelPercent = m_dMeshLevelPercent;
            CSettingData.m_emLanguage = m_emLanguage;
        }

        public void copySettingDataToClone()
        {
            m_strWorkingDirName = CSettingData.m_strBaseWorkingDirPath;
            m_strFemmExeFileFullName = CSettingData.m_strFemmExeFileFullName;
            m_dMeshLevelPercent = CSettingData.m_dMeshLevelPercent;
            m_emLanguage = CSettingData.m_emLanguage;
        }        
    }
}
