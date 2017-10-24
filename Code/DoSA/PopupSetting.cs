﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Xml.Serialization;
using gtLibrary;

namespace DoSA
{
    public partial class PopupSetting : Form
    {
        CManageFile m_manageFile = new CManageFile();

        public PopupSetting()
        {
            InitializeComponent();

            // Setting Form 으로 올린다.
            uploadSettingData();
        }

        private void buttonSettingOK_Click(object sender, EventArgs e)
        {
            bool bCheck;

            // CSettingData 으로 내린다.
            downloadSettingData();

            bCheck = CSettingData.verifyData();

            if (bCheck == true)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void downloadSettingData()
        {
            CSettingData.m_strWorkingDirName = textBoxWorkingDirectory.Text;
            CSettingData.m_strFemmExeFileFullName = textBoxFemmPath.Text;

            //Optional항목 보일때 셋팅
            //CSettingData.m_bShowProperyGridCollapse = checkBoxProperyGridCollapse.Checked;
        }

        private void uploadSettingData()
        {
            textBoxWorkingDirectory.Text = CSettingData.m_strWorkingDirName;
            textBoxFemmPath.Text = CSettingData.m_strFemmExeFileFullName;

            //checkBoxProperyGridCollapse.Checked = CSettingData.m_bShowProperyGridCollapse;
        }

        private void buttonSettingCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonSelectWorkingDirectory_Click(object sender, EventArgs e)
        {
            // 폴더 선택창 띄우기
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBoxWorkingDirectory.Text = dialog.SelectedPath;
            }
        }

        private void buttonSelectFemmPath_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // 파일 열기창 설정
            openFileDialog.Title = "Select a Exe File";
            openFileDialog.FileName = null;
            openFileDialog.Filter = "FEMM EXE files|femm.exe|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
                this.textBoxFemmPath.Text = openFileDialog.FileName;
        }

        public bool saveSettingToFile()
        {
            string strSettingFileFullName = Path.Combine(CSettingData.m_strProgramDirName, "setting.ini");

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(CSettingDataClone));
                StreamWriter writer = new StreamWriter(strSettingFileFullName);

                // Static 객체는 XML Serialize 가 불가능해서 일반 Clone 객체에 복사를 하고 Serialize 를 하고 있다. 
                CSettingDataClone settingData = new CSettingDataClone();
                settingData.copySettingDataToClone();

                xmlSerializer.Serialize(writer, settingData);
                writer.Close();
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
            }

            return true;
        }

        public bool loadSettingFromFile()
        {
            string strSettingFileFullName = Path.Combine(CSettingData.m_strProgramDirName, "setting.ini");

            // CSettingData.ProgramDirectory 가 초기화 되어 있어야 한다.
            if (m_manageFile.isExistFile(strSettingFileFullName) == false)
            {
                CNotice.noticeWarning("환경 설정파일이 존재하지 않습니다.");
                return false;
            }

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(CSettingDataClone));
                StreamReader reader = new StreamReader(strSettingFileFullName);

                CSettingDataClone settingDataClone = new CSettingDataClone();
                settingDataClone = (CSettingDataClone)xmlSerializer.Deserialize(reader);

                settingDataClone.copyCloneToSettingData();

                reader.Close();

                // Setting Data 객체에 담은 정보를 Form 으로 올린다.
                uploadSettingData();
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                CNotice.printTrace("환경변수를 읽을 때 예외가 발생했습니다.");
            }
            
            return true;
        }
    }
}