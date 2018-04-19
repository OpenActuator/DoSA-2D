using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

// 파일 처리
using System.IO;

// Debugging
using System.Diagnostics;

// 함수 이름 읽어내기
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace gtLibrary
{
    public class CWriteFile
    {
        CManageFile m_manageFile = new CManageFile();

        #region =============== 파일 저장 함수들 ==================

        // 리스트의 문자열을 하나씩 한 라인으로 저장한다.
        public bool writeLineString(string strFileFullName, List<string> listString, bool bOberwrite = false)
        {
            string[] arrayString = new string[listString.Count];

            try
            {
                if (true == m_manageFile.isExistFile(strFileFullName) && bOberwrite == false)
                {
                    ResourceManager resManager = ResourceManager.CreateFileBasedResourceManager("LanguageResource", Application.StartupPath, null);

                    CNotice.printTrace(resManager.GetString("TIAA6") + strFileFullName + resManager.GetString("_TAE"));
                    return false;
                }

                // 생성할 파일의 디렉토리가 존제하지 않으면 우선 디렉토리를 생성한다.
                if (false == m_manageFile.isExistDirectory(Path.GetDirectoryName(strFileFullName)))
                {
                    m_manageFile.createDirectory(Path.GetDirectoryName(strFileFullName));
                }

                // 기존에 파일이 존재하면 삭제후 쓰기를 한다.
                if (m_manageFile.isExistFile(strFileFullName) == true)
                {
                    m_manageFile.deleteFile(strFileFullName);
                    Thread.Sleep(50);
                }

                for (int i = 0; i < listString.Count; i++)
                {
                    arrayString.SetValue(listString.ElementAt(i), i);
                }

                File.WriteAllLines(strFileFullName, arrayString);

                return true;

            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return false;
            }
        }

        #endregion

        #region =============== 작업 저장파일용 함수들 ==================

        internal void writeBeginLine(StreamWriter writeStream, string strCommand, int nTabCount)
        {
            string strTab = string.Empty;

            // 오류 방지
            if (writeStream == null)
                return;

            for (int i = 0; i < nTabCount; i++)
                strTab = strTab + "\t";

            writeStream.Write(strTab + "$begin "); writeStream.WriteLine("\"" + strCommand + "\"");
        }

        internal void writeEndLine(StreamWriter writeStream, string strCommand, int nTabCount)
        {
            string strTab = string.Empty;

            // 오류 방지
            if (writeStream == null)
                return;

            for (int i = 0; i < nTabCount; i++)
                strTab = strTab + "\t";

            writeStream.Write(strTab + "$end "); writeStream.WriteLine("\"" + strCommand + "\"");
        }

        internal void writeDataLine(StreamWriter writeStream, string strCommand, object strData, int nTabCount)
        {
            string strTab = string.Empty;

            // 오류 방지
            if (writeStream == null)
            {
                CNotice.printTraceID("TSWI");
                return;
            }

            // 오류 방지
            if (strData == null)
            {
                CNotice.printTraceID("TODI");
                return;
            }

            for (int i = 0; i < nTabCount; i++)
                strTab = strTab + "\t";

            writeStream.WriteLine(strTab + strCommand + "=" + strData.ToString());
        }

        #endregion

    }
    
    public class CReadFile
    {
        CManageFile m_manageFile = new CManageFile();

        #region =============== 읽기 함수들 ==================

        // 모든 라인을 List 에 담아서 리턴한다.
        public bool readAllLines(string strTargetFileFullName, ref List<string> listLines)
        {
            string[] arrayAllLine;

            try
            {
                if (false == m_manageFile.isExistFile(strTargetFileFullName))
                {
                    ResourceManager resManager = ResourceManager.CreateFileBasedResourceManager("LanguageResource", Application.StartupPath, null);

                    CNotice.printTrace(resManager.GetString("TIAA5") + strTargetFileFullName + resManager.GetString("_TDNE"));
                    return false;
                }

                arrayAllLine = File.ReadAllLines(strTargetFileFullName);

                // string Array 를 List 에 담는다.
                foreach (string strLine in arrayAllLine)
                {
                    listLines.Add(strLine);
                }

                return true;

            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return false;
            }
        }

        // 제목줄을 제외한 모든 행에서
        // 특정 열만을 List 에 담아서 리턴한다.
        public bool readCSVData(string strCSVFileFullName, int iColumeIndex, ref List<double> listData, int iExceptionLineCount)
        {
            string[] arrayAllLine;
            string[] arraySeperatedLine;

            int nLineCount = 0;

            try
            {
                if (false == m_manageFile.isExistFile(strCSVFileFullName))
                {
                    ResourceManager resManager = ResourceManager.CreateFileBasedResourceManager("LanguageResource", Application.StartupPath, null);

                    CNotice.printTrace(resManager.GetString("TIAA5") + strCSVFileFullName + resManager.GetString("_TDNE"));
                    return false;
                }

                arrayAllLine = File.ReadAllLines(strCSVFileFullName);

                // string Array 를 List 에 담는다.
                foreach (string strLine in arrayAllLine)
                {
                    nLineCount++;

                    // CSV 상측의 제목줄을 삭제할 때 사용한다.
                    if (nLineCount > iExceptionLineCount)
                    {
                        arraySeperatedLine = strLine.Split(',');

                        if (arraySeperatedLine.Length <= iColumeIndex)
                        {
                            CNotice.printTraceID("TCII");
                            return false;
                        }

                        listData.Add(Convert.ToDouble(arraySeperatedLine[iColumeIndex]));
                    }
                }

                return true;

            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return false;
            }
        }

        // 특정라인을 뽑아낸다.
        public string getLine(string strTargetFileFullName, int iLineNum)
        {
            string[] arrayAllLine;
            int nLineCount = 0;

            try
            {
                if (false == m_manageFile.isExistFile(strTargetFileFullName))
                {
                    ResourceManager resManager = ResourceManager.CreateFileBasedResourceManager("LanguageResource", Application.StartupPath, null);
                    
                    CNotice.printTrace(resManager.GetString("TIAA5") + strTargetFileFullName + resManager.GetString("_TDNE"));
                    return "";
                }

                arrayAllLine = File.ReadAllLines(strTargetFileFullName);

                // string Array 를 List 에 담는다.
                foreach (string strLine in arrayAllLine)
                {
                    nLineCount++;

                    if (nLineCount == iLineNum)
                        return strLine;
                }
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
            }

            // 원하는 라인보다 파일의 길이가 짧은 경우 "" 이 리턴된다.
            return "";
        }

        // 해당 키워드를 만났을 때 
        // 해당 라인의 startPos 부터 endPos 사이의 문자열을 숫자로 만들어서 리턴한다.
        public string pickoutString(string strTargetFileFullName, string strKeyword, int startPos, int endPos)
        {
            string strLine, strTemp;
            int iLength;

            try
            {
                if (File.Exists(strTargetFileFullName) == false)
                    return null;

                if (startPos >= endPos)
                    CNotice.printTrace("The StartPos is greater then the EndPos in the GetData");

                System.IO.StreamReader readFile = new System.IO.StreamReader(strTargetFileFullName);

                while ((strLine = readFile.ReadLine()) != null)
                {
                    // Keyword 나 시작 Pos 보다는 strLine이 커야한다
                    if (strLine.Length > strKeyword.Length && strLine.Length > startPos)
                    {
                        strTemp = strLine.Substring(0, strKeyword.Length);

                        if (strTemp == strKeyword)
                        {
                            // 혹시 strLine 의 길이가 endPos 보다 작다면 
                            // 리턴하지 않고 강제로 endPos 크기를 strLine 길이로 변경한다
                            if (strLine.Length < endPos)
                                endPos = strLine.Length;

                            iLength = endPos - startPos;

                            strTemp = strLine.Substring(startPos, iLength);

                            readFile.Close();

                            return strTemp;
                        }
                    }
                }

                readFile.Close();

            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
            }

            return null;
        }

        #endregion


        #region =============== 작업 저장파일용 함수들 ==================

        // 구분의 시작행인 $begin 의 구분 명령어를 얻어온다.
        internal string isBeginLine(string strLine)
        {
            // 구분 Tab 을 삭제한다.
            string strTemp = strLine.Trim();
            string[] arraySplit;

            // 오류 방지
            if (strTemp.Length == 0)
                return null;

            if (strTemp[0] == '$')
            {
                arraySplit = strTemp.Split('\"');

                // 오류 방지
                if (arraySplit.Length <= 1)
                    return null;

                if (arraySplit[0] == "$begin ")
                    return arraySplit[1];
                else
                    return null;
            }
            else
                return null;
        }

        // 구분의 끝행인 $end 의 구분 명령어를 얻어온다.
        internal string isEndLine(string strLine)
        {
            // 구분 Tab 을 삭제한다.
            string strTemp = strLine.Trim();
            string[] arraySplit;

            // 오류 방지
            if (strTemp.Length == 0)
                return null;

            if (strTemp[0] == '$')
            {
                arraySplit = strTemp.Split('\"');

                // 오류 방지
                if (arraySplit.Length <= 1)
                    return null;

                if (arraySplit[0] == "$end ")
                    return arraySplit[1];
                else
                    return null;
            }
            else
                return null;
        }

        // 라인의 변수명칭과 데이터를 얻어온다
        internal bool readDataInLine(string strLine, ref string strCommand, ref string strData)
        {
            // 구분 Tab 을 삭제한다.
            string strTemp = strLine.Trim();
            string[] arraySplit;

            // 오류 방지
            if (strTemp.Length == 0)
                return false;

            // 데이터라인이 아닌 경우 바로 리턴한다
            if (strTemp[0] == '$')
                return false;

            arraySplit = strTemp.Split('=');

            // 오류 방지
            if (arraySplit.Length <= 1)
                return false;

            strCommand = arraySplit[0];
            strData = arraySplit[1];

            return true;
        }

        #endregion =============== 작업 저장파일용 함수들 ==================
    }

}
