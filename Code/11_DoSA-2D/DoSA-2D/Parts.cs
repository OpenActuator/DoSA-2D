using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Properties Category
using System.ComponentModel;

using Nodes;
using gtLibrary;
using Shapes;
using Scripts;

using System.IO;

namespace Parts
{
    public enum EMMagnetDirection 
    { 
        UP,
        DOWN,
        LEFT,
        RIGHT
    };

    public enum EMCurrentDirection
    {
        IN,
        OUT
    };

    public enum EMMoving
    {
        FIXED,
        MOVING
    };

    public enum EMCoilWireGrade
    {
        Enameled_IEC_Grade_1,
        Enameled_IEC_Grade_2,
        Enameled_IEC_Grade_3,
        Bonded_IEC_Grade_1B,
        Bonded_IEC_Grade_2B, 
        Enameled_JIS_Class_0,
        Enameled_JIS_Class_1,
        Enameled_JIS_Class_2,
        Enameled_JIS_Class_3,
        Bonded_JIS_Class_0,
        Bonded_JIS_Class_1,
        Bonded_JIS_Class_2
    };

    public class CShapeParts : CNode
    {
        protected string m_strMaterialName;

        protected CFace m_face = null;

        [BrowsableAttribute(false)]
        public CFace Face
        {
            get { return m_face; }
            set { m_face = value; }
        }

        [DisplayNameAttribute("Moving Parts"), CategoryAttribute("\t\tSpecification Fields"), DescriptionAttribute("Distinguishing between moving parts and fixed parts")]
        public EMMoving MovingPart { get; set; }        
 
        // 같은 strMaterial 을 가지고 두가지 형태로 접근하고 있다.
        //
        // 입출력이 필요한 하위 객체들은 Get, Set 용 Material 프로퍼티를 사용하고,
        // 출력만 필요한 전체 하위 객체들을 위해 표시되지 않는 Get 용 getMaterial() 메소드를 사용한다.
        public string getMaterial()
        {
            return m_strMaterialName;
        }

        public CShapeParts()
        {
            // 초기값은 고정된 것으로 가정함
            this.MovingPart = EMMoving.FIXED;
        }

    }

    public class CNonKind : CShapeParts
    {
        public CNonKind()
        {
            KindKey = EMKind.NON_KIND;
            m_strMaterialName = "Air";
        }

        // 파일스트림 객체에 코일 정보를 기록한다.
        // override 를 꼭 사용해야 가상함수가 아니라 현 함수가 호출된다.
        public override bool writeObject(StreamWriter writeStream, int nLevel)
        {
            try
            {
                CWriteFile writeFile = new CWriteFile();

                writeFile.writeBeginLine(writeStream, "Non-Kind", nLevel);

                // CNode
                writeFile.writeDataLine(writeStream, "NodeName", NodeName, nLevel + 1);
                writeFile.writeDataLine(writeStream, "KindKey", KindKey, nLevel + 1);

                // Non Kind 는 CNode 의 정보와 CShapParts 의 Face 정보만 필요하기 때문에
                // 나머지 정보는 저장하지 않는다.
                // 
                // CShapeParts
                //writeFile.writeDataLine(writeStream, "MovingParts", MovingPart, nLevel + 1);

                // CFace
                if (Face != null)
                {
                    Face.writeObject(writeStream, nLevel + 1);
                }

                writeFile.writeEndLine(writeStream, "Non-Kind", nLevel);

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
            CReadFile readFile = new CReadFile();
            string strTemp;
            string[] arrayString;

            List<string> listInformationLines = new List<string>();
            List<string> listShapeLines = new List<string>();

            bool bShapeLine = false;

            if (KindKey != EMKind.NON_KIND)
            {
                CNotice.printLog("NON_KIND 가 아닌 객체가 CNonKind 로 열리려고 한다.");
                return false;
            }

            try
            {
                // Shape 라인과 정보 라인을 분리한다.
                foreach (string strLine in listStringLines)
                {
                    if (readFile.isEndLine(strLine) == "Shape")
                        bShapeLine = false;

                    if (bShapeLine == true)
                        listShapeLines.Add(strLine);
                    else
                    {
                        if (readFile.isBeginLine(strLine) == "Shape")
                            bShapeLine = true;
                        else
                            listInformationLines.Add(strLine);
                    }
                }

                // 정보 라인을 처리한다.
                foreach (string strLine in listInformationLines)
                {
                    strTemp = strLine.Trim('\t');

                    arrayString = strTemp.Split('=');

                    /// 각 줄의 String 배열은 항상 2개이여야 한다.
                    if (arrayString.Length != 2)
                    {
                        CNotice.printLog("Non-Kind 데이터에 문제가 있습니다.");
                        return false;
                    }

                    switch (arrayString[0])
                    {
                        // CNode
                        case "NodeName":
                            NodeName = arrayString[1];
                            break;

                        case "KindKey":
                            KindKey = (EMKind)Enum.Parse(typeof(EMKind), arrayString[1]);
                            break;

                        // Non Kind 는 CNode 의 정보와 CShapParts 의 Face 정보만 필요하기 때문에
                        // 나머지 정보는 저장하지 않는다.
                        // 
                        // CShapeParts
                        //case "MovingParts":
                        //    MovingPart = (EMMoving)Enum.Parse(typeof(EMMoving), arrayString[1]);
                        //    break;

                        default:
                            // 해당사항이 없는 항목은 아무것도 하지 않는다. foreach 가 동작하기 때문에 return 해서는 않된다.
                            break;
                    }
                }

                // Shape 정보가 있는 경우만 m_face 를 생성하고 읽기 작업을 진행한다.
                if (listShapeLines.Count > 0)
                {
                    m_face = new CFace();

                    if (m_face == null)
                        CNotice.printLogID("IIAT");
                    else
                        m_face.readObject(listShapeLines);
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

    [DefaultPropertyAttribute("Turns")]
    public class CCoil : CShapeParts
    {
        // 상위 strMaterial 을 사용하기 때문에  { get; set; } 형식은 사용해서는 안된다
        // Property Change Event 에서 라벨이름으로 사용되고 있음을 주의하라
        [TypeConverter(typeof(CCoilWirePropertyConverter))]
        [DisplayNameAttribute("Part Material"), CategoryAttribute("\t\tSpecification Fields"), DescriptionAttribute("Name of part material")]
        public string MaterialName
        {
            get { return m_strMaterialName; }
            set { m_strMaterialName = value; }
        }

        [DisplayNameAttribute("Curent Direction"), CategoryAttribute("\t\tSpecification Fields"), DescriptionAttribute("Voltage Direction")]
        public EMCurrentDirection CurrentDirection { get; set; }

        [DisplayNameAttribute("Turns"), CategoryAttribute("\tCalculated Fields"), DescriptionAttribute("Coil Turns")]
        public int Turns { get; set; }

        private double m_dResistance;

        [DisplayNameAttribute("Resistance [Ω]"), CategoryAttribute("\tCalculated Fields"), DescriptionAttribute("Coil Resistance")]
        public double Resistance
        {
            // 소수점 5째 자리 까지만 출력한다.
            get { return Math.Round(m_dResistance, 5); }
            set { m_dResistance = value; }
        }

        private double m_dResistance_20;

        [ReadOnlyAttribute(true)]
        [DisplayNameAttribute("Resistance at 20℃ [Ω]"), CategoryAttribute("\tCalculated Fields"), DescriptionAttribute("Coil Resistance at 20℃")]
        public double Resistance_20
        {
            // 소수점 5째 자리 까지만 출력한다.
            get { return Math.Round(m_dResistance_20, 5); }
            set { m_dResistance_20 = value; }
        }

        [ReadOnlyAttribute(true)]
        [DisplayNameAttribute("Layers"), CategoryAttribute("\tCalculated Fields"), DescriptionAttribute("Number of coil layers")]
        public int Layers { get; set; }

        [ReadOnlyAttribute(true)]
        [DisplayNameAttribute("Turns of One Layer"), CategoryAttribute("\tCalculated Fields"), DescriptionAttribute("Turns of one layer")]
        public int TurnsOfOneLayer { get; set; }

        [DisplayNameAttribute("Coil Wire Grade"), CategoryAttribute("Design Fields (optional)"), DescriptionAttribute("Coil Wire Grade")]
        public EMCoilWireGrade  CoilWireGrade { get; set; }

        [ReadOnlyAttribute(true)]
        [DisplayNameAttribute("Inner Diameter [mm]"), CategoryAttribute("Design Fields (optional)"), DescriptionAttribute("Inner Diameter of coil")]
        public double InnerDiameter { get; set; }

        [ReadOnlyAttribute(true)]
        [DisplayNameAttribute("Outer Diameter [mm]"), CategoryAttribute("Design Fields (optional)"), DescriptionAttribute("Outer Diameter of coil")]
        public double OuterDiameter { get; set; }

        [ReadOnlyAttribute(true)]
        [DisplayNameAttribute("Coil Height [mm]"), CategoryAttribute("Design Fields (optional)"), DescriptionAttribute("Coil Height")]
        public double Height { get; set; }

        // Property Change Event 에서 라벨이름으로 사용되고 있음을 주의하라
        [DisplayNameAttribute("Copper Diameter [mm]"), CategoryAttribute("Design Fields (optional)"), DescriptionAttribute("Diameter of internal copper wire")]
        public double CopperDiameter { get; set; }
        
        private double m_dWireDiameter;

        [DisplayNameAttribute("Wire Diameter [mm]"), CategoryAttribute("Design Fields (optional)"), DescriptionAttribute("Outer diameter of enameled wire")]
        public double WireDiameter
        {
            // 소수점 5째 자리 까지만 출력한다.
            get { return Math.Round(m_dWireDiameter, 5); }
            set { m_dWireDiameter = value; }
        }

        [DisplayNameAttribute("Coil Temperature [℃]"), CategoryAttribute("Design Fields (optional)"), DescriptionAttribute("Coil Temperature")]
        public double Temperature { get; set; }

        [DisplayNameAttribute("Horizontal Coefficient"), CategoryAttribute("Design Fields (optional)"), DescriptionAttribute("Horizontal coefficient, [Bonded:0.95, Enameled:0.9]")]
        public double HorizontalCoefficient { get; set; }

        [DisplayNameAttribute("Vertical Coefficient"), CategoryAttribute("Design Fields (optional)"), DescriptionAttribute("Vertical Coefficient, [Bonded:1.13, Enameled:0.98]")]
        public double VerticalCoefficient { get; set; }

        [DisplayNameAttribute("Resistance Coefficient"), CategoryAttribute("Design Fields (optional)"), DescriptionAttribute("Coil Resistance Coefficient, [Bonded:1.1, Enameled:1.0]")]
        public double ResistanceCoefficient { get; set; }
       
        public CCoil()
        {
            KindKey = EMKind.COIL;
            m_strMaterialName = "Copper";            
                        
            CurrentDirection = EMCurrentDirection.IN;
            CoilWireGrade = EMCoilWireGrade.Enameled_IEC_Grade_2;

            // double 형의 PropertyGrid 값을 float 형로 초기화 하면 소수점 문제가 발생한다.
            // 계산중에 사용하는 초기화는 float 형으로 사용하고 있다.
            Temperature = 20.0;
            WireDiameter = 0.0;
            //
            // Enameled Coil 를 기본 설정값으로 하고 있다.
            HorizontalCoefficient = 0.9;
            VerticalCoefficient = 0.98;
            ResistanceCoefficient = 1.0;
        }

        public double calculateWireDiameter()
        {
            double a, b, c;

            switch (CoilWireGrade)
            {
                case EMCoilWireGrade.Enameled_IEC_Grade_1:
                    a = 1.0591106f;
                    b = 0.97731441f;
                    c = 0.97146019f;
                    break;
                
                case EMCoilWireGrade.Enameled_IEC_Grade_2:
                    a = 1.0594096f;
                    b = 1.0174494f;
                    c = 0.94036838f;
                    break;

                case EMCoilWireGrade.Enameled_IEC_Grade_3:
                    a = 1.0779866f;
                    b = 1.0306803f;
                    c = 0.91887626f;
                    break;
                
                case EMCoilWireGrade.Bonded_IEC_Grade_1B:
                    a = 1.0556977f;
                    b = 0.96503642f;
                    c = 0.98600866f;
                    break;
                
                case EMCoilWireGrade.Bonded_IEC_Grade_2B:
                    a = 1.0948313f;
                    b = 0.95022851f;
                    c = 0.97043003f;
                    break;
                
                case EMCoilWireGrade.Enameled_JIS_Class_0:
                    a = 0.85272126f;
                    b = 1.3328143f;
                    c = 0.79354723f;
                    break;
                
                case EMCoilWireGrade.Enameled_JIS_Class_1:
                    a = 0.91849177f;
                    b = 1.1785193f;
                    c = 0.86705218f;
                    break;
                
                case EMCoilWireGrade.Enameled_JIS_Class_2:
                    a = 0.91715613f;
                    b = 1.1725843f;
                    c = 0.89585176f;                    
                    break;
                
                case EMCoilWireGrade.Enameled_JIS_Class_3:
                    a = 0.93049414f;
                    b = 1.139631f;
                    c = 0.92093323f;                    
                    break;
                
                case EMCoilWireGrade.Bonded_JIS_Class_0:
                    a = 0.95818743f;
                    b = 1.0831926f;
                    c = 0.94759452f;                    
                    break;
                
                case EMCoilWireGrade.Bonded_JIS_Class_1:
                    a = 0.9962885f;
                    b = 1.0224635f;
                    c = 0.97657213f;                    
                    break;
                
                case EMCoilWireGrade.Bonded_JIS_Class_2:
                    a = 0.99195693f;
                    b = 1.0231506f;
                    c = 0.98189265f;                    
                    break;
                
                default:
                    // 해당사항이 없는 항목이 넘어 왔기 때문에 바로 retrun 해서 아래의 동작을 하지 않는다.
                    return 0;
            }

            return a * (Math.Pow(b, CopperDiameter) * Math.Pow(CopperDiameter, c));
        }

        public void designCoil()
        {
            if (InnerDiameter <= 0.0f)
            {
                CNotice.noticeWarningID("TIDV");
                return;
            }

            if (OuterDiameter <= 0.0f)
            {
                CNotice.noticeWarningID("TODV");
                return;
            }

            if (OuterDiameter <= InnerDiameter)
            {
                CNotice.noticeWarningID("ODSB");
                return;
            }

            if (Height <= 0.0f)
            {
                CNotice.noticeWarningID("CHVI");
                return;
            }

            if (CopperDiameter <= 0.0f)
            {
                CNotice.noticeWarningID("ETCD");
                return;
            }

            if (HorizontalCoefficient <= 0.0f)
            {
                CNotice.noticeWarningID("ETVF");
                return;
            }

            if (VerticalCoefficient <= 0.0f)
            {
                CNotice.noticeWarningID("ETVF2");
                return;
            }

            if (ResistanceCoefficient <= 0.0f)
            {
                CNotice.noticeWarningID("ETVF1");
                return;
            }

            try
            {
                //*****************************************************************************
                // 수식계산에서 발생하는 예외처리를 하라.
                //*****************************************************************************
                double dCoilAvgDiameter = (InnerDiameter + OuterDiameter) / 2.0f;
                double dWidth = Math.Abs(OuterDiameter - InnerDiameter) / 2.0f;

                // 동선경을 선택해서 WireDiameter 가 설정이 되지 않는 경우 여기서 자동 계산한다.
                if (WireDiameter == 0.0f)
                {
                    WireDiameter = calculateWireDiameter();
                }
                                
                int iHorizontal_N = (int)((1 / HorizontalCoefficient) * (dWidth / WireDiameter));
                int iVirtical_N = (int)((1 / VerticalCoefficient) * (Height / WireDiameter));

                // 수평 적층 수가 짝수가 아니면 한층을 뺀다. (짝수 정렬만 가능하다)
                if (iHorizontal_N % 2 != 0)
                    iHorizontal_N -= 1;

                this.Turns = iHorizontal_N * iVirtical_N;
                this.TurnsOfOneLayer = iVirtical_N;
                this.Layers = iHorizontal_N;

                double dWireLength_Meter = Math.PI * this.Turns * dCoilAvgDiameter / 1000.0;

                // IEC 317, 단위 저항 보간
                double res_a = 0.0, res_b = 0.0, res_c = 0.0;

                // 온도 계수
                double dTemperatureCoefficient = 0.004041f;

                if (m_strMaterialName == "Copper")
                {
                    // IEC 317, 단위 저항 보간
                    res_a = 0.021771473f;
                    res_b = 0.99730833f;
                    res_c = -1.9999322f;

                    // 온도 계수
                    dTemperatureCoefficient = 0.00393f;
                }
                // 두가지 밖에 없어서 알루미늄의 경우이다.
                else if(m_strMaterialName == "Aluminum")
                {
                    // IEC 317, 단위 저항 보간
                    res_a = 0.036438f;
                    res_b = 0.981116f;
                    res_c = -1.995774f;

                    // 온도 계수
                    dTemperatureCoefficient = 0.004308f;
                }
                else
                {
                    CNotice.printLogID("TIAP1");
                    return;
                }


                double dResistancePerMeter = res_a * (Math.Pow(res_b, CopperDiameter) * Math.Pow(CopperDiameter, res_c));

                Resistance_20 = dResistancePerMeter * dWireLength_Meter * ResistanceCoefficient;

                Resistance = Resistance_20 * (1 + dTemperatureCoefficient * (Temperature - 20.0f));
  
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }       
        }

        public bool initialShapeDesignValue()
        {
            if (Face == null)
            {
                CNotice.printLogID("YATT3");
                return false;
            }

            double minX = 0;
            double maxX = 0;
            double minY = 0;
            double maxY = 0;

            Face.getMinMaxX(ref minX, ref maxX);
            Face.getMinMaxY(ref minY, ref maxY);

            Height = maxY - minY;
            InnerDiameter = minX * 2.0f;
            OuterDiameter = maxX * 2.0f;

            return true;
        }

        // 파일스트림 객체에 코일 정보를 기록한다.
        // override 를 꼭 사용해야 가상함수가 아니라 현 함수가 호출된다.
        public override bool writeObject(StreamWriter writeStream, int nLevel)
        {
            try
            {
                CWriteFile writeFile = new CWriteFile();

                writeFile.writeBeginLine(writeStream, "Coil", nLevel);

                // CNode
                writeFile.writeDataLine(writeStream, "NodeName", NodeName, nLevel + 1);
                writeFile.writeDataLine(writeStream, "KindKey", KindKey, nLevel + 1);

                // CParts
                writeFile.writeDataLine(writeStream, "MovingParts", MovingPart, nLevel + 1);

                // CCoil
                writeFile.writeDataLine(writeStream, "Material", MaterialName, nLevel + 1);
                writeFile.writeDataLine(writeStream, "CurrentDirection", CurrentDirection, nLevel + 1);
                writeFile.writeDataLine(writeStream, "Turns", Turns, nLevel + 1);
                writeFile.writeDataLine(writeStream, "Resistance", Resistance, nLevel + 1);
                writeFile.writeDataLine(writeStream, "Resistance_20", Resistance_20, nLevel + 1);
                writeFile.writeDataLine(writeStream, "Layers", Layers, nLevel + 1);
                writeFile.writeDataLine(writeStream, "TurnsOfOneLayer", TurnsOfOneLayer, nLevel + 1);
                writeFile.writeDataLine(writeStream, "CoilWireGrade", CoilWireGrade, nLevel + 1);
                writeFile.writeDataLine(writeStream, "InnerDiameter", InnerDiameter, nLevel + 1);
                writeFile.writeDataLine(writeStream, "OuterDiameter", OuterDiameter, nLevel + 1);
                writeFile.writeDataLine(writeStream, "Height", Height, nLevel + 1);
                writeFile.writeDataLine(writeStream, "CopperDiameter", CopperDiameter, nLevel + 1);
                writeFile.writeDataLine(writeStream, "WireDiameter", WireDiameter, nLevel + 1);
                writeFile.writeDataLine(writeStream, "Temperature", Temperature, nLevel + 1);
                writeFile.writeDataLine(writeStream, "HorizontalCoefficient", HorizontalCoefficient, nLevel + 1);
                writeFile.writeDataLine(writeStream, "VerticalCoefficient", VerticalCoefficient, nLevel + 1);
                writeFile.writeDataLine(writeStream, "ResistanceCoefficient", ResistanceCoefficient, nLevel + 1);

                // CFace
                if (Face != null)
                {
                    Face.writeObject(writeStream, nLevel + 1);
                }
            
                writeFile.writeEndLine(writeStream, "Coil", nLevel);

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
            CReadFile readFile = new CReadFile();
            string strTemp;
            string[] arrayString;

            List<string> listInformationLines = new List<string>();
            List<string> listShapeLines = new List<string>();

            bool bShapeLine = false;

            if (KindKey != EMKind.COIL)
            {
                CNotice.printLogID("YATT7");

                return false;
            }      
    
            try
            {
                // Shape 라인과 정보 라인을 분리한다.
                foreach (string strLine in listStringLines)
                {
                    if (readFile.isEndLine(strLine) == "Shape")
                        bShapeLine = false;

                    if (bShapeLine == true)
                        listShapeLines.Add(strLine);
                    else
                    {
                        if (readFile.isBeginLine(strLine) == "Shape")
                            bShapeLine = true;
                        else
                            listInformationLines.Add(strLine);
                    }
                }

                // 정보 라인을 처리한다.
                foreach (string strLine in listInformationLines)
                {
                    strTemp = strLine.Trim('\t');

                    arrayString = strTemp.Split('=');
                  

                    switch (arrayString[0])
                    {
                        // CNode
                        case "NodeName":
                            NodeName = arrayString[1];
                            break;

                        case "KindKey":
                            KindKey = (EMKind)Enum.Parse(typeof(EMKind), arrayString[1]);
                            break;

                        // CParts
                        case "MovingParts":
                            MovingPart = (EMMoving)Enum.Parse(typeof(EMMoving), arrayString[1]);
                            break;

                        // CCoil
                        case "Material":
                            if (CMaterialListInFEMM.isCoilWIreInList(arrayString[1]) == true)
                            {
                                m_strMaterialName = arrayString[1];
                            }
                            else
                            {
                                // 현재의 버전에서 사용할 수 없는 재질이 존재한다면 공백으로 처리하고
                                // 동작 중에 공백을 사용해서 재질이 초기화 되지 않음을 확인한다.
                                m_strMaterialName = "";
                            }  
                            break;

                        case "CurrentDirection":
                            CurrentDirection = (EMCurrentDirection)Enum.Parse(typeof(EMCurrentDirection), arrayString[1]);
                            break;

                        case "Turns":
                            Turns = Convert.ToInt16(arrayString[1]);
                            break;

                        case "Resistance":
                            Resistance = Convert.ToDouble(arrayString[1]);
                            break;

                        case "Resistance_20":
                            Resistance_20 = Convert.ToDouble(arrayString[1]);
                            break;

                        case "Layers":
                            Layers = Convert.ToInt16(arrayString[1]);
                            break;

                        case "TurnsOfOneLayer":
                            TurnsOfOneLayer = Convert.ToInt16(arrayString[1]);
                            break;

                        case "CoilWireGrade":
                            CoilWireGrade = (EMCoilWireGrade)Enum.Parse(typeof(EMCoilWireGrade), arrayString[1]);
                            break;

                        case "InnerDiameter":
                            InnerDiameter = Convert.ToDouble(arrayString[1]);
                            break;

                        case "OuterDiameter":
                            OuterDiameter = Convert.ToDouble(arrayString[1]);
                            break;

                        case "Height":
                            Height = Convert.ToDouble(arrayString[1]);
                            break;

                        case "CopperDiameter":
                            CopperDiameter = Convert.ToDouble(arrayString[1]);
                            break;

                        case "WireDiameter":
                            WireDiameter = Convert.ToDouble(arrayString[1]);
                            break;

                        case "Temperature":
                            Temperature = Convert.ToDouble(arrayString[1]);
                            break;

                        case "HorizontalCoefficient":
                            HorizontalCoefficient = Convert.ToDouble(arrayString[1]);
                            break;

                        case "VerticalCoefficient":
                            VerticalCoefficient = Convert.ToDouble(arrayString[1]);
                            break;

                        case "ResistanceCoefficient":
                            ResistanceCoefficient = Convert.ToDouble(arrayString[1]);
                            break;

                        default:
                            // 해당사항이 없는 항목은 아무것도 하지 않는다. foreach 가 동작하기 때문에 return 해서는 않된다.
                            break;
                    }
                }

                // Shape 정보가 있는 경우만 m_face 를 생성하고 읽기 작업을 진행한다.
                if (listShapeLines.Count > 0)
                {
                    m_face = new CFace();
                    m_face.readObject(listShapeLines);
                }

            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return false;
            }  

            return true;
        }

        public void setBlockPropCurrent(CScriptFEMM femm, double dCurrent, double dMeshSize)
        {
            try
            {
                CPoint blockPoint = null;
                blockPoint = this.Face.getBlockPoint();

                if (blockPoint == null)
                {
                    CNotice.printLogID("NBPF");
                    return;
                }

                string strMaterialName = this.m_strMaterialName;

                string strCircuit = NodeName + "_current";

                if (CurrentDirection == EMCurrentDirection.OUT)
                    dCurrent = -dCurrent;

                femm.addCircuitProp(strCircuit, dCurrent);

                femm.setBlockProp(blockPoint, strMaterialName, dMeshSize, strCircuit, 0, MovingPart, Turns);
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }
    }

    public class CMagnet : CShapeParts
    {
        // 상위 strMaterial 을 사용하기 때문에  { get; set; } 형식은 사용해서는 안된다
        [TypeConverter(typeof(CMagnetPropertyConverter))]
        [DisplayNameAttribute("Part Material"), CategoryAttribute("\t\tSpecification Fields"), DescriptionAttribute("Name of Part Material")]
        public string MaterialName
        {
            get { return m_strMaterialName; }
            set { m_strMaterialName = value; }
        }

        [DisplayNameAttribute("Direction"), CategoryAttribute("\t\tSpecification Fields"), DescriptionAttribute("Determination of magnetization direction")]
        public EMMagnetDirection emMagnetDirection { get; set; }

        public CMagnet()
        {
            KindKey = EMKind.MAGNET;
            MaterialName = "N45";
        }

        // 파일스트림 객체에 코일 정보를 기록한다.
        // override 를 꼭 사용해야 가상함수가 아니라 현 함수가 호출된다.
        public override bool writeObject(StreamWriter writeStream, int nLevel)
        {
            try
            {
                CWriteFile writeFile = new CWriteFile();

                writeFile.writeBeginLine(writeStream, "Magnet", nLevel);

                // CNode
                writeFile.writeDataLine(writeStream, "NodeName", NodeName, nLevel + 1);
                writeFile.writeDataLine(writeStream, "KindKey", KindKey, nLevel + 1);

                // CParts
                writeFile.writeDataLine(writeStream, "MovingParts", MovingPart, nLevel + 1);

                // CMagnet
                writeFile.writeDataLine(writeStream, "Material", m_strMaterialName, nLevel + 1);
                writeFile.writeDataLine(writeStream, "MagnetDirection", emMagnetDirection, nLevel + 1);

                // CFace
                if (Face != null)
                {
                    Face.writeObject(writeStream, nLevel + 1);
                }            

                writeFile.writeEndLine(writeStream, "Magnet", nLevel);
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
            CReadFile readFile = new CReadFile();
            string strTemp;
            string[] arrayString;

            List<string> listInformationLines = new List<string>();
            List<string> listShapeLines = new List<string>();

            bool bShapeLine = false;

            if (KindKey != EMKind.MAGNET)
            {
                CNotice.printLogID("YATT5");
                return false;
            }

            try
            {
                // Shape 라인과 정보 라인을 분리한다.
                foreach (string strLine in listStringLines)
                {
                    if (readFile.isEndLine(strLine) == "Shape")
                        bShapeLine = false;

                    if (bShapeLine == true)
                        listShapeLines.Add(strLine);
                    else
                    {
                        if (readFile.isBeginLine(strLine) == "Shape")
                            bShapeLine = true;
                        else
                            listInformationLines.Add(strLine);
                    }
                }

                // 정보 라인을 처리한다.
                foreach (string strLine in listInformationLines)
                {
                    strTemp = strLine.Trim('\t');

                    arrayString = strTemp.Split('=');

                    if (arrayString.Length != 2)
                    {
                        CNotice.noticeWarningID("TIAP4");
                        return false;
                    }

                    switch (arrayString[0])
                    {
                        // CNode
                        case "NodeName":
                            NodeName = arrayString[1];
                            break;

                        case "KindKey":
                            KindKey = (EMKind)Enum.Parse(typeof(EMKind), arrayString[1]);
                            break;

                        // CParts
                        case "MovingParts":
                            MovingPart = (EMMoving)Enum.Parse(typeof(EMMoving), arrayString[1]);
                            break;

                        // CMagnet
                        case "Material":

                            m_strMaterialName = arrayString[1];

                            // FEMM (21Apr2019)에서 NdFeB 40 MGOe 빠져 있어서 호환이 되지 않아 강제로 N40 으로 변경한다.
                            // 추후에 FEMM 에 NdFeB 40 MGOe 가 Legacy 로 추가되면 아래의 코드를 삭제하라.
                            if (CProgramFEMM.getYearFEMM() >= 2019)
                            {
                                if (m_strMaterialName == "NdFeB 40 MGOe")
                                    m_strMaterialName = "N40";
                            }

                            if (CMaterialListInFEMM.isMagnetlInList(m_strMaterialName) == false)
                            {
                                // 현재의 버전에서 사용할 수 없는 재질이 존재한다면 공백으로 처리하고
                                // 동작 중에 공백을 사용해서 재질이 초기화 되지 않음을 확인한다.
                                m_strMaterialName = "";
                            }

                            break;

                        case "MagnetDirection":
                            emMagnetDirection = (EMMagnetDirection)Enum.Parse(typeof(EMMagnetDirection), arrayString[1]);
                            break;
                        
                        default:
                            // 해당사항이 없는 항목은 아무것도 하지 않는다. foreach 가 동작하기 때문에 return 해서는 않된다.
                            break;
                    }
                }

                // Shape 정보가 있는 경우만 m_face 를 생성하고 읽기 작업을 진행한다.
                if (listShapeLines.Count > 0)
                {
                    m_face = new CFace();
                    m_face.readObject(listShapeLines);
                }

            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return false;
            }

            return true;
        }

        public void setBlockProp(CScriptFEMM femm, double dMeshSize)
        {
            try
            {
                CPoint blockPoint = null;
                blockPoint = this.Face.getBlockPoint();

                if (blockPoint == null)
                {
                    CNotice.printLogID("NBPF");
                    return;
                }

                string strMaterialName = this.m_strMaterialName;

                double dMagnetAngle = 0;

                switch (emMagnetDirection)
                {
                    case EMMagnetDirection.RIGHT:
                        dMagnetAngle = 0;
                        break;

                    case EMMagnetDirection.UP:
                        dMagnetAngle = 90;
                        break;

                    case EMMagnetDirection.LEFT:
                        dMagnetAngle = 180;
                        break;

                    case EMMagnetDirection.DOWN:
                        dMagnetAngle = 270;
                        break;

                    default:
                        CNotice.printLog("정의 되지 않은 착자방향이 사용되고 있다.");

                        // 해당사항이 없는 항목이 넘어 왔기 때문에 바로 retrun 해서 아래의 동작을 하지 않는다.
                        return;
                }

                femm.setBlockProp(blockPoint, strMaterialName, dMeshSize, "none", dMagnetAngle, MovingPart, 0);
            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return;
            }
        }
    }

    public class CSteel : CShapeParts
    {
        // 상위 strMaterial 을 사용하기 때문에  { get; set; } 형식은 사용해서는 안된다
        [TypeConverter(typeof(CSteelPropertyConverter))]
        [DisplayNameAttribute("Part Material"), CategoryAttribute("\t\tSpecification Fields"), DescriptionAttribute("Name of Part Material")]
        public string MaterialName
        {
            get { return m_strMaterialName; }
            set { m_strMaterialName = value; }
        }

        public CSteel()
        {
            KindKey = EMKind.STEEL;
            MaterialName = "1010 Steel";
        }
        
        // 파일스트림 객체에 코일 정보를 기록한다.
        // override 를 꼭 사용해야 가상함수가 아니라 현 함수가 호출된다.
        public override bool writeObject(StreamWriter writeStream, int nLevel)
        {
            try
            {
                CWriteFile writeFile = new CWriteFile();

                writeFile.writeBeginLine(writeStream, "Steel", nLevel);

                // CNode
                writeFile.writeDataLine(writeStream, "NodeName", NodeName, nLevel + 1);
                writeFile.writeDataLine(writeStream, "KindKey", KindKey, nLevel + 1);

                // CParts
                writeFile.writeDataLine(writeStream, "MovingParts", MovingPart, nLevel + 1);

                // CSteel
                writeFile.writeDataLine(writeStream, "Material", m_strMaterialName, nLevel + 1);

                // CFace
                if (Face != null)
                {
                    Face.writeObject(writeStream, nLevel + 1);
                }            

                writeFile.writeEndLine(writeStream, "Steel", nLevel);
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
            CReadFile readFile = new CReadFile();
            string strTemp;
            string[] arrayString;

            List<string> listInformationLines = new List<string>();
            List<string> listShapeLines = new List<string>();

            bool bShapeLine = false;

            if (KindKey != EMKind.STEEL)
            {
                CNotice.printLogID("YATT6");
                return false;
            }

            try
            {
                // Shape 라인과 정보 라인을 분리한다.
                foreach (string strLine in listStringLines)
                {
                    if (readFile.isEndLine(strLine) == "Shape")
                        bShapeLine = false;

                    if (bShapeLine == true)
                        listShapeLines.Add(strLine);
                    else
                    {
                        if (readFile.isBeginLine(strLine) == "Shape")
                            bShapeLine = true;
                        else
                            listInformationLines.Add(strLine);
                    }
                }

                // 정보 라인을 처리한다.
                foreach (string strLine in listInformationLines)
                {
                    strTemp = strLine.Trim('\t');

                    arrayString = strTemp.Split('=');

                    if (arrayString.Length != 2)
                    {
                        CNotice.noticeWarningID("TIAP5");
                        return false;
                    }

                    switch (arrayString[0])
                    {
                        // CNode
                        case "NodeName":
                            NodeName = arrayString[1];
                            break;

                        case "KindKey":
                            KindKey = (EMKind)Enum.Parse(typeof(EMKind), arrayString[1]);
                            break;

                        // CParts
                        case "MovingParts":
                            MovingPart = (EMMoving)Enum.Parse(typeof(EMMoving), arrayString[1]);
                            break;

                        // CSteel
                        case "Material":
                            if (CMaterialListInFEMM.isSteelInList(arrayString[1]) == true)
                            {
                                m_strMaterialName = arrayString[1];
                            }
                            else
                            {
                                // 현재의 버전에서 사용할 수 없는 재질이 존재한다면 공백으로 처리하고
                                // 동작 중에 공백을 사용해서 재질이 초기화 되지 않음을 확인한다.
                                m_strMaterialName = "";
                            }     
                            break;                       

                        default:
                            // 해당사항이 없는 항목은 아무것도 하지 않는다. foreach 가 동작하기 때문에 return 해서는 않된다.
                            break;
                    }
                }

                // Shape 정보가 있는 경우만 m_face 를 생성하고 읽기 작업을 진행한다.
                if (listShapeLines.Count > 0)
                {
                    m_face = new CFace();
                    m_face.readObject(listShapeLines);
                }

            }
            catch (Exception ex)
            {
                CNotice.printLog(ex.Message);

                return false;
            }

            return true;
        }

        public void setBlockProp(CScriptFEMM femm, double dMeshSize)
        {
            CPoint blockPoint = null;
            blockPoint = this.Face.getBlockPoint();

            if (blockPoint == null)
            {
                CNotice.printLogID("NBPF");
                return;
            }

            string strMaterialName = this.m_strMaterialName;

            femm.setBlockProp(blockPoint, strMaterialName, dMeshSize, "none", 0, MovingPart, 0);
        }
    }


    internal class CMaterialListInFEMM
    {
        internal static List<string> steelList = new List<string>();
        internal static List<string> magnetList = new List<string>();
        internal static List<string> coilWireList = new List<string>();

        public static bool isSteelInList(string strMaterial)
        {
            return steelList.Contains(strMaterial);
        }

        public static bool isMagnetlInList(string strMaterial)
        {
            return magnetList.Contains(strMaterial);
        }

        public static bool isCoilWIreInList(string strMaterial)
        {
            return coilWireList.Contains(strMaterial);
        }
    }

    internal class CSteelPropertyConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            //True - means show a Combobox and False for show a Modal 
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            //False - a option to edit values and True - set values to state readonly
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(CMaterialListInFEMM.steelList);
        }
    }

    internal class CMagnetPropertyConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            //True - means show a Combobox and False for show a Modal 
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            //False - a option to edit values and True - set values to state readonly
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(CMaterialListInFEMM.magnetList);
        }
    }

    internal class CCoilWirePropertyConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            //True - means show a Combobox and False for show a Modal 
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            //False - a option to edit values and True - set values to state readonly
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(CMaterialListInFEMM.coilWireList);
        }
    }

}
