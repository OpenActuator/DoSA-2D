using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Properties Category
using System.ComponentModel;

using gtLibrary;
using Parts;
using System.IO;
using Scripts;
using Shapes;
using System.Windows.Forms;
using DoSA;

namespace Nodes
{
    // Enum 사용목적
    // - Treeview ImageList 의 인덱스
    // - CNode 에서 상속받은 객체들의 종류 구분
    public enum EMKind
    {
        PARTS,
        COIL,
        MAGNET,
        STEEL,
        EXPERIMENTS,
        FORCE_EXPERIMENT,
        STROKE_EXPERIMENT,
        CURRENT_EXPERIMENT
    };

    public class CNode
    {
        public EMKind m_kindKey;

        private string m_nodeName;

        ///  \t\t\t 갯수가 많을수록 해당 카테고리가 상측으로 올라간다.
        [DisplayNameAttribute("Node Name"), CategoryAttribute("\t\t\tCommon Fields"), DescriptionAttribute("Part or Experiment name")]
        public string NodeName
        {
            get { return m_nodeName; }
            set { m_nodeName = value; }
        }

        // 작업파일 저장을 각 객체에서 진행하고 있다.
        public virtual bool writeObject(StreamWriter writeFile) { return true; }
        public virtual bool readObject(List<string> listStringLines, ref CNode node) { return true; }
    }

    public class CDesign
    {
        // Design 객체의 이름
        public string m_strDesignName;

        // Design 객체의 객체 디렉토리
        // Design 디렉토리는 복사가 가능하기 때문에 아래의 디렉토리는 저장하지는 않는다
        public string m_strDesignDirName;

        // Design 에 사용되는 부품이나 시험조건을 저장하는 List 이다.
        private List<CNode> m_listNode = new List<CNode>();

        public bool m_bChanged;

        // Get 전용로 오픈한다
        public List<CNode> NodeList
        {
            get
            {
                return m_listNode;
            }
        }


        private bool isIntersectedAllLines()
        {
            List<CLine> listLineAll = new List<CLine>();
            List<CLine> listAbsoluteLine = null;
            CFace face = null;

            foreach (CNode node in NodeList)
            {
                if (node.GetType().BaseType.Name == "CParts")
                {
                    CParts nodeParts = (CParts)node;

                    face = nodeParts.Face;

                    if (null != face)
                    {
                        listAbsoluteLine = face.AbsoluteLineList;

                        /// 모든 라인들을 하나의 Line List 에 담는다.
                        foreach (CLine line in listAbsoluteLine)
                            listLineAll.Add(line);
                    }
                }
            }

            CShapeTools shapeTools = new CShapeTools();

            for (int i = 0; i < listLineAll.Count - 1; i++)
            {
                for (int j = i + 1; j < listLineAll.Count; j++)
                {
                    if (true == shapeTools.isIntersected(listLineAll[i], listLineAll[j]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool isContactedMovingParts()
        {
            List<CLine> listMovingPartLines = new List<CLine>();
            List<CLine> listFixedPartLines = new List<CLine>();
            List<CLine> listAbsoluteLine = null;
            CFace face = null;

            foreach (CNode node in NodeList)
            {
                if (node.GetType().BaseType.Name == "CParts")
                {
                    CParts nodeParts = (CParts)node;

                    face = nodeParts.Face;

                    if (null != face)
                    {
                        listAbsoluteLine = face.AbsoluteLineList;

                        if (nodeParts.MovingPart == EMMoving.MOVING)
                        {
                            /// Moving Part 라인들을 하나의 Line List 에 담는다.
                            foreach (CLine line in listAbsoluteLine)
                                listMovingPartLines.Add(line);
                        }
                        else
                        {
                            /// Moving Part 라인들을 하나의 Line List 에 담는다.
                            foreach (CLine line in listAbsoluteLine)
                                listFixedPartLines.Add(line);
                        }

                    }
                }
            }

            CShapeTools shapeTools = new CShapeTools();

            for (int i = 0; i < listMovingPartLines.Count - 1; i++)
            {
                for (int j = i + 1; j < listFixedPartLines.Count; j++)
                {
                    if (true == shapeTools.isContacted(listMovingPartLines[i], listFixedPartLines[j]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public CDesign()
        {
            // 꼭 초기화가 되어야 한다.
            m_strDesignName = string.Empty;
            m_strDesignDirName = string.Empty;

            m_bChanged = false;
        }

        public CNode getNode(string nodeName)
        {
            foreach (CNode node in m_listNode)
            {
                // 같은 이름의 노드가 있으면 리턴한다.
                if (node.NodeName == nodeName)
                    return node;
            }

            return null;
        }

        public bool getModelMinMaxX(ref double dMinX, ref double dMaxX)
        {
            /// 비교 값을 초기화 한다.
            /// Max 는 아주 작은 값, Min 는 아주 큰 값으로 설정한다.
            double minX = 1e300;
            double maxX = -1e300;

            double tempMinX = 0;
            double tempMaxX = 0;

            if (m_listNode.Count == 0)
                return false;

            foreach (CNode node in m_listNode)
            {
                if (node.GetType().BaseType.Name == "CParts")
                {
                    if (false == ((CParts)node).Face.getMinMaxX(ref tempMinX, ref tempMaxX))
                        return false;

                    if (tempMinX < minX) minX = tempMinX;
                    if (tempMaxX > maxX) maxX = tempMaxX;
                }
            }

            /// minX 나 maxX 가 값이 입력되지 않으면 오류를 발생시킨다.
            if (minX == 1e300 || maxX == -1e300)
                return false;
            else
            {
                dMinX = minX;
                dMaxX = maxX;

                return true;
            }
        }

        public double calcShapeModelArea()
        {
            double dFaceArea = 0;
            double dSumArea = 0;

            CShapeTools shapeTools = new CShapeTools();

            foreach (CNode node in m_listNode)
            {
                if (node.GetType().BaseType.Name == "CParts")
                {
                    dFaceArea = shapeTools.calcArea(((CParts)node).Face);

                    dSumArea += dFaceArea;
                }
            }

            return dSumArea;
        }

        public bool getModelMinMaxY(ref double dMinY, ref double dMaxY)
        {
            /// 비교 값을 초기화 한다.
            /// Max 는 아주 작은 값, Min 는 아주 큰 값으로 설정한다.
            double minY = 1e300;
            double maxY = -1e300;

            double tempMinY = 0;
            double tempMaxY = 0;

            if (m_listNode.Count == 0)
                return false;

            foreach (CNode node in m_listNode)
            {
                if (node.GetType().BaseType.Name == "CParts")
                {
                    if (false == ((CParts)node).Face.getMinMaxY(ref tempMinY, ref tempMaxY))
                        return false;

                    if (tempMinY < minY) minY = tempMinY;
                    if (tempMaxY > maxY) maxY = tempMaxY;
                }
            }

            /// minX 나 maxX 가 값이 입력되지 않으면 오류를 발생시킨다.
            if (minY == 1e300 || maxY == -1e300)
                return false;
            else
            {
                dMinY = minY;
                dMaxY = maxY;

                return true;
            }
        }

        public int getNodeCount()
        {
            return m_listNode.Count;
        }

        public bool addNode(CNode node)
        {
            if (isExistNode(node.NodeName) == true)
                return false;

            m_listNode.Add(node);

            return true;
        }

        // 같이 이름의 Node 가 있는지 검사한다.
        public bool isExistNode(string nodeName)
        {
            foreach (CNode node in m_listNode)
            {
                if (node.NodeName == nodeName)
                    return true;
            }

            return false;
        }

        public bool deleteNode(string nodeName)
        {

            // 추가된 Node 중에 없으면 바로 리턴한다.
            if (isExistNode(nodeName) == false)
                return false;

            foreach (CNode node in m_listNode)
            {
                if (node.NodeName == nodeName)
                {
                    // 삭제 후 바로 빠져나가야 한다.
                    m_listNode.Remove(node);
                    return true;
                }
            }

            return false;
        }

        public void clearDesign()
        {
            m_strDesignName = string.Empty;
            m_strDesignDirName = string.Empty;

            m_listNode.Clear();
        }

        // 해당 종류의 노드 갯수를 얻어 온다
        public int getKindNodeSize(EMKind kind)
        {
            int size = 0;

            foreach (CNode node in m_listNode)
            {
                if (node.m_kindKey == kind)
                    size++;
            }

            return size;
        }

        public void writeObject(StreamWriter writeStream)
        {
            CWriteFile writeFile = new CWriteFile();

            writeFile.writeBeginLine(writeStream, "Design", 1);
            writeFile.writeDataLine(writeStream, "DesignName", m_strDesignName, 2);

            foreach (CNode node in NodeList)
            {
                node.writeObject(writeStream);
            }

            writeFile.writeEndLine(writeStream, "Design", 1);
        }

        public void drawDesign(CScriptFEMM femm)
        {
            CFace face = null;

            foreach (CNode node in NodeList)
            {
                if (node.GetType().BaseType.Name == "CParts")
                {
                    CParts nodeParts = (CParts)node;

                    face = nodeParts.Face;

                    if (null != face)
                        nodeParts.Face.drawFace(femm, nodeParts.MovingPart);
                    else
                        CNotice.printTraceID("YATT1");
                }
            }

            femm.zoomFit();
        }

        public void setBlockPropeties(CScriptFEMM femm, double dVolt, double dMeshSizePercent)
        {
            // MeshSizePercent 에 문제가 있으면 1% 로 초기화 한다.
            if (dMeshSizePercent <= 0)
                dMeshSizePercent = 1;

            // Mesh Size 는 길이단위이기 때문에 면적을 루트 취한 값과 곱하고 있다.
            double dMeshSize = Math.Sqrt(this.calcShapeModelArea()) * dMeshSizePercent / 100.0f;

            foreach (CNode node in NodeList)
            {
                switch (node.m_kindKey)
                {
                    case EMKind.COIL:
                        double dCurrent;
                        double dResistance = getSerialResistance();

                        // 전류 계산
                        if (dResistance != 0.0f)
                            dCurrent = dVolt / dResistance;
                        else
                            dCurrent = 0.0f;

                        ((CCoil)node).setBlockPropCurrent(femm, dCurrent, dMeshSize);
                        break;

                    case EMKind.MAGNET:
                        ((CMagnet)node).setBlockProp(femm, dMeshSize);
                        break;

                    case EMKind.STEEL:
                        ((CSteel)node).setBlockProp(femm, dMeshSize);
                        break;

                    default:
                        break;
                }
            }
        }

        public double getSerialResistance()
        {
            double Resistance = 0;

            foreach (CNode node in m_listNode)
            {
                if (node.m_kindKey == EMKind.COIL)
                    Resistance = Resistance + ((CCoil)node).Resistance;
            }

            return Resistance;
        }

        public void changeCurrent(CScriptFEMM femm, double dCurrent, double dMeshSizePercent)
        {
            // MeshSizePercent 에 문제가 있으면 1% 로 초기화 한다.
            if (dMeshSizePercent <= 0)
                dMeshSizePercent = 1;

            // Mesh Size 는 길이단위이기 때문에 면적을 루트 취한 값과 곱하고 있다.
            double dMeshSize = Math.Sqrt(this.calcShapeModelArea()) * dMeshSizePercent / 100.0f;

            foreach (CNode node in NodeList)
            {
                switch (node.m_kindKey)
                {
                    case EMKind.COIL:
                        ((CCoil)node).setBlockPropCurrent(femm, dCurrent, dMeshSize);
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 해석 모델안으로 사용하는 재질들을 추가한다.
        /// </summary>
        /// <param name="femm"></param>
        public void addMaterials(CScriptFEMM femm)
        {
            // 겹치는 재질을 제외하기 위해 사용한다.
            List<string> listTempMaterial = new List<string>();

            string strMaterial = "Air";

            bool bCheck;

            // Air 는 무조건 사용하기 때문에 여기서 우선적으로 추가해 둔다.
            femm.addMaterial(strMaterial);
            listTempMaterial.Add(strMaterial);

            foreach (CNode node in NodeList)
            {
                bCheck = false;
                if (node.GetType().BaseType.Name == "CParts")
                {
                    strMaterial = ((CParts)node).getMaterial();

                    /// 현 파트의 재료가 기존에 저장된 Material 과 겹치는지를 확인한다.
                    foreach (string strTemp in listTempMaterial)
                        if (strTemp == strMaterial)
                            bCheck = true;

                    // 겹치지 않는 재료만 추가한다.
                    if (bCheck == false)
                    {
                        femm.addMaterial(strMaterial);
                        listTempMaterial.Add(strMaterial);

                        // 발생해서는 안되는 상황이 발생하는 경우
                        if(strMaterial == "")
                        {
                            CNotice.printTrace("재질이 설정되지 않는 파트가 존재해 해석 오류가 발생한다.");
                        }
                    }
                }
            }
        }

        public void setBoundary(CScriptFEMM femm, double dMeshSizePercent, double dPlusMovingStroke, double dMinusMovingStroke)
        {
            const int iPaddingPercent = 200;

            double minX, minY, maxX, maxY;
            minX = minY = maxX = maxY = 0;

            this.getModelMinMaxX(ref minX, ref maxX);
            this.getModelMinMaxY(ref minY, ref maxY);

            double lengthX = Math.Abs(maxX - minX);
            double lengthY = Math.Abs(maxY - minY);

            // MeshSizePercent 에 문제가 있으면 1% 로 초기화 한다.
            if (dMeshSizePercent <= 0)
                dMeshSizePercent = 1;

            // Mesh Size 는 길이단위이기 때문에 면적을 루트 취한 값과 곱하고 있다.
            double dMeshSize = Math.Sqrt(this.calcShapeModelArea()) * dMeshSizePercent / 100.0f;

            double padLengthX = lengthX * iPaddingPercent / 100.0f;
            double padLengthY = lengthY * iPaddingPercent / 100.0f;

            /// - 긴방향의 길이로 Pad 량을 결정한다.
            /// - 100.0f 는 Percent 를 배수로 환산한다.
            double padLength = (padLengthX > padLengthY) ? padLengthX : padLengthY;

            CFace face = new CFace();

            /// 외부 Region 을 생성 및 경계조건을 부여한다.
            /// - X min 값 : 0
            /// - Mesh : AutoMesh
            face.setOutsideBoundary(femm, 0, maxY + padLength + dPlusMovingStroke,
                                maxX + padLength, minY - padLength + dMinusMovingStroke, 0);

            /// 내부 Region 은 경계조건과 상관없이 메쉬만를 위해 추가하였다.
            /// 내부 Region 의 메쉬 크기는 기본 메쉬의 3배로 설정한다.
            const double dRatioRegion = 5.0f;

            /// 내부 Region 을 생성한다.
            /// - X min 값 : 0
            /// - Mesh : 지정메쉬 * 3.0f
            face.setInsideRegion(   femm, 0, maxY + padLengthY / dRatioRegion + dPlusMovingStroke,
                                maxX + padLengthX / dRatioRegion, minY - padLengthY / dRatioRegion + dMinusMovingStroke, dMeshSize * 3.0f);
        }

        /// <summary>
        /// 이름이 겹치는 Node 가 있는지를 확인한다.
        /// 
        /// [목적]
        ///  - PropertyGrid 에서 이름을 수정할 때 이름이 겹치는 문제를 해결하기 위해 추가함
        ///  - 이름을 수정 할 때 기존에 동일한 이름이 있는지를 확인하고 이름을 변경하는 것이 좋으나,
        ///    PropertyGrid 에서 이름을 변경하면 바로 NodeList 의 이름이 바뀌기 때문에 
        ///    이름 겹침으로 수정이 잘못되었음을 확인하고 이름을 복원하는 방법을 사용해 해결함
        /// </summary>
        /// <returns>이름이 겹치면 true 리턴</returns>
        public bool duplicateNodeName()
        {
            /// 비교의 앞 이름은 m_listNode.Count - 1 까지 이다.
            for (int i = 0; i < m_listNode.Count - 1; i++)
            {
                /// 비교의 뒤 이름은 1 부터 이다.
                for (int j = i + 1; j < m_listNode.Count; j++)
                {
                    if (m_listNode[i].NodeName == m_listNode[j].NodeName)
                        return true;
                }
            }

            return false;
        }

        public bool isDesignShapeOK(double dStroke = 0)
        {
            CFace face = null;
            bool bError = false;
            CParts nodeParts = null;

            // Moving Part 를 Stroke 만큼 이동시킨다.
            foreach (CNode node in NodeList)
            {
                if (node.GetType().BaseType.Name == "CParts")
                {
                    nodeParts = (CParts)node;

                    if (nodeParts.MovingPart == EMMoving.MOVING)
                    {
                        face = nodeParts.Face;
                        face.BasePoint.m_dY = face.BasePoint.m_dY + dStroke;
                    }
                }
            }

            if (isIntersectedAllLines() == true)
            {
                CNotice.noticeWarningID("LCBP");
                bError = true;
            }

            if (isContactedMovingParts() == true)
            {
                CNotice.noticeWarningID("IHOT");
                bError = true;
            }

            // Moving Part 를 Stroke 만큼 복원 시킨다.
            foreach (CNode node in NodeList)
            {
                if (node.GetType().BaseType.Name == "CParts")
                {
                    nodeParts = (CParts)node;

                    if (nodeParts.MovingPart == EMMoving.MOVING)
                    {
                        face = nodeParts.Face;
                        face.BasePoint.m_dY = face.BasePoint.m_dY - dStroke;
                    }
                }
            }

            if (bError == true)
                return false;
            else
                return true;
        }

    }
}
