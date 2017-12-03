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
using System.Resources;
using System.Windows.Forms;

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
                    ResourceManager resManager = ResourceManager.CreateFileBasedResourceManager("LanguageResource", Application.StartupPath, null);

                    CNotice.printTrace(resManager.GetString("TIAA2") + strFileFullPathName + resManager.GetString("_TDNE"));
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
                    ResourceManager resManager = ResourceManager.CreateFileBasedResourceManager("LanguageResource", Application.StartupPath, null);

                    CNotice.printTrace(resManager.GetString("TIAA") + sourceFileFullPathName + resManager.GetString("_TDNE"));
                    return false;
                }

                if (!isExistDirectory(destDirName))
                    createDirectory(destDirName);

                // 이미 파일이 존재하면 복사를 취소 한다.
                if (true == isExistFile(destFileFullPathName) && bOverWrite == false)
                {
                    ResourceManager resManager = ResourceManager.CreateFileBasedResourceManager("LanguageResource", Application.StartupPath, null);

                    CNotice.printTrace(resManager.GetString("TIAA4") + destFileFullPathName + resManager.GetString("_TDNE"));
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
                    ResourceManager resManager = ResourceManager.CreateFileBasedResourceManager("LanguageResource", Application.StartupPath, null);

                    CNotice.printTrace(resManager.GetString("TIAA3") + dirPath + resManager.GetString("_DITD"));
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
                    ResourceManager resManager = ResourceManager.CreateFileBasedResourceManager("LanguageResource", Application.StartupPath, null);

                    CNotice.printTrace(resManager.GetString("TIAA1") + dirPath + resManager.GetString("_TAE"));
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
                    ResourceManager resManager = ResourceManager.CreateFileBasedResourceManager("LanguageResource", Application.StartupPath, null);

                    CNotice.printTrace(resManager.GetString("TIAA2") + dirPath + resManager.GetString("_TDNE"));
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
