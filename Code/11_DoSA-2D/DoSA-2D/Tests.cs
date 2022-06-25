using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Properties Category
using System.ComponentModel;

using System.IO;

using Nodes;
using Scripts;
using gtLibrary;

namespace Tests
{

    //------------------------------------------------------------------------------------------
    // 측정조건에 대해 Node 들을 만들고 성능결과를 얻고 싶을 때 개발자에게 측정 조건의 입력을 요청한다 
    //------------------------------------------------------------------------------------------
    public class CTest : CDataNode
    {

        [DisplayNameAttribute("Mesh Size [%]"), CategoryAttribute("Condition Fields"), DescriptionAttribute("Mesh Size / Shape Length * 100")]
        public double MeshSizePercent { get; set; }

        public CTest()
        {
            // MeshSizePercent 와 이전 파일버전 인 경우는 아래의 값으로 초기화된다.
            MeshSizePercent = 2.0;
        }
    }

    public class CForceTest : CTest
    {
        private double m_dCurrent;

        [DisplayNameAttribute("Voltage [V]"), CategoryAttribute("\t\tCurrent Fields"), DescriptionAttribute("Input Voltage")]
        public double Voltage { get; set; }

        [DisplayNameAttribute("Max. Current [A]"), CategoryAttribute("\t\tCurrent Fields"), DescriptionAttribute("Maximum Input Current")]
        [ReadOnly(true)]
        public double Current
        {
            // 소수점 5째 자리 까지만 출력한다.
            get { return Math.Round(m_dCurrent, 5); }
            set { m_dCurrent = value; }
        }

        [DisplayNameAttribute("Moving Stroke [mm]"), CategoryAttribute("\tStroke Fields"), DescriptionAttribute("Moving Displacement")]
        public double MovingStroke { get; set; }

        public CForceTest()
        {
            KindKey = EMKind.FORCE_TEST;
            Voltage = 5.0;
        }

        // 파일스트림 객체에 코일 정보를 기록한다.
        // override 를 꼭 사용해야 가상함수가 아니라 현 함수가 호출된다.
        public override bool writeObject(StreamWriter writeStream, int nLevel)
        {
            try
            {
                CWriteFile writeFile = new CWriteFile();

                writeFile.writeBeginLine(writeStream, "ForceTest", nLevel);

                // CNode
                writeFile.writeDataLine(writeStream, "NodeName", NodeName, nLevel + 1);
                writeFile.writeDataLine(writeStream, "KindKey", KindKey, nLevel + 1);

                // CTest
                writeFile.writeDataLine(writeStream, "MeshSizePercent", MeshSizePercent, nLevel + 1);

                // CForceTest
                writeFile.writeDataLine(writeStream, "Voltage", Voltage, nLevel + 1);
                writeFile.writeDataLine(writeStream, "Current", Current, nLevel + 1);
                writeFile.writeDataLine(writeStream, "MovingStroke", MovingStroke, nLevel + 1);

                writeFile.writeEndLine(writeStream, "ForceTest", nLevel);
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);
                return false;
            }

            return true;
        }

        // 코일에 대한 문자열 라인을 넘겨 받아서 코일 객체를 초기화 한다.
        public bool readObject(List<string> listStringLines)
        {
            string strTemp;
            string[] arrayString;

            //if (KindKey != EMKind.FORCE_TEST)
            //{
            //    CNotice.printTrace("다른 객체를 ForceTest 로 읽으려고 하고 있습니다.");
            //    return false;
            //}

            try
            {
                foreach (string strLine in listStringLines)
                {
                    strTemp = strLine.Trim('\t');

                    arrayString = strTemp.Split('=');

                    if (arrayString.Length != 2)
                    {
                        CNotice.noticeWarningID("TIAP3");
                        return false;
                    }

                    switch (arrayString[0])
                    {
                        // CNode
                        case "NodeName":
                            NodeName = arrayString[1];
                            break;

                        case "KindKey":
                            // 하위 버전 호환 유지 ver(0.9.15.6)
                            if (arrayString[1] == "FORCE_EXPERIMENT")
                                arrayString[1] = "FORCE_TEST";

                            KindKey = (EMKind)Enum.Parse(typeof(EMKind), arrayString[1]);
                            break;

                        // CTest
                        case "MeshSizePercent":
                            MeshSizePercent = Convert.ToDouble(arrayString[1]);
                            break;

                        // CForceTest
                        case "Voltage":
                            Voltage = Convert.ToDouble(arrayString[1]);
                            break;

                        case "Current":
                            Current = Convert.ToDouble(arrayString[1]);
                            break;
                        
                        case "MovingStroke":
                            MovingStroke = Convert.ToDouble(arrayString[1]);
                            break;

                        default:
                            break;
                    }
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

    public class CStrokeTest : CTest
    {

        private double m_dCurrent;

        [DisplayNameAttribute("Voltage [V]"), CategoryAttribute("\t\tCurrent Fields"), DescriptionAttribute("Input Voltage")]
        public double Voltage { get; set; }

        [DisplayNameAttribute("Max. Current [A]"), CategoryAttribute("\t\tCurrent Fields"), DescriptionAttribute("Maximum Input Current")]
        [ReadOnly(true)]
        public double Current
        {
            // 소수점 5째 자리 까지만 출력한다.
            get { return Math.Round(m_dCurrent, 5); }
            set { m_dCurrent = value; }
        }

        [DisplayNameAttribute("Initial Stroke [mm]"), CategoryAttribute("\tStroke Fields"), DescriptionAttribute("Initial Displacement")]
        public double InitialStroke { get; set; }

        [DisplayNameAttribute("Final Stroke [mm]"), CategoryAttribute("\tStroke Fields"), DescriptionAttribute("Final Displacement")]
        public double FinalStroke { get; set; }

        [DisplayNameAttribute("Step Count"), CategoryAttribute("\tStroke Fields"), DescriptionAttribute("Step Count")]
        public int StepCount { get; set; }

        public CStrokeTest()
        {
            KindKey = EMKind.STROKE_TEST;
            InitialStroke = 0.0;
            FinalStroke = 0.1;
            StepCount = 5;
            Voltage = 5.0;
        }

        // 파일스트림 객체에 코일 정보를 기록한다.
        // override 를 꼭 사용해야 가상함수가 아니라 현 함수가 호출된다.
        public override bool writeObject(StreamWriter writeStream, int nLevel)
        {
            try 
            { 
                CWriteFile writeFile = new CWriteFile();

                writeFile.writeBeginLine(writeStream, "StrokeTest", nLevel);

                // CNode
                writeFile.writeDataLine(writeStream, "NodeName", NodeName, nLevel + 1);
                writeFile.writeDataLine(writeStream, "KindKey", KindKey, nLevel + 1);

                // CTest
                writeFile.writeDataLine(writeStream, "MeshSizePercent", MeshSizePercent, nLevel + 1);

                // CStrokeTest
                writeFile.writeDataLine(writeStream, "Voltage", Voltage, nLevel + 1);
                writeFile.writeDataLine(writeStream, "Current", Current, nLevel + 1);
                writeFile.writeDataLine(writeStream, "InitialStroke", InitialStroke, nLevel + 1);
                writeFile.writeDataLine(writeStream, "FinalStroke", FinalStroke, nLevel + 1);
                writeFile.writeDataLine(writeStream, "StepCount", StepCount, nLevel + 1);

                writeFile.writeEndLine(writeStream, "StrokeTest", nLevel);
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);
                return false;
            }

            return true;
        }

        // 코일에 대한 문자열 라인을 넘겨 받아서 코일 객체를 초기화 한다.
        public bool readObject(List<string> listStringLines)
        {
            string strTemp;
            string[] arrayString;

            //if (KindKey != EMKind.STROKE_TEST)
            //{
            //    CNotice.printTrace("다른 객체를 StrokeTest 로 읽으려고 하고 있습니다.");
            //    return false;
            //}

            try
            {
                foreach (string strLine in listStringLines)
                {
                    strTemp = strLine.Trim('\t');

                    arrayString = strTemp.Split('=');

                    if (arrayString.Length != 2)
                    {
                        CNotice.noticeWarningID("TIAP6");
                        return false;
                    }

                    switch (arrayString[0])
                    {
                        // CNode
                        case "NodeName":
                            NodeName = arrayString[1];
                            break;

                        case "KindKey":
                            // 하위 버전 호환 유지 ver(0.9.15.6)
                            if (arrayString[1] == "STROKE_EXPERIMENT")
                                arrayString[1] = "STROKE_TEST";

                            KindKey = (EMKind)Enum.Parse(typeof(EMKind), arrayString[1]);
                            break;

                        // CTest
                        case "MeshSizePercent":
                            MeshSizePercent = Convert.ToDouble(arrayString[1]);
                            break;

                        // CStrokeTest
                        case "Voltage":
                            Voltage = Convert.ToDouble(arrayString[1]);
                            break;

                        case "Current":
                            Current = Convert.ToDouble(arrayString[1]);
                            break;

                        case "InitialStroke":
                            InitialStroke = Convert.ToDouble(arrayString[1]);
                            break;

                        case "FinalStroke":
                            FinalStroke = Convert.ToDouble(arrayString[1]);
                            break;

                        case "StepCount":
                            StepCount = Convert.ToInt16(arrayString[1]);
                            break;

                        default:
                            break;
                    }
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

    public class CCurrentTest : CTest
    {

        [DisplayNameAttribute("Initial Current [A]"), CategoryAttribute("\t\tCurrent Fields"), DescriptionAttribute("Initial Current")]
        public double InitialCurrent { get; set; }

        [DisplayNameAttribute("Final Current [A]"), CategoryAttribute("\t\tCurrent Fields"), DescriptionAttribute("Final Current")]
        public double FinalCurrent { get; set; }

        [DisplayNameAttribute("Step Count"), CategoryAttribute("\t\tCurrent Fields"), DescriptionAttribute("Step Count")]
        public int StepCount { get; set; }

        [DisplayNameAttribute("Moving Stroke [mm]"), CategoryAttribute("\tStroke Fields"), DescriptionAttribute("Moving Displacement")]
        public double MovingStroke { get; set; }

        public CCurrentTest()
        {
            KindKey = EMKind.CURRENT_TEST;
            InitialCurrent = 0.0;
            FinalCurrent = 0.1;
            StepCount = 5;
            MovingStroke = 0.0;
        }

        // 파일스트림 객체에 코일 정보를 기록한다.
        // override 를 꼭 사용해야 가상함수가 아니라 현 함수가 호출된다.
        public override bool writeObject(StreamWriter writeStream, int nLevel)
        {
            try
            {
                CWriteFile writeFile = new CWriteFile();

                writeFile.writeBeginLine(writeStream, "CurrentTest", nLevel);

                // CNode
                writeFile.writeDataLine(writeStream, "NodeName", NodeName, nLevel + 1);
                writeFile.writeDataLine(writeStream, "KindKey", KindKey, nLevel + 1);

                // CTest
                writeFile.writeDataLine(writeStream, "MeshSizePercent", MeshSizePercent, nLevel + 1);

                // CCurrentTest
                writeFile.writeDataLine(writeStream, "InitialCurrent", InitialCurrent, nLevel + 1);
                writeFile.writeDataLine(writeStream, "FinalCurrent", FinalCurrent, nLevel + 1);
                writeFile.writeDataLine(writeStream, "StepCount", StepCount, nLevel + 1);
                writeFile.writeDataLine(writeStream, "MovingStroke", MovingStroke, nLevel + 1);

                writeFile.writeEndLine(writeStream, "CurrentTest", nLevel);
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);
                return false;
            }

            return true;
        }

        // 코일에 대한 문자열 라인을 넘겨 받아서 코일 객체를 초기화 한다.
        public bool readObject(List<string> listStringLines)
        {
            string strTemp;
            string[] arrayString;

            //if (KindKey != EMKind.CURRENT_TEST)
            //{
            //    CNotice.printTrace("다른 객체를 StrokeTest 로 읽으려고 하고 있습니다.");
            //    return false;
            //}

            try
            {
                foreach (string strLine in listStringLines)
                {
                    strTemp = strLine.Trim('\t');

                    arrayString = strTemp.Split('=');

                    if (arrayString.Length != 2)
                    {
                        CNotice.noticeWarningID("TIAP8");
                        return false;
                    }

                    switch (arrayString[0])
                    {
                        // CNode
                        case "NodeName":
                            NodeName = arrayString[1];
                            break;
                            
                        case "KindKey":
                            // 하위 버전 호환 유지 ver(0.9.15.6)
                            if (arrayString[1] == "CURRENT_EXPERIMENT")
                                arrayString[1] = "CURRENT_TEST";

                            KindKey = (EMKind)Enum.Parse(typeof(EMKind), arrayString[1]);
                            break;

                        // CTest
                        case "MeshSizePercent":
                            MeshSizePercent = Convert.ToDouble(arrayString[1]);
                            break;

                        // CCurrentTest
                        case "InitialCurrent":
                            InitialCurrent = Convert.ToDouble(arrayString[1]);
                            break;

                        case "FinalCurrent":
                            FinalCurrent = Convert.ToDouble(arrayString[1]);
                            break;

                        case "StepCount":
                            StepCount = Convert.ToInt16(arrayString[1]);
                            break;

                        case "MovingStroke":
                            MovingStroke = Convert.ToDouble(arrayString[1]);
                            break;
                        
                        default:
                            break;
                    }
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
}
