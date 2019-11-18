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

namespace Experiments
{

    //------------------------------------------------------------------------------------------
    // 측정조건에 대해 Node 들을 만들고 성능결과를 얻고 싶을 때 개발자에게 측정 조건의 입력을 요청한다 
    //------------------------------------------------------------------------------------------
    public class CExperiment : CNode
    {
        private double m_dCurrent;

        [DisplayNameAttribute("Voltage [V]"), CategoryAttribute("\t\tInput Fields"), DescriptionAttribute("Input Voltage")]
        public double Voltage { get; set; }

        [DisplayNameAttribute("Max. Current [A]"), CategoryAttribute("\t\tInput Fields"), DescriptionAttribute("Maximum Input Current")]
        [ReadOnly(true)]
        public double Current 
        {
            // 소수점 5째 자리 까지만 출력한다.
            get { return Math.Round(m_dCurrent, 5); }
            set { m_dCurrent = value; }
        }

    }

    public class CForceExperiment : CExperiment
    {
        [DisplayNameAttribute("Moving Displacement [mm]"), CategoryAttribute("Stroke Fields"), DescriptionAttribute("Moving Displacement")]
        public double MovingStroke { get; set; }

        public CForceExperiment()
        {
            m_kindKey = EMKind.FORCE_EXPERIMENT;
        }

        // 파일스트림 객체에 코일 정보를 기록한다.
        // override 를 꼭 사용해야 가상함수가 아니라 현 함수가 호출된다.
        public override bool writeObject(StreamWriter writeStream)
        {
            try
            {
                CWriteFile writeFile = new CWriteFile();

                writeFile.writeBeginLine(writeStream, "ForceExperiment", 2);

                // CNode
                writeFile.writeDataLine(writeStream, "NodeName", NodeName, 3);

                // CExperiment
                writeFile.writeDataLine(writeStream, "Voltage", Voltage, 3);
                writeFile.writeDataLine(writeStream, "Current", Current, 3);

                // CForceExperiment
                writeFile.writeDataLine(writeStream, "MovingStroke", MovingStroke, 3);

                writeFile.writeEndLine(writeStream, "ForceExperiment", 2);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return false;
            }

            return true;
        }

        // 코일에 대한 문자열 라인을 넘겨 받아서 코일 객체를 초기화 한다.
        public bool readObject(List<string> listStringLines)
        {
            string strTemp;
            string[] arrayString;

            //if (KindKey != EMKind.FORCE_EXPERIMENT)
            //{
            //    CNotice.printTrace("다른 객체를 ForceExperiment 로 읽으려고 하고 있습니다.");
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

                        // CExperiment
                        case "Voltage":
                            Voltage = Convert.ToDouble(arrayString[1]);
                            break;
                        case "Current":
                            Current = Convert.ToDouble(arrayString[1]);
                            break;

                        // CForceExperiment
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
                CNotice.printTrace(ex.Message);
                return false;
            }

            return true;
        }
    }

    public class CStrokeExperiment : CExperiment
    {

        [DisplayNameAttribute("Initial Displacement [mm]"), CategoryAttribute("Stroke Fields"), DescriptionAttribute("Initial Displacement")]
        public double InitialStroke { get; set; }

        [DisplayNameAttribute("Final Displacement [mm]"), CategoryAttribute("Stroke Fields"), DescriptionAttribute("Final Displacement")]
        public double FinalStroke { get; set; }

        [DisplayNameAttribute("Step Count"), CategoryAttribute("Stroke Fields"), DescriptionAttribute("Step Count")]
        public int StepCount { get; set; }

        public CStrokeExperiment()
        {
            m_kindKey = EMKind.STROKE_EXPERIMENT;
        }

        // 파일스트림 객체에 코일 정보를 기록한다.
        // override 를 꼭 사용해야 가상함수가 아니라 현 함수가 호출된다.
        public override bool writeObject(StreamWriter writeStream)
        {
            try 
            { 
                CWriteFile writeFile = new CWriteFile();

                writeFile.writeBeginLine(writeStream, "StrokeExperiment", 2);

                // CNode
                writeFile.writeDataLine(writeStream, "NodeName", NodeName, 3);

                // CExperiment
                writeFile.writeDataLine(writeStream, "Voltage", Voltage, 3);
                writeFile.writeDataLine(writeStream, "Current", Current, 3);

                // CStrokeExperiment
                writeFile.writeDataLine(writeStream, "InitialStroke", InitialStroke, 3);
                writeFile.writeDataLine(writeStream, "FinalStroke", FinalStroke, 3);
                writeFile.writeDataLine(writeStream, "StepCount", StepCount, 3);

                writeFile.writeEndLine(writeStream, "StrokeExperiment", 2);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return false;
            }

            return true;
        }

        // 코일에 대한 문자열 라인을 넘겨 받아서 코일 객체를 초기화 한다.
        public bool readObject(List<string> listStringLines)
        {
            string strTemp;
            string[] arrayString;

            //if (KindKey != EMKind.STROKE_EXPERIMENT)
            //{
            //    CNotice.printTrace("다른 객체를 StrokeExperiment 로 읽으려고 하고 있습니다.");
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

                        // CExperiment
                        case "Voltage":
                            Voltage = Convert.ToDouble(arrayString[1]);
                            break;
                        case "Current":
                            Current = Convert.ToDouble(arrayString[1]);
                            break;

                        // CStrokeExperiment
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
                CNotice.printTrace(ex.Message);
                return false;
            }

            return true;
        }

    }

    public class CCurrentExperiment : CExperiment
    {
        /// <summary>
        ///  CCurrentExperiment 에서는 상위 CExperiment 의 Voltage와 Current 를 사용하지 않고 있다.
        ///  따라서 프로퍼티에서 숨기기 위해서 재선언을 하고 Brawsable 을 False 로 했는데,
        ///  상속된 변수를 숨김에 문제가 발생한다는 CS0108 경고가 발생해서 new 한정자를 추가해 새로운 변수임을 알려서 문제를 해결하였다
        ///  단, praviate 선언을 하면 숨기기가 되지 않는다.
        /// </summary>
        /// ----------------------------------------------
        [BrowsableAttribute(false)]
        public new double Voltage { get; set; }
        [BrowsableAttribute(false)]
        public new double Current { get; set; }
        //-------------------------------------------------


        [DisplayNameAttribute("Moving Displacement [mm]"), CategoryAttribute("Stroke Fields"), DescriptionAttribute("Moving Displacement")]
        public double MovingStroke { get; set; }

        [DisplayNameAttribute("Initial Current [A]"), CategoryAttribute("Current Fields"), DescriptionAttribute("Initial Current")]
        public double InitialCurrent { get; set; }

        [DisplayNameAttribute("Final Current [A]"), CategoryAttribute("Current Fields"), DescriptionAttribute("Final Current")]
        public double FinalCurrent { get; set; }

        [DisplayNameAttribute("Step Count"), CategoryAttribute("Current Fields"), DescriptionAttribute("Step Count")]
        public int StepCount { get; set; }

        public CCurrentExperiment()
        {
            m_kindKey = EMKind.CURRENT_EXPERIMENT;
        }

        // 파일스트림 객체에 코일 정보를 기록한다.
        // override 를 꼭 사용해야 가상함수가 아니라 현 함수가 호출된다.
        public override bool writeObject(StreamWriter writeStream)
        {
            try
            {
                CWriteFile writeFile = new CWriteFile();

                writeFile.writeBeginLine(writeStream, "CurrentExperiment", 2);

                // CNode
                writeFile.writeDataLine(writeStream, "NodeName", NodeName, 3);

                // CCurrentExperiment
                writeFile.writeDataLine(writeStream, "InitialCurrent", InitialCurrent, 3);
                writeFile.writeDataLine(writeStream, "FinalCurrent", FinalCurrent, 3);
                writeFile.writeDataLine(writeStream, "StepCount", StepCount, 3);

                // CCurrentExperiment
                writeFile.writeDataLine(writeStream, "MovingStroke", MovingStroke, 3);


                writeFile.writeEndLine(writeStream, "CurrentExperiment", 2);
            }
            catch (Exception ex)
            {
                CNotice.printTrace(ex.Message);
                return false;
            }

            return true;
        }

        // 코일에 대한 문자열 라인을 넘겨 받아서 코일 객체를 초기화 한다.
        public bool readObject(List<string> listStringLines)
        {
            string strTemp;
            string[] arrayString;

            //if (KindKey != EMKind.CURRENT_EXPERIMENT)
            //{
            //    CNotice.printTrace("다른 객체를 StrokeExperiment 로 읽으려고 하고 있습니다.");
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

                        // CStrokeExperiment
                        case "InitialCurrent":
                            InitialCurrent = Convert.ToDouble(arrayString[1]);
                            break;
                        case "FinalCurrent":
                            FinalCurrent = Convert.ToDouble(arrayString[1]);
                            break;
                        case "StepCount":
                            StepCount = Convert.ToInt16(arrayString[1]);
                            break;
                        // CForceExperiment
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
                CNotice.printTrace(ex.Message);
                return false;
            }

            return true;
        }
    }
}
