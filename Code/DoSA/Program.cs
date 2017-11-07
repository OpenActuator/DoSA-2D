using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using gtLibrary;

namespace DoSA
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }


    public static class CSettingData
    {
        static CManageFile m_manageFile = new CManageFile();

        // 저장 변수들
        public static string m_strWorkingDirName { get; set; }
        public static string m_strFemmExeFileFullName { get; set; }

        //public static string m_strMaxwellMaterialDirName { get; set; }
        //public static bool m_bShowProperyGridCollapse { get; set; }

		// 내부 사용변수
		// - 프로그램이 실행될때 초기화하여 내부에서 사용한다.
		public static string m_strProgramDirName { get; set; }

        public static bool verifyData(bool bOpenNoticeDialog = true)
        {
            bool bCheck = false;

            bCheck = m_manageFile.isExistFile(m_strFemmExeFileFullName);
            if (bCheck == false)
            {
                if (bOpenNoticeDialog == true)
                    CNotice.noticeWarning("FEMM 실행파일이 존재하지 않습니다.");
                else
                    CNotice.printTrace("FEMM 실행파일이 존재하지 않습니다.");

                return false;
            }

            bCheck = m_manageFile.isExistDirectory(m_strWorkingDirName);

            if (bCheck == false)
            {
                if (bOpenNoticeDialog == true)
                    CNotice.noticeWarning("기본 작업 디렉토리가 존재하지 않습니다.");
                else
                    CNotice.printTrace("기본 작업 디렉토리가 존재하지 않습니다.");

                return false;
            }
            
            bCheck = m_manageFile.isExistDirectory(m_strProgramDirName);

            if (bCheck == false)
            {
                if (bOpenNoticeDialog == true)
                    CNotice.noticeWarning("프로그램 실행 디렉토리에 문제가 있습니다.");
                else
                    CNotice.printTrace("프로그램 실행 디렉토리에 문제가 있습니다.");

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
        public bool m_bShowProperyGridCollapse { get; set; }

        public void copyCloneToSettingData()
        {
            CSettingData.m_strWorkingDirName = m_strWorkingDirName;
            CSettingData.m_strFemmExeFileFullName = m_strFemmExeFileFullName;
        }

        public void copySettingDataToClone()
        {
            m_strWorkingDirName = CSettingData.m_strWorkingDirName;
            m_strFemmExeFileFullName = CSettingData.m_strFemmExeFileFullName;
        }
    }
}
