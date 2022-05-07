using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Properties Category
using System.ComponentModel;

using Nodes;
using Scripts;
using gtLibrary;
using Parts;

namespace Shapes
{
    public enum EMLineKind
    {
        STRAIGHT,
        ARC
    };

    public enum EMDirectionArc
    {
        FORWARD,
        BACKWARD
    };

    public enum EMFaceType
    {
        RECTANGLE,
        POLYGON
    }

    public enum EMNumberKind
    {
        ODD,
        EVEN,
        ZERO
    }

    public class CFace
    {
        const int INDEX_ERROR = -1;

        private CPoint m_basePoint = new CPoint();

        private CShapeTools m_shapeTools = new CShapeTools();

        const int MIN_POLYGON_LINE_COUNT = 4;
        const int MIN_POINT_COUNT = 4;

        public CPoint BasePoint
        {
            get { return m_basePoint; }
            set { m_basePoint = value; }
        }

        /// <summary>
        /// [ 정책 ]
        /// - Face 안의 형상관리는 Line 이 아니라 Point 를 사용하고 있다.
        /// - 이유는 기본형상 입력을 Point 를 사용하기 때문이다.
        ///   따라서 CLine 객체는 외부에서 필요할 때만 그때의 Point 정보로 만들어서 사용한다.
        /// </summary>
        private List<CPoint> m_listRelativePoint = new List<CPoint>();

        // 내부좌표 List 는 상대좌표임으로 m_listPoint 를 바로 리턴한다.
        public List<CPoint> RelativePointList
        {
            get { return m_listRelativePoint; }
        }

        // 절대좌표는 내부 기준점과 내부좌표 List 를 고려해서 생성해서 리턴한다.
        public List<CPoint> AbsolutePointList
        {
            get
            {
                List<CPoint> listPoint = new List<CPoint>();
                CPoint point = null;
                CShapeTools shapeTools = new CShapeTools();

                // foreach 를 사용해서 읽어내서 더하게 되면 계속 증가하는 문제가 발생한다.
                // for 문에서 매번 point 를 new 를 하면 Add 하라.
                for (int i=0; i < m_listRelativePoint.Count; i++ )
                {
                    point = new CPoint();
                    point.X = shapeTools.roundDigitOfShape(m_listRelativePoint[i].X + m_basePoint.X);
                    point.Z = shapeTools.roundDigitOfShape(m_listRelativePoint[i].Z + m_basePoint.Z);
                    point.DirectionArc = m_listRelativePoint[i].DirectionArc;
                    point.LineKind = m_listRelativePoint[i].LineKind;

                    listPoint.Add(point);
                }

                return listPoint;
            }
        }

        private EMFaceType m_emFaceType;

        public EMFaceType FaceType
        {
            get { return m_emFaceType; }
            set { m_emFaceType = value; }
        }

        /// <summary>
        ///  필요할 때만 Point 정보를 가지고 임시로 생성한다.
        ///  [주의사항]
        ///  - 접근 때마다 생성하기 때문에 
        ///    사용할 때는 List<CLine> 로 새로운 List 를 만들고 한번 초기화 한 후에 새로운 List 를 사용하라.
        /// </summary>
        public List<CLine> AbsoluteLineList
        { 
            get
            {
                if (m_listRelativePoint.Count < MIN_POLYGON_LINE_COUNT)
                {
                    CNotice.printLogID("WRAL");
                    return null;
                }

                List<CLine> listLine = new List<CLine>();
                CPoint startPoint = null;
                CPoint endPoint = null;
                CShapeTools shapeTools = new CShapeTools();
                
                for (int i = 0; i < m_listRelativePoint.Count; i++)
                {
                    // 기준 점인 Base Point 의 좌표와 상대좌표인 PointList 를 더해서
                    // 절대좌표값을 계산한 후에 형상을 그리기 위한 LineList 를 만든다
                    startPoint = new CPoint();
                    startPoint.X = shapeTools.roundDigitOfShape(m_listRelativePoint[i].X + m_basePoint.X);
                    startPoint.Z = shapeTools.roundDigitOfShape(m_listRelativePoint[i].Z + m_basePoint.Z);
                    startPoint.DirectionArc = m_listRelativePoint[i].DirectionArc;
                    startPoint.LineKind = m_listRelativePoint[i].LineKind;

                    // 마지막 Point 을 제외한 나머지 경우
                    if (i < m_listRelativePoint.Count - 1)
                    {
                        // 기준 점인 Base Point 의 좌표와 상대좌표인 PointList 를 더해서
                        // 절대좌표값을 계산한 후에 형상을 그리기 위한 LineList 를 만든다
                        endPoint = new CPoint();
                        endPoint.X = shapeTools.roundDigitOfShape(m_listRelativePoint[i + 1].X + m_basePoint.X);
                        endPoint.Z = shapeTools.roundDigitOfShape(m_listRelativePoint[i + 1].Z + m_basePoint.Z);
                        endPoint.DirectionArc = m_listRelativePoint[i + 1].DirectionArc;
                        endPoint.LineKind = m_listRelativePoint[i + 1].LineKind;
                    }
                    /// 마지막인 Point 경우
                    else
                    {
                        endPoint = new CPoint();
                        endPoint.X = shapeTools.roundDigitOfShape(m_listRelativePoint[0].X + m_basePoint.X);
                        endPoint.Z = shapeTools.roundDigitOfShape(m_listRelativePoint[0].Z + m_basePoint.Z);
                        endPoint.DirectionArc = m_listRelativePoint[0].DirectionArc;
                        endPoint.LineKind = m_listRelativePoint[0].LineKind;
                    }

                    listLine.Add(new CLine(startPoint, endPoint));
                }

                return listLine;
            }
        }

        public CFace()
        {
            m_basePoint.X = 0.0f;
            m_basePoint.Z = 0.0f;
        }

        public bool addPoint(CPoint point)
        {
            m_listRelativePoint.Add(point);

            return true;
        }

        public bool isRectangle()
        {
            bool bCheck = false;

            CPoint startPoint, endPoint;
            double dDistanceX, dDistanceZ;

            if (m_listRelativePoint.Count != 4)
            {
                bCheck = false;
            }
            else
            {
                // 회전하지 않은 직사각형인지를 확인한다.
                for (int i = 0; i < 4; i++)
                {
                    if (i != 3)
                    {
                        startPoint = m_listRelativePoint[i];
                        endPoint = m_listRelativePoint[i + 1];
                    }
                    else
                    {
                        startPoint = m_listRelativePoint[i];
                        endPoint = m_listRelativePoint[0];
                    }

                    dDistanceX = Math.Abs(startPoint.X - endPoint.X);
                    dDistanceZ = Math.Abs(startPoint.Z - endPoint.Z);

                    // 회전하지 않았으면 모든 라인은 수직이거나 수평이기 때문에 한쪽의 좌표의 거리는 0이 있어야 한다.
                    if (CSmallUtil.isZeroPosition(dDistanceX) == true || CSmallUtil.isZeroPosition(dDistanceZ) == true)
                        bCheck = true;
                    else
                        bCheck = false;
                }
            }

            return bCheck;
        }

        /// <summary>
        /// Face 안의 형상문제를 확인한다.
        /// </summary>
        public bool isShapeOK()
        {
            ///-----------------------------------------
            /// 1. X 음의 좌료 입력 확인
            ///----------------------------------------- 
            /// 축대칭 모델만 사용하기 때문에 
            /// 내부 포인트의 X 좌표값은 항상 영보다 커야 한다.
            foreach(CPoint point in m_listRelativePoint)
            {
                if((point.X + m_basePoint.X) < 0)
                {
                    CNotice.noticeWarningID("YUNX");
                    return false;
                }   
            }

            ///-----------------------------------------
            /// 2. 내부 라인들의 교차 및 겹침 확인
            ///-----------------------------------------
            /// 
            /// 매번 생성하는 Property 이기 때문에 
            /// LineList 는 새로운 List에  담는 동작 한번만 호출하고, 사용은 새로운 List 를 사용한다.
            List<CLine> listAbsoluteLine = new List<CLine>();
            listAbsoluteLine = AbsoluteLineList;

            if (AbsoluteLineList == null)
            {
                CNotice.noticeWarningID("TWAP");
                return false;
            }
                

            /// 아래와 같이 라인의 교차와 겹칩을 판단한다.
            /// 1 line <=> 2 line ... last line
            /// 2 line <=> 3 line ... last line
            /// ...
            /// lsat-2 line <=> last-1 line , last line
            /// last-1 line <=> last line 
            for (int i = 0; i < listAbsoluteLine.Count - 1; i++)
            {
                for (int j = i + 1; j < listAbsoluteLine.Count; j++)
                {
                    if (true == m_shapeTools.isIntersected(listAbsoluteLine[i], listAbsoluteLine[j]))
                    {
                        CNotice.noticeWarningID("AIPB");
                        return false;
                    }

                    if (true == m_shapeTools.isOverlaped(listAbsoluteLine[i], listAbsoluteLine[j]))
                    {
                        CNotice.noticeWarningID("AOPB");
                        return false;
                    }
                }
            }

            ///-----------------------------------------
            /// 3. 면적 확인을 한다.
            ///----------------------------------------- 
            if (m_shapeTools.calcArea(this) == 0)
            {
                CNotice.noticeWarningID("TINA");
                return false;
            }


             return true;
        }

        /// <summary>
        ///  사각형의 4 포인트를 생성하고 저장한다.
        /// </summary>
        public bool setRectanglePoints(double x1, double y1, double x2, double y2)
        {
            double bigX, smallX;
            double bigY, smallY;

            m_listRelativePoint.Clear();

            m_emFaceType = EMFaceType.RECTANGLE;

            if (x1 > x2) 
            {  
                bigX = x1;  
                smallX = x2;    
            }
            else         
            {  
                bigX = x2;  
                smallX = x1;    
            }

            if (y1 > y2)
            {
                bigY = y1;
                smallY = y2;
            }
            else
            {
                bigY = y2;
                smallY = y1;
            }

            /// Base Point 가 좌하점이기 때문에 
            /// 박스를 그리는 순서는 좌하점 부터 반시계 방향으로 그린다.

            // 1 점 (좌하)
            m_listRelativePoint.Add(new CPoint(smallX, smallY, EMLineKind.STRAIGHT, EMDirectionArc.FORWARD));

            // 2 점 (우하)
            m_listRelativePoint.Add(new CPoint(bigX, smallY, EMLineKind.STRAIGHT, EMDirectionArc.FORWARD));

            // 3 점 (우상)
            m_listRelativePoint.Add(new CPoint(bigX, bigY, EMLineKind.STRAIGHT, EMDirectionArc.FORWARD));

            // 4 점 (좌상)
            m_listRelativePoint.Add(new CPoint(smallX, bigY, EMLineKind.STRAIGHT, EMDirectionArc.FORWARD));

            return true;
        }

        /// <summary>
        /// FEMM 에 Face 를 그린다.
        /// 
        /// FEMM 에 형상을 그릴 때는 절대좌표를 사용해야 한다.
        /// </summary>
        /// <param name="femm">FEMM</param>
        /// <param name="emMoving">Line 의 Group 를 결정하기 위한 동작부 여부</param>
        public bool drawFace(CScriptFEMM femm, EMMoving emMoving = EMMoving.FIXED)
        {
            double x1, y1, x2, y2;
            bool bDirectionArc = false;

            if (m_listRelativePoint.Count < MIN_POINT_COUNT)
            {
                CNotice.printLogID("YATT2");
                return false;
            }

            /// 매번 생성하는 Property 이기 때문에 
            /// LineList 는 새로운 List에  담는 동작 한번만 호출하고, 사용은 새로운 List 를 사용한다.
            List<CPoint> listAbsolutePoint = new List<CPoint>();
            listAbsolutePoint = AbsolutePointList;

            // Face 에 저장될 때는 Rectangle 도 4개의 직선으로 저장되기 때문에
            // Face 를 그릴 때는 모두 다각형으로 취급한다.
            for (int i = 0; i < listAbsolutePoint.Count; i++)
            {
                // 마지막 Point 만 제외한다.
                if (i < listAbsolutePoint.Count - 1)
                {
                    x1 = listAbsolutePoint[i].X;
                    y1 = listAbsolutePoint[i].Z;
                    x2 = listAbsolutePoint[i + 1].X;
                    y2 = listAbsolutePoint[i + 1].Z;
                }
                // 마지막 선은 끝점과 첫점을 있는다
                else
                {
                    x1 = listAbsolutePoint[i].X;
                    y1 = listAbsolutePoint[i].Z;
                    x2 = listAbsolutePoint[0].X;
                    y2 = listAbsolutePoint[0].Z;
                }

                if (listAbsolutePoint[i].LineKind == EMLineKind.ARC)
                {
                    bDirectionArc = (listAbsolutePoint[i].DirectionArc == EMDirectionArc.BACKWARD ? true : false);
                    femm.drawArc(x1, y1, x2, y2, bDirectionArc, emMoving);
                }
                else
                {
                    femm.drawLine(x1, y1, x2, y2, emMoving);
                }           
            }

            return true;
        }

        /// <summary>
        /// 다각형의 포인트를 저장한다.
        /// </summary>
        /// <param name="listPoint"></param>
        /// <returns></returns>
        public bool setPolygonPoints(List<CPoint> listPoint)
        {
            if (listPoint.Count < MIN_POINT_COUNT)
            {
                CNotice.printLogID("YATT");
                return false;
            }

            m_listRelativePoint.Clear();

            m_emFaceType = EMFaceType.POLYGON;

            foreach (CPoint point in listPoint)
                m_listRelativePoint.Add(point);

            return true;
        }

        /// <summary>
        /// Face 안의 부품의 속성을 입력하기 위한 BlockPoint 를 얻어낸다.
        /// 
        /// 절대좌표의 내부 블럭점을 찾는 것이기 때문에 절대 좌표를 사용해야 한다.
        /// </summary>
        /// <returns>Block Point</returns>
        public CPoint getBlockPoint()
        {
            CPoint blockPoint = new CPoint();

            double sumX = 0;
            double sumY = 0;

            /// Rectangle 은 4 좌표의 평균점을 사용한다.
            if(FaceType == EMFaceType.RECTANGLE)
            {
                /// 매번 생성하는 Property 이기 때문에 
                /// LineList 는 새로운 List에  담는 동작 한번만 호출하고, 사용은 새로운 List 를 사용한다.
                List<CPoint> listAbsolutePoint = new List<CPoint>();
                listAbsolutePoint = AbsolutePointList;

                foreach (CPoint point in listAbsolutePoint)
                {
                    sumX += point.X;
                    sumY += point.Z;
                }

                blockPoint.X = sumX / listAbsolutePoint.Count;
                blockPoint.Z = sumY / listAbsolutePoint.Count;
            }
            else
            {
                double minX, maxX, minY, maxY;
                minX = maxX = minY = maxY = 0;

                getMinMaxX(ref minX, ref maxX);
                getMinMaxY(ref minY, ref maxY);

                CPoint retBlockPoint = m_shapeTools.findInsidePoint(this, minX, maxX, minY, maxY);

                if (retBlockPoint != null)
                    blockPoint = retBlockPoint;
                else
                {
                    // 예외 처리를 한다.
                    blockPoint.X = 0;
                    blockPoint.Z = 0;
                }
            }

            return blockPoint;
        }

        /// <summary>
        /// Face 의 최대, 최소 X 좌표값을 얻어낸다.
        /// 
        /// 절대좌표의 최대최소 X 를 찾는다.
        /// 
        /// </summary>
        /// <param name="dMinX">리턴용 최소 X 값</param>
        /// <param name="dMaxX">리턴용 최대 X 값</param>
        public bool getMinMaxX(ref double dMinX, ref double dMaxX )
        {
            /// 비교 값을 초기화 한다.
            /// Max 는 아주 작은 값, Min 는 아주 큰 값으로 설정한다.
            double minX = 1e300;
            double maxX = -1e300;

            /// 아직 Point 가 존재하지 않는 경우 false 를 리턴한다.
            if (m_listRelativePoint.Count == 0)
                return false;

            /// 매번 생성하는 Property 이기 때문에 
            /// LineList 는 새로운 List에  담는 동작 한번만 호출하고, 사용은 새로운 List 를 사용한다.
            List<CPoint> listAbsolutePoint = new List<CPoint>();
            listAbsolutePoint = AbsolutePointList;

            foreach (CPoint point in listAbsolutePoint)
            {
                if (point.X < minX)  minX = point.X;
                if (point.X > maxX)  maxX = point.X;
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

        /// <summary>
        /// Face 의 최대, 최소 Y 좌표값을 얻어낸다.
        /// 
        /// 절대좌표의 최대최소 X 를 찾는다.
        /// </summary>
        /// <param name="dMinX">리턴용 최소 Y 값</param>
        /// <param name="dMaxX">리턴용 최대 Y 값</param>
        public bool getMinMaxY(ref double dMinY, ref double dMaxY)
        {
            /// 비교 값을 초기화 한다.
            /// Max 는 아주 작은 값, Min 는 아주 큰 값으로 설정한다.
            double minY = 1e300;
            double maxY = -1e300;

            /// 아직 Point 가 존재하지 않는 경우 false 를 리턴한다.
            if (m_listRelativePoint.Count == 0)
                return false;

            /// 매번 생성하는 Property 이기 때문에 
            /// LineList 는 새로운 List에  담는 동작 한번만 호출하고, 사용은 새로운 List 를 사용한다.
            List<CPoint> listAbsolutePoint = new List<CPoint>();
            listAbsolutePoint = AbsolutePointList;

            foreach (CPoint point in listAbsolutePoint)
            {
                if (point.Z < minY) minY = point.Z;
                if (point.Z > maxY) maxY = point.Z;
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
        
        /// <summary>
        /// Face 포인트 수를 얻는다.
        /// </summary>
        /// <returns>Face 포인트 수</returns>
        public int getPointCount()
        {
            return m_listRelativePoint.Count;
        }

        /// <summary>
        /// 경계조건의 부여되는 외각 Region 을 그리고 경계조건을 부여한다.
        /// </summary>
        /// <param name="femm">FEMM</param>
        /// <param name="x1">사각형 첫점의 X 좌표</param>
        /// <param name="y1">사각형 첫점의 Y 좌표</param>
        /// <param name="x2">사각형 둘째점의 X 좌표</param>
        /// <param name="y2">사각형 둘째점의 Y 좌표</param>
        /// <param name="dMeshSize"></param>
        public void setOutsideBoundary(CScriptFEMM femm, double x1, double y1, double x2, double y2, double dMeshSize = 0)
        {            
            femm.addBoundaryConditon();

            setRectanglePoints(x1, y1, x2, y2);

            double maxX = (x1 > x2) ? x1 : x2;
            double maxY = (y1 > y2) ? y1 : y2;
            double width = Math.Abs(x2 - x1);
            double height = Math.Abs(y2 - y1);

            CPoint blockPoint = new CPoint();

            // 좌상단 구석에 block point 좌표를 얻어낸다.
            // Region 은 무조건 직사각형이기 때문에 촤상단 점의 근접점이 내부점이 될 수 있다.
            blockPoint.X = maxX - width / 1000.0f;
            blockPoint.Z = maxY - height / 1000.0f;

            femm.setRegionBlockProp(blockPoint, dMeshSize);

            double sx, sy, ex, ey;

            /// 매번 생성하는 Property 이기 때문에 
            /// LineList 는 새로운 List에  담는 동작 한번만 호출하고, 사용은 새로운 List 를 사용한다.
            List<CPoint> listAbsolutePoint = new List<CPoint>();
            listAbsolutePoint = AbsolutePointList;

            // 좌변을 제외하고 하변, 상변, 우변에만 경계조건이 부여된다.
            for (int i = 0; i < listAbsolutePoint.Count; i++)
            {
                // 마지막 Point 만 제외한다.
                if (i < listAbsolutePoint.Count - 1)
                {
                    sx = listAbsolutePoint[i].X;
                    sy = listAbsolutePoint[i].Z;
                    ex = listAbsolutePoint[i + 1].X;
                    ey = listAbsolutePoint[i + 1].Z;

                    femm.drawBoundaryLine(sx, sy, ex, ey);
                }
                /// 마지막 선은 끝점과 첫점을 있는다
                /// 마지막 선은 좌변 라인으로 경계조건이 필요 없다.
                else
                {
                    sx = listAbsolutePoint[i].X;
                    sy = listAbsolutePoint[i].Z;
                    ex = listAbsolutePoint[0].X;
                    ey = listAbsolutePoint[0].Z;

                    femm.drawLine(sx, sy, ex, ey);
                }
            }
        }

        /// <summary>
        /// 경계조건의 부여되지 않는 내부 Region 을 그린다. (단, 경계조건은 부여하지 않음)
        /// </summary>
        /// <param name="femm">FEMM</param>
        /// <param name="x1">사각형 첫점의 X 좌표</param>
        /// <param name="y1">사각형 첫점의 Y 좌표</param>
        /// <param name="x2">사각형 둘째점의 X 좌표</param>
        /// <param name="y2">사각형 둘째점의 Y 좌표</param>
        /// <param name="dMeshSize">Mesh Size (0 이면 Auto Mesh)</param>
        public void setInsideRegion(CScriptFEMM femm, double x1, double y1, double x2, double y2, double dMeshSize = 0)
        {
            setRectanglePoints(x1, y1, x2, y2);

            double maxX = (x1 > x2) ? x1 : x2;
            double maxY = (y1 > y2) ? y1 : y2;
            double width = Math.Abs(x2 - x1);
            double height = Math.Abs(y2 - y1);

            CPoint blockPoint = new CPoint();

            // 좌상단 구석에 block point 좌표를 얻어낸다.
            //
            // Region 은 무조건 직사각형이기 때문에 촤상단 점의 근접점이 내부점이 될 수 있다.
            blockPoint.X = maxX - width / 1000.0f;
            blockPoint.Z = maxY - height / 1000.0f;

            femm.setRegionBlockProp(blockPoint, dMeshSize);

            double sx, sy, ex, ey;

            /// 매번 생성하는 Property 이기 때문에 
            /// LineList 는 새로운 List에  담는 동작 한번만 호출하고, 사용은 새로운 List 를 사용한다.
            List<CPoint> listAbsolutePoint = new List<CPoint>();
            listAbsolutePoint = AbsolutePointList;

            // 좌변을 제외하고 하변, 상변, 우변에만 경계조건이 부여된다.
            for (int i = 0; i < listAbsolutePoint.Count; i++)
            {
                // 마지막 Point 만 제외한다.
                if (i < listAbsolutePoint.Count - 1)
                {
                    sx = listAbsolutePoint[i].X;
                    sy = listAbsolutePoint[i].Z;
                    ex = listAbsolutePoint[i + 1].X;
                    ey = listAbsolutePoint[i + 1].Z;

                    /// 경계조건은 부여하지 않음
                    femm.drawLine(sx, sy, ex, ey);

                }
                // 마지막 선은 끝점과 첫점을 있는다
                else
                {
                    sx = listAbsolutePoint[i].X;
                    sy = listAbsolutePoint[i].Z;
                    ex = listAbsolutePoint[0].X;
                    ey = listAbsolutePoint[0].Z;

                    femm.drawLine(sx, sy, ex, ey);
                }
            }
        }

        public void writeObject(System.IO.StreamWriter writeStream, int nLevel)
        {
            CWriteFile writeFile = new CWriteFile();

            writeFile.writeBeginLine(writeStream, "Shape", nLevel);

            writeFile.writeDataLine(writeStream, "BasePointX", m_basePoint.X.ToString(), nLevel + 1);
            writeFile.writeDataLine(writeStream, "BasePointY", m_basePoint.Z.ToString(), nLevel + 1);

            writeFile.writeDataLine(writeStream, "FaceType", m_emFaceType, nLevel + 1);

            foreach (CPoint pointe in m_listRelativePoint)
            {
                writeFile.writeDataLine(writeStream, "PointX", pointe.X.ToString(), nLevel + 1);
                writeFile.writeDataLine(writeStream, "PointY", pointe.Z.ToString(), nLevel + 1);
                writeFile.writeDataLine(writeStream, "LineKind", pointe.LineKind.ToString(), nLevel + 1);
                writeFile.writeDataLine(writeStream, "ArcDriction", pointe.DirectionArc.ToString(), nLevel + 1);
            }

            writeFile.writeEndLine(writeStream, "Shape", nLevel);
        }
    }
    
    /// <summary>
    /// 포인트 객체
    /// [유의 사항]
    ///  - 포인트 정보와 라인 정보를 모두 가지고 있다.
    ///    * 포인트 정보 : X,Y 좌표값
    ///    * 라인 정보 : 현 포인트에서 시작되는 라인의 종류와 Arc 그리기 방향
    ///  - 관련 라인은 현 포인터의 X,Y 좌표값으로 시작해서, CPoint List 다음 포인트의 X,Y 좌표값으로 끝난다.
    /// </summary>
    public class CPoint
    {
        private double m_dX;
        private double m_dZ;
        private EMLineKind m_emLineKind;
        private EMDirectionArc m_emDirectionArc;

        public double X
        {
            get { return m_dX; }
            set { m_dX = value; }
        }

        public double Z
        {
            get { return m_dZ; }
            set { m_dZ = value; }
        }

        public EMLineKind LineKind
        {
            get { return m_emLineKind; }
            set { m_emLineKind = value; }
        }

        public EMDirectionArc DirectionArc
        {
            get { return m_emDirectionArc; }
            set { m_emDirectionArc = value; }
        }

        public CPoint()
        {
            m_dX = 0.0;
            m_dZ = 0.0;

            m_emLineKind = EMLineKind.STRAIGHT;
            m_emDirectionArc = EMDirectionArc.FORWARD;
        }

        public CPoint(double x, double y, EMLineKind kind, EMDirectionArc direction)
        {
            m_dX = x;
            m_dZ = y;

            m_emLineKind = kind;
            m_emDirectionArc = direction;
        }
    }

    /// <summary>
    /// Line 객체
    /// [유의 사항]
    ///  - 라인 정보를 시작점이 가지고 있고, 정작 라인에는 시작과 끝점만 가지고 있다.
    ///    따라서 라인의 정보는 시작점으로 저장하고 있다. 
    /// </summary>
    public class CLine
    {
        public CPoint m_startPoint = new CPoint();
        public CPoint m_endPoint = new CPoint();

        public CLine()
        {
            m_startPoint.X = 0;
            m_startPoint.Z = 0;
            m_endPoint.X = 0;
            m_endPoint.Z = 0;

            m_startPoint.LineKind = EMLineKind.STRAIGHT;
            m_startPoint.DirectionArc = EMDirectionArc.FORWARD;
        }

        public CLine(double x1, double y1, double x2, double y2, EMLineKind kind, EMDirectionArc direction)
        {
            m_startPoint.X = x1;
            m_startPoint.Z = y1;
            m_endPoint.X = x2;
            m_endPoint.Z = y2;

            /// Line 정보는 Start 점에 담겨있는 정보를 사용한다.
            m_startPoint.LineKind = kind;
            m_startPoint.DirectionArc = direction;
        }

        public CLine(CPoint start, CPoint end)
        {
            m_startPoint.X = start.X;
            m_startPoint.Z = start.Z;
            m_endPoint.X = end.X;
            m_endPoint.Z = end.Z;

            /// Line 정보는 Start 점에 담겨있는 정보를 사용한다.
            m_startPoint.LineKind = start.LineKind;
            m_startPoint.DirectionArc = start.DirectionArc;
        }
    }

    class CShapeTools
    {
        // double 형인 좌표 값을 비교할 때 
        // 유효 동일한 값이 다른 값으로 처리되는 문제를 해결하기 위해 
        // 특정 이하의 자리수를 반올림한 다음 비교한다.
        private bool isEqual(double a, double b)
        {
            return Math.Equals(roundDigitOfShape(a), roundDigitOfShape(b));
        }        
        /// <summary>
        /// 짝수인지 홀수인지 아니면 영인지를 판단한다.
        /// </summary>
        private EMNumberKind getNumberKind(int num)
        {
            // even number
            if (num == 0)
                return EMNumberKind.ZERO;
            // even number
            else if (num % 2 == 0)
                return EMNumberKind.EVEN;
            // odd number
            else
                return EMNumberKind.ODD;
        }

        /// <summary>
        /// 선 위에 다른 점이 올라가 있는 지를 검사한다.
        /// </summary>
        public bool isPerchedOnLine(CLine line, CPoint point)
        {
            double dL_P1_X = line.m_startPoint.X;
            double dL_P1_Y = line.m_startPoint.Z;

            double dL_P2_X = line.m_endPoint.X;
            double dL_P2_Y = line.m_endPoint.Z;

            double dP_X = point.X;
            double dP_Y = point.Z;

            /// 라인의 X 구간 
            double dBigX = Math.Max(dL_P1_X, dL_P2_X);
            double dSmallX = Math.Min(dL_P1_X, dL_P2_X);

            /// 라인의 Y 구간 
            double dBigY = Math.Max(dL_P1_Y, dL_P2_Y);
            double dSmallY = Math.Min(dL_P1_Y, dL_P2_Y);

            /// 직선이 수직선인 경우는 예외 처리한다.
            if (isEqual(dL_P2_X, dL_P1_X))
            {
                /// 점의 X 좌표가 직선의 X 좌표와 일치하면 라인 위의 점이다.
                if (isEqual(dL_P1_X, dP_X))
                {
                    if (roundDigitOfShape(dBigY) >= roundDigitOfShape(dP_Y) && roundDigitOfShape(dSmallY) <= roundDigitOfShape(dP_Y))
                        return true;
                }
            }
            /// 직선이 수평선인 경우는 예외 처리한다.
            else if (isEqual(dL_P2_Y, dL_P1_Y))
            {
                /// 점의 Y 좌표가 직선의 Y 좌표와 일치하면 라인 위의 점이다.
                if (isEqual(dL_P1_Y, dP_Y))
                {
                    if (roundDigitOfShape(dBigX) >= roundDigitOfShape(dP_X) && roundDigitOfShape(dSmallX) <= roundDigitOfShape(dP_X))
                        return true;
                }
            }
            else
            {
                /// 라인의 직선방정식 기울기와 Y 절편을 계산한다.
                double dL_A = (dL_P2_Y - dL_P1_Y) / (dL_P2_X - dL_P1_X);
                double dL_B = dL_P1_Y - dL_A * (dL_P1_X);

                /// 라인의 직선방정식에 점의 X 값을 입력하여 Y 값을 계산한다.
                double dP_Calc_Y = dL_A * dP_X + dL_B;

                /// 계산된 Y 값과 좌표 Y 값이 일치하면 직선의 방정식위에 있는 점이다.
                if (isEqual(dP_Calc_Y, dP_Y))
                {
                    /// 직선의 방정식 위에 있는 점 중에서 실제 라인 위에 있는 점인지를 판단한다.
                    if ((roundDigitOfShape(dBigX) >= roundDigitOfShape(dP_X) && roundDigitOfShape(dSmallX) <= roundDigitOfShape(dP_X)) && (roundDigitOfShape(dBigY) >= roundDigitOfShape(dP_Y) && roundDigitOfShape(dSmallY) <= roundDigitOfShape(dP_Y)))
                        return true;
                }
            }

            return false;
        }

        public double roundDigitOfShape(double a)
        {
            // 기본 좌표 단위는 mm 이다. 
            // 따라서 0.000001 mm (즉, nm) 까지만 사용하고 반올림을 처리한다.
            const int VALID_DIGIT = 6;

            return Math.Round(a, VALID_DIGIT);
        }

        /// <summary>
        /// 두 라인의 교차를 확인한다.
        /// 단, 한 선의 점이 다른 선위에 올라가거나, 다른 선의 점과 겹치는 것은 고려하지 않는다. 
        /// </summary>
        public bool isIntersected(CLine firstLine, CLine secondLine)
        {
            double dFL_P1_X = roundDigitOfShape(firstLine.m_startPoint.X);
            double dFL_P1_Y = roundDigitOfShape(firstLine.m_startPoint.Z);

            double dFL_P2_X = roundDigitOfShape(firstLine.m_endPoint.X);
            double dFL_P2_Y = roundDigitOfShape(firstLine.m_endPoint.Z);

            double dSL_P1_X = roundDigitOfShape(secondLine.m_startPoint.X);
            double dSL_P1_Y = roundDigitOfShape(secondLine.m_startPoint.Z);

            double dSL_P2_X = roundDigitOfShape(secondLine.m_endPoint.X);
            double dSL_P2_Y = roundDigitOfShape(secondLine.m_endPoint.Z);

            /// 알고리즘 2 번
            /// 
            /// ">" 를 ">=" 로 변경한 것은 꼭지점끼리 만나는 경우는 교차로 취급하지 않기 위해서이다.
            if (((dFL_P1_X - dFL_P2_X) * (dSL_P1_Y - dFL_P1_Y) + (dFL_P1_Y - dFL_P2_Y) * (dFL_P1_X - dSL_P1_X)) *
                ((dFL_P1_X - dFL_P2_X) * (dSL_P2_Y - dFL_P1_Y) + (dFL_P1_Y - dFL_P2_Y) * (dFL_P1_X - dSL_P2_X)) >= 0.0)
                return false;

            if (((dSL_P1_X - dSL_P2_X) * (dFL_P1_Y - dSL_P1_Y) + (dSL_P1_Y - dSL_P2_Y) * (dSL_P1_X - dFL_P1_X)) *
                ((dSL_P1_X - dSL_P2_X) * (dFL_P2_Y - dSL_P1_Y) + (dSL_P1_Y - dSL_P2_Y) * (dSL_P1_X - dFL_P2_X)) >= 0.0)
                return false;

            return true;
        }

        /// <summary>
        /// 두 라인의 겹침을 확인한다.
        /// 
        /// 두선이 같은 직선위에 있는지를 판단하여 겹침을 검사한다.
        /// 단, 같은 직선위에 있더라도 떨어져 있는 직선일 수 있기 때문에 양점의 X 좌표를 비교해서 겹치는 구간이 있는지 판단한다.        /// 
        /// </summary>
        public bool isOverlaped(CLine firstLine, CLine secondLine)
        {
            double dFL_P1_X = firstLine.m_startPoint.X;
            double dFL_P1_Y = firstLine.m_startPoint.Z;

            double dFL_P2_X = firstLine.m_endPoint.X;
            double dFL_P2_Y = firstLine.m_endPoint.Z;

            double dSL_P1_X = secondLine.m_startPoint.X;
            double dSL_P1_Y = secondLine.m_startPoint.Z;

            double dSL_P2_X = secondLine.m_endPoint.X;
            double dSL_P2_Y = secondLine.m_endPoint.Z;

            /// 첫번째 라인의 직선방정식 기울기와 Y 절편을 계산한다.
            double dA = (dFL_P2_Y - dFL_P1_Y) / (dFL_P2_X - dFL_P1_X);
            double dB = dFL_P1_Y - dA * (dFL_P1_X);

            /// 첫번째 라인의 직선방정식에 두번째 라인 두점의 X 값을 입력하여 Y 값을 계산한다.
            double dStartY = dA * dSL_P1_X + dB;
            double dEndY = dA * dSL_P2_X + dB;

            double dBigX = Math.Max(dFL_P1_X, dFL_P2_X);
            double dSmallX = Math.Min(dFL_P1_X, dFL_P2_X);

            // 계산된 Y 값과 실제 두번째 라인 두점의 값이 일치하면 
            // 하나의 직선 방정식 위에 두개의 라인이 존재한다고 판단할 수 있다.
            if (isEqual(dStartY, dSL_P1_Y) && isEqual(dEndY, dSL_P2_Y))
            {
                // 하나의 직선 방정식 위에 두개의 라인이 존재하더라도 두 라인이 떨어져 있을 수 있다.
                // 하나의 직선 방정식 위에 존재하면서 두개의 라인이 겹치는지를 아래의 조건을 판단하고 있다.
                // ( 방법은 두번째 라인의 두점의 X 값이 첫번째 라인의 X 값들 사이에 있는지로 검사하고 있다.)
                if (roundDigitOfShape(dBigX) >= roundDigitOfShape(dSL_P1_X) && roundDigitOfShape(dSmallX) <= roundDigitOfShape(dSL_P1_X) || roundDigitOfShape(dBigX) >= roundDigitOfShape(dSL_P2_X) && roundDigitOfShape(dSmallX) <= roundDigitOfShape(dSL_P2_X))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        /// <summary>
        /// 두 라인의 접촉을 확인한다.
        /// 
        /// 하나의 선에 다른 선의 두점 중 한점이라도 올라탔는지를 확인해서 접촉을 판단한다.
        /// </summary>
        public bool isContacted(CLine firstLine, CLine secondLine)
        {
            // 첫번째 라인에 두번째 라인의 양점이 올라갔는지를 판단한다.
            if (true == isPerchedOnLine(firstLine, secondLine.m_startPoint))
                return true;

            if (true == isPerchedOnLine(firstLine, secondLine.m_endPoint))
                return true;

            // 두번째 라인에 첫번째 라인의 양점이 올라갔는지를 판단한다.
            if (true == isPerchedOnLine(secondLine, firstLine.m_startPoint))
                return true;

            if (true == isPerchedOnLine(secondLine, firstLine.m_endPoint))
                return true;

            return false;
        }
        
        /// <summary>
        /// Face 의 면적을 계산한다.
        /// 
        /// 면적계산을 상대좌표를 상관이 없기 때문에 사용한다.
        public double calcArea(CFace face)
        {
            double dArea = 0;

            double minX = 0;
            double maxX = 0;
            double minY = 0;
            double maxY = 0;

            /// Rectangle 은 4 좌표의 평균점을 사용한다.
            if (face.FaceType == EMFaceType.RECTANGLE)
            {
                face.getMinMaxX(ref minX, ref maxX);
                face.getMinMaxY(ref minY, ref maxY);

                /// 가로, 세로 곱
                dArea = Math.Abs((maxX - minX) * (maxY - minY));
            }
            // 다각형 면적계산 알고리즘 : https://mathworld.wolfram.com/PolygonArea.html
            else
            {
                // 가장 마지막 항 (xn*y1 과 yn*x1) 을 첫번째로 우선처리하기 위한 설정한다.
                //
                // - 계산 순서는 점이 3개라면 자료의 수식 순서와 달리 아래와 같이 마지막항이 가장 앞으로 위치한다.
                //   x3y1 + x1y2 + x2y3
                //   y3x1 + y1x2 + y2x3
                int j = face.RelativePointList.Count - 1;

                for (int i = 0; i < face.RelativePointList.Count; i++)
                {
                    dArea += (face.RelativePointList[j].X * face.RelativePointList[i].Z) - (face.RelativePointList[j].Z * face.RelativePointList[i].X);
 
                    j = i;  /// j is previous vertex to i
                }

                dArea = Math.Abs(dArea / 2.0f);
            }

            return dArea;

        }

        /// <summary>
        /// 재귀호출을 사용해서 FACE 내부점을 찾아낸다.
        /// 재질 블럭점을 찾는 것이기 때문에 절대좌표를 사용해야 한다.
        /// 
        /// 참고 사이트 : http://bowbowbow.tistory.com/24
        /// </summary>
        /// <param name="minX">내부점을 찾는 구간의 X 점 최소값</param>
        /// <param name="maxX">내부점을 찾는 구간의 X 점 최대값</param>
        /// <param name="baseY">내부 좌표값을 찾은 Y 위치 </param>
        /// <returns></returns>
        public CPoint findInsidePoint(CFace face, double minX, double maxX, double minY, double maxY)
        {
            int nRightIntersection = 0;
            int nLeftIntersection = 0;

            int nRightPerchedPointOnCheckLine = 0;
            int nLeftPerchedPointOnCheckLine = 0;

            int nPerchedCenterPointOnFaceLine = 0;

            CLine rightCheckLine = new CLine();
            CLine leftCheckLine = new CLine();

            CPoint centerPoint = new CPoint();

            double baseY = (minY + maxY) / 2.0f;
            double centerX = (minX + maxX) / 2.0f;

            //// 너무 작은 소수점 이하는 정리한다.
            minX = roundDigitOfShape(minX);
            maxX = roundDigitOfShape(maxX);
            baseY = roundDigitOfShape(baseY);

            // 중심점을 만든다.
            centerPoint.X = centerX;
            centerPoint.Z = baseY;

            /// create a right check line
            rightCheckLine.m_startPoint.X = centerX;
            rightCheckLine.m_startPoint.Z = baseY;
            rightCheckLine.m_endPoint.X = maxX + 10.0f;    // 오른 검색선의 끝을 maxX 보다 10 크게 한다.
            rightCheckLine.m_endPoint.Z = baseY;

            /// create a left check line
            leftCheckLine.m_startPoint.X = centerX;
            leftCheckLine.m_startPoint.Z = baseY;
            leftCheckLine.m_endPoint.X = minX - 10.0f;    // 오른 검색선의 끝을 minX 보다 10 작게 한다.
            leftCheckLine.m_endPoint.Z = baseY;

            /// 매번 생성하는 Property 이기 때문에 
            /// LineList 는 새로운 List에  담는 동작 한번만 호출하고, 사용은 새로운 List 를 사용한다.
            List<CLine> listAbsoluteLine = new List<CLine>();
            listAbsoluteLine = face.AbsoluteLineList;
            
            foreach (CLine line in listAbsoluteLine)
            {
                /// Face 선들과 검색 선의 교차 횟수 확인
                if (true == isIntersected(line, rightCheckLine))
                    nRightIntersection++;

                if (true == isIntersected(line, leftCheckLine))
                    nLeftIntersection++;

                /// Face 선의 양점이 검색 선 위에 올라가 있는지 확인
                if (true == isPerchedOnLine(rightCheckLine, line.m_startPoint))
                    nRightPerchedPointOnCheckLine++;

                if (true == isPerchedOnLine(rightCheckLine, line.m_endPoint))
                    nRightPerchedPointOnCheckLine++;

                if (true == isPerchedOnLine(leftCheckLine, line.m_startPoint))
                    nLeftPerchedPointOnCheckLine++;

                if (true == isPerchedOnLine(leftCheckLine, line.m_endPoint))
                    nLeftPerchedPointOnCheckLine++;

                /// Face 선위에 중심점이 올라가 있는지 확인
                if (true == isPerchedOnLine(line, centerPoint))
                    nPerchedCenterPointOnFaceLine++;
            }


            //-------------------------------------------------------------------
            // 내부점 판단
            //
            // 내부점을 만족하면 Point 를 리턴하고,
            // 만족하지 못하면 측정 면적을 수정하고 재귀호출을 통해 Point 를 찾아낸다.
            //------------------------------------------------------------------
            //
            CPoint point = new CPoint();

            // Center 점이 Face Line 위에 올라가 있으면 우측 1/2 사각형에서 내부점을 찾는다.
            if (nPerchedCenterPointOnFaceLine > 0)
            {
                return findInsidePoint(face, centerX, maxX, minY, maxY);
            }

            // Face Line 의 양점이 우측 검색 라인에 올라가는 경우는 상측 1/2 사각형에서 내부점을 찾는다.
            if (nRightPerchedPointOnCheckLine > 0 || nLeftPerchedPointOnCheckLine > 0)
            {
                return findInsidePoint(face, minX, maxX, baseY, maxY);
            }

            // 양측이 홀수이면 Inside Point 이다.
            if (EMNumberKind.ODD == getNumberKind(nRightIntersection) && EMNumberKind.ODD == getNumberKind(nLeftIntersection))
            {
                point.X = centerX;
                point.Z = baseY;

                return point;
            }
            /// 왼쪽이 짝수이면 X 값의 최소값과 중심값 사이의 중점을 다시 확인한다.
            else if (EMNumberKind.EVEN == getNumberKind(nLeftIntersection))
            {
                return findInsidePoint(face, minX, centerX, minY, maxY);
            }
            /// 오른쪽이 짝수이면 X 값의 중심값과 최대값 사이의 중점을 다시 확인한다.
            else if (EMNumberKind.EVEN == getNumberKind(nRightIntersection))
            {
                return findInsidePoint(face, centerX, maxX, minY, maxY);
            }
            else
            {
                /// Block Point 를 찾기 위해서 findInsidePoint() 를 호출할 때
                /// Face 형상의 문제로 오류가 발생하여 Face 바깥의 지점으로 계산이 리턴되면
                /// Block Point 가 추가되지 못해서
                /// FEMM 에서 Block Point 에 재질 인가 할 때 다른 Block Point 에 인가되는 문제가 발생한다.
                /// 
                /// 따라서 findInsidePoint() 에서 내부점을 찾지 못할 때는
                /// 중심의 좌표값을 넘기지 않고 null 을 리턴하여 Block Point 재질 설정 동작을 막는다.
                CNotice.noticeWarningID("FTCT");
                return null;
            }
        }
    }
}
