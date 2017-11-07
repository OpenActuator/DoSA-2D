using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

// Caller Information
using System.Runtime.CompilerServices;

using System.IO;

namespace gtLibrary
{
    public enum EMOutputTarget
    {
        TRACE,
        MESSAGE_VIEW
    }

    public class CNotice
    {
        public delegate void LogEventHandler(EMOutputTarget target, string strMSG);
        public static event LogEventHandler Notice;

        public static void printTrace(string strMSG,
                [CallerMemberName] string functionName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int lineNumber = 0)
        {
            if (Notice != null)
            {
                string fileName = Path.GetFileName(sourceFilePath);

                strMSG = fileName + ", " + lineNumber + ", " + functionName + " : " + strMSG;

                Notice(EMOutputTarget.TRACE, strMSG);
            }
        }

        public static void printUserMessage(string strMSG)
        {
            if (Notice != null)
            {
                Notice(EMOutputTarget.MESSAGE_VIEW, strMSG);
            }
        }

        public static void noticeInfomation(string strMSG, string strTitle = "알림")
        {
            MessageBox.Show(strMSG, strTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void noticeWarning(string strMSG, string strTitle = "경고")
        {
            MessageBox.Show(strMSG, strTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void noticeError(string strMSG, string strTitle = "에러")
        {
            MessageBox.Show(strMSG, strTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static DialogResult noticeWarningOKCancel(string strMSG, string strTitle = "경고")
        {
            return MessageBox.Show(strMSG, strTitle, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
        }

        public static DialogResult noticeWarningYesNoCancel(string strMSG, string strTitle = "경고")
        {
            return MessageBox.Show(strMSG, strTitle, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
        }

    }
}
