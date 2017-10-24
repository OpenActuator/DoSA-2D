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
        [DisplayNameAttribute("Node Name"), CategoryAttribute("\t\t\tCommon Fields"), DescriptionAttribute("Part 나 Experiment 이름")]
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

        // Design 에 사용되는 부품이나 실험조건을 저장하는 List 이다.
        private List<CNode> m_listNode = new List<CNode>();

        public bool m_bChanged;

        const double MESH_DENSITY = 0.02;

        // Get 전용로 오픈한다
        public List<CNode> NodeList
        {
            get
            {
                return m_listNode;
            }
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
            
            foreach(CNode node in m_listNode)
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

            foreach (CNode node in m_listNode)
            {
                if (node.GetType().BaseType.Name == "CParts")
                {
                    dFaceArea = ((CParts)node).Face.calcArea();

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

		internal void writeObject(StreamWriter writeStream)
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
                        CNotice.printTrace("초기화 되지 않은 Face 를 DrawDesign 을 하려하고 있다.");
                }
            }

            femm.zoomFit();
        }

        internal void setBlockPropeties(CScriptFEMM femm, double dVolt)
        {
            // Mesh Size 는 길이단위이기 때문에 면적을 루트 취한 값과 곱하고 있다.
            double dMeshSize = Math.Sqrt(this.calcShapeModelArea()) * MESH_DENSITY;

            foreach (CNode node in NodeList)
            {
                switch (node.m_kindKey)
                {
                    case EMKind.COIL:
                        double dCurrent = dVolt / ((CCoil)node).Resistance;
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

        internal void changeCurrent(CScriptFEMM femm, double dCurrent)
        {
            // Mesh Size 는 길이단위이기 때문에 면적을 루트 취한 값과 곱하고 있다.
            double dMeshSize = Math.Sqrt(this.calcShapeModelArea()) * MESH_DENSITY;

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

        internal void getMaterial(CScriptFEMM femm)
        {
            List<string> listMaterial = new List<string>();
            string strMaterial = "Air";
            CParts nodeParts = null;

            bool bCheck;

            femm.getMaterial(strMaterial);
            listMaterial.Add(strMaterial);

            foreach (CNode node in NodeList)
            {
                bCheck = false;
                if (node.GetType().BaseType.Name == "CParts")
                {
                    nodeParts = (CParts)node;
                    strMaterial = nodeParts.getMaterial();

                    /// 현 파트의 재료가 기존에 저장된 Material 과 겹치는지를 확인한다.
                    foreach(string strTemp in listMaterial)
                        if (strTemp == strMaterial)
                            bCheck = true;

                    // 겹치지 않는 재료만 추가한다.
                    if(bCheck == false)
                    {
                        listMaterial.Add(strMaterial);
                        femm.getMaterial(nodeParts.getMaterial());
                    }
                        
                }
            }
        }

        internal void setBoundary(CScriptFEMM femm, double dPlusMovingStroke = 0, double dMinusMovingStroke = 0)
        {
            const int iPaddingPercent = 200;
            
            double minX = 0;
            double maxX = 0;
            double minY = 0;
            double maxY = 0;

            this.getModelMinMaxX(ref minX, ref maxX);
            this.getModelMinMaxY(ref minY, ref maxY);

            double lengthX = Math.Abs(maxX - minX);
            double lengthY = Math.Abs(maxY - minY);

            // Mesh Size 는 길이단위이기 때문에 면적을 루트 취한 값과 곱하고 있다.
            double dMeshSize = Math.Sqrt(this.calcShapeModelArea()) * MESH_DENSITY;

            double padLengthX = lengthX * iPaddingPercent / 100.0f;
            double padLengthY = lengthY * iPaddingPercent / 100.0f;

            /// - 긴방향의 길이로 Pad 량을 결정한다.
            /// - 100.0f 는 Percent 를 배수로 환산한다.
            double padLength = (padLengthX > padLengthY) ? padLengthX : padLengthY;


            CFace face = new CFace();

            //CPoint blockPoint = new CPoint();

            /// 외부 Region 을 생성한다.
            /// - X min 값 : 0
            /// - Mesh : AutoMesh
            face.setOutsideBoundary(   femm, 0, maxY + padLength + dPlusMovingStroke, 
                                maxX + padLength, minY - padLength + dMinusMovingStroke, 0);


            /// 내부 Region 은 메쉬를 위해 추가했지만
            /// Lorenz Force 와 Virtual Force 차이가 감소하지 않아서 사용을 보류하고 있다.
            /// 
            //const double dRatioRegion = 5.0f;

            /// 내부 Region 을 생성한다.
            /// - X min 값 : 0
            /// - Mesh : 지정메쉬 
            //face.setInsideBoundary(   femm, 0, maxY + padLengthY / dRatioRegion + dPlusMovingStroke,
            //                    maxX + padLengthX / dRatioRegion, minY - padLengthY / dRatioRegion + dMinusMovingStroke, dMeshSize);

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
        internal bool duplicateNodeName()
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
    }
}
