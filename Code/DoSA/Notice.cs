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
            
            //---------------------------------------------------------
            // 초기 설정 사용 예제
            //---------------------------------------------------------
            //
            //// 출력방향을 결정함 (아래 코드가 동작하면 파일 출력, 동작하지 않으면 Output 창 출력)
            //Trace.Listeners.Add(new TextWriterTraceListener(Path.Combine(CSettingData.m_strProgramDirName, "Log", DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + ".Log")));

            //// 내부함수인 printLogEvent() 의 함수포인트를 사용해서 이벤트 함수를 설정한다
            //// printLogEvent 는 함수포인트이다
            //CNotice.Notice += printLogEvent;
            //---------------------------------------------------------


            //---------------------------------------------------------
            // 호출되는 함수의 예제
            //---------------------------------------------------------
            //
            //// 이벤트 발생 때 호출되는 함수
            //void printLogEvent(emOutputTarget target, string strMSG)
            //{
            //    if (target == emOutputTarget.Trace)
            //    {
            //        Trace.WriteLine(DateTime.Now.ToString() + ", " + strMSG);
            //        Trace.Flush();
            //    }
            //    else
            //    {
            //        listViewMessageWindow.addItem(strMSG);
            //    }
            //}
            //---------------------------------------------------------
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

        public static void noticeWarning(string strMSG, string strTitle = "경고", bool bTopMost = true)
        {
            // 최상위로 띄우면 시작 바에서 프로그램이 실행되었는지를 표시가 되지 않는 문제가 있어서 사용을 보유한다.
            //if(bTopMost == true)
            //    MessageBox.Show(new Form { TopMost = true }, strMSG, strTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //else
                MessageBox.Show(strMSG, strTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void noticeError(string strMSG, string strTitle = "에러", bool bTopMost = true)
        {
            // 최상위로 띄우면 시작 바에서 프로그램이 실행되었는지를 표시가 되지 않는 문제가 있어서 사용을 보유한다.
            //if (bTopMost == true)
            //    MessageBox.Show(new Form { TopMost = true }, strMSG, strTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //else
                MessageBox.Show(strMSG, strTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static DialogResult noticeWarningOKCancel(string strMSG, string strTitle = "경고", bool bTopMost = true)
        {
            // 최상위로 띄우면 시작 바에서 프로그램이 실행되었는지를 표시가 되지 않는 문제가 있어서 사용을 보유한다.
            //if (bTopMost == true)
            //    return MessageBox.Show(new Form { TopMost = true }, strMSG, strTitle, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            //else
                return MessageBox.Show(strMSG, strTitle, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

        }

        public static DialogResult noticeWarningYesNoCancel(string strMSG, string strTitle = "경고", bool bTopMost = true)
        {
            // 최상위로 띄우면 시작 바에서 프로그램이 실행되었는지를 표시가 되지 않는 문제가 있어서 사용을 보유한다.
            //if (bTopMost == true)
            //    return MessageBox.Show(new Form { TopMost = true }, strMSG, strTitle, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            //else
                return MessageBox.Show(strMSG, strTitle, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
        }

    }
}
