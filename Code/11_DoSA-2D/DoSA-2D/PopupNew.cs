using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using gtLibrary;
using Nodes;

using System.IO;

namespace DoSA
{
    public partial class PopupNew : Form
    {
        CManageFile m_manageFile = new CManageFile();

        private string m_strNewKind; 
        
        public string m_strName;

        public PopupNew(string strNewKind)
        {
            InitializeComponent();

            groupBoxNew.Text = "New " + strNewKind;
            labelName.Text = strNewKind + " Name :";

            m_strNewKind = strNewKind;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private bool isInputDataOK()
        {
            try
            {
                // 빈칸 확인으로 null 비교를 사용하지 말라. (.Length == 0 나 "" 를 사용하라)
                if (textBoxName.Text.Length == 0)
                {
                    CNotice.noticeWarningID("PEAN");
                    return false;
                }

                /// Design 생성이다.
                if (m_strNewKind == "Design")
                {
                    // 디자인을 무조건 프로그램 작업디렉토리에 생성하는 것으로 한다.
                    // 따라서 디자인을 생성할 때의 적용버튼 임으로 작업 디렉토리는 프로그램 작업 디렉토리를 사용하고 있다.
                    List<string> listDirectories = m_manageFile.getDirectoryList(CSettingData.m_strCurrentWorkingDirPath);

                    // 소문자로 비교하기 위해서 임시로 사용한다.
                    string strOldTempName, strNewTempName;

                    foreach (string directoryName in listDirectories)
                    {
                        // 디렉토리 경로에 GetFileName 을 사용하면 가장 마지막 디렉토리가 넘어온다.
                        strOldTempName = Path.GetFileName(directoryName).ToLower();
                        strNewTempName = m_strName.ToLower();

                        if (strOldTempName == strNewTempName)
                        {
                            // 기존 디자인이 이미 존재할 때 삭제하고 새롭게 시작할지를 물어 온다
                            DialogResult ret = CNotice.noticeWarningOKCancelID("TSDA", "W");

                            if (ret == DialogResult.OK)
                            {
                                m_manageFile.deleteDirectory(directoryName);
                                return true;
                            }
                            else
                                return false;
                        }
                    }
                }
                /// Test 들의 생성이다.        
                else
                {
                    /// 호출할 때 Owner 를 FormMain 으로 초기화 해야 확실하게 얻을 수 있다.
                    FormMain formMain = ((FormMain)this.Owner);

                    if (formMain == null)
                    {
                        CNotice.printTraceID("CNGM");
                        return false;
                    }

                    if (true == formMain.m_design.isExistNode(textBoxName.Text))
                    {
                        CNotice.noticeWarningID("IIAE");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return false;
            }

            return true;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // 검증전에 설정 되어야 함
            m_strName = textBoxName.Text;

            bool retOK = isInputDataOK();

            if (retOK == true)
                this.DialogResult = DialogResult.OK;
        }

        private void textBoxName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (System.Text.Encoding.UTF8.GetByteCount(new char[] { e.KeyChar }) > 1)
            {
                e.Handled = true;
            }
            /// 이름만 입력을 받는 Popup 창이기 때문에
            /// Enter 가 들어오면 OK Button 과 동일하게 처리한다
            else if (e.KeyChar == Convert.ToChar(Keys.Enter))
            {
                buttonOK.PerformClick();
            }
        }

    }
}
