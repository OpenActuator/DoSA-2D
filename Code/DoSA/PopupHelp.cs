using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using gtLibrary;

namespace DoSA
{
    public partial class PopupHelp : Form
    {
        CManageFile m_manageFile = new CManageFile();

        public PopupHelp()
        {
            InitializeComponent();
        }

        private void buttonHelpClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonDoSAUserGuide_Click(object sender, EventArgs e)
        {
            string strHelpFileFullName = Path.Combine(CSettingData.m_strProgramDirName, "Help", "DoSA_User_Guide.pdf");

            if (m_manageFile.isExistFile(strHelpFileFullName) == false)
            {
                CNotice.noticeWarning("도움말 파일이 존재하지 않습니다.\nDoSA 디렉토리 > Help > DoSA_User_Guide.pdf 를 확인하세요.");
                return;
            }

            System.Diagnostics.Process.Start(strHelpFileFullName);
        }

        private void buttonInstallGuide_Click(object sender, EventArgs e)
        {
            string strHelpFileFullName = Path.Combine(CSettingData.m_strProgramDirName, "Help", "DoSA_Install_Guide.pdf");

            if (m_manageFile.isExistFile(strHelpFileFullName) == false)
            {
                CNotice.noticeWarning("도움말 파일이 존재하지 않습니다.\nDoSA 디렉토리 > Help > DoSA_Install_Guide.pdf 를 확인하세요.");
                return;
            }

            System.Diagnostics.Process.Start(strHelpFileFullName);
        }

        private void buttonVCMGuide_Click(object sender, EventArgs e)
        {
            string strHelpFileFullName = Path.Combine(CSettingData.m_strProgramDirName, "Help", "VCM_Sample_Guide.pdf");

            if (m_manageFile.isExistFile(strHelpFileFullName) == false)
            {
                CNotice.noticeWarning("도움말 파일이 존재하지 않습니다.\nDoSA 디렉토리 > Help > VCM_Sample_Guide.pdf 를 확인하세요.");
                return;
            }

            System.Diagnostics.Process.Start(strHelpFileFullName);
        }

        private void buttonSolenoidGuide_Click(object sender, EventArgs e)
        {
            string strHelpFileFullName = Path.Combine(CSettingData.m_strProgramDirName, "Help", "Solenoid_Sample_Guide.pdf");

            if (m_manageFile.isExistFile(strHelpFileFullName) == false)
            {
                CNotice.noticeWarning("도움말 파일이 존재하지 않습니다.\nDoSA 디렉토리 > Help > Solenoid_Sample_Guide.pdf 를 확인하세요.");
                return;
            }

            System.Diagnostics.Process.Start(strHelpFileFullName);
        }

        private void buttonExampleDirectory_Click(object sender, EventArgs e)
        {
            string strSampleDirName = Path.Combine(CSettingData.m_strProgramDirName, "Sample");

            if (m_manageFile.isExistDirectory(strSampleDirName) == false)
            {
                CNotice.noticeWarning("샘플 파일 디렉토리가 존재하지 않습니다.\nDoSA 디렉토리 > Sample 디렉토리를 확인하세요.");
                return;
            }

            System.Diagnostics.Process.Start(strSampleDirName);
        }
    }
}
