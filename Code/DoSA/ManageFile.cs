using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using System.Windows.Forms;

// 파일 처리
using System.IO;

// Debugging
using System.Diagnostics;

namespace gtLibrary
{
    public class CManageFile
    {

        #region File - 체크(사용여부,존재여부),복사,삭제,이동

        public bool deleteFile(string strFileFullPathName)
        {
            try
            {
                if (false == isExistFile(strFileFullPathName))
                {
                    CNotice.printTrace("존재하지 않는 " + strFileFullPathName + " 를 삭제할려고 합니다.");
                    return false;
                }

                File.Delete(strFileFullPathName);
                return true;

            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return false;
            }
        }

        public bool isExistFile(string fileFullPathName)
        {
            try
            {
                return File.Exists(fileFullPathName);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return false;
            }
        }

        public bool copyFile(string sourceFileFullPathName, string destFileFullPathName, bool bOverWrite = false)
        {
            try
            {
                string destDirName = Path.GetDirectoryName(destFileFullPathName);

                if (false == isExistFile(sourceFileFullPathName))
                {
                    CNotice.printTrace("존재하지 않는 " + sourceFileFullPathName + " 를 복사하려고 합니다.");
                    return false;
                }

                if (!isExistDirectory(destDirName))
                    createDirectory(destDirName);

                // 이미 파일이 존재하면 복사를 취소 한다.
                if (true == isExistFile(destFileFullPathName) && bOverWrite == false)
                {
                    CNotice.printTrace("존재하지 않는 " + destFileFullPathName + " 를 덮어 쓰기를 하려고 합니다.");
                    return false;
                }

                File.Copy(sourceFileFullPathName, destFileFullPathName, bOverWrite);

                return true;
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return false;
            }
        }

        #endregion

        #region Direcotry - 체크(사용여부,존재여부),생성,복사,삭제,이동,파일리스트

        public List<string> getDirectoryList(string dirPath)
        {
            try
            {
                if (false == isExistDirectory(dirPath))
                {
                    CNotice.printTrace("존재하지 않는 " + dirPath + " 의 내부 디렉토리정보를 얻으려고 합니다.");
                    return null;
                }

                List<string> lsDirs = new List<string>();
                lsDirs = Directory.GetDirectories(dirPath).Cast<string>().ToList();

                return lsDirs;
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return null;
            }
        }

        public bool isExistDirectory(string dirPath)
        {
            try
            {
                return Directory.Exists(dirPath);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return false;
            }
        }

        public bool setCurrentDirectory(string strDirectory)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(strDirectory);
                return true;
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return false;
            }
        }

        public bool createDirectory(string dirPath)
        {
            try
            {
                if (true == isExistDirectory(dirPath))
                {
                    CNotice.printTrace("존재하는 " + dirPath + " 를 생성하려고 합니다.");
                    return false;
                }

                Directory.CreateDirectory(dirPath);
                return true;

            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return false;
            }
        }

		public bool deleteDirectory(string dirPath)
		{
			try
			{
				if (false == isExistDirectory(dirPath))
				{
					CNotice.printTrace("존재하지 않는 " + dirPath + " 를 삭제할려고 합니다.");
					return false;
				}

				Directory.Delete(dirPath, true);
				return true;
			}
			catch (Exception ex)
			{
				CNotice.printTrace(ex.Message);
				return false;
			}
        }

        #endregion

    }
}
