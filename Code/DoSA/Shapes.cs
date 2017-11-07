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

                // foreach 를 사용해서 읽어내서 더하게 되면 계속 증가하는 문제가 발생한다.
                // for 문에서 매번 point 를 new 를 하면 Add 하라.
                for (int i=0; i < m_listRelativePoint.Count; i++ )
                {
                    point = new CPoint();
                    point.m_dX = m_listRelativePoint[i].m_dX + m_basePoint.m_dX;
                    point.m_dY = m_listRelativePoint[i].m_dY + m_basePoint.m_dY;
                    point.m_emDirectionArc = m_listRelativePoint[i].m_emDirectionArc;
                    point.m_emLineKind = m_listRelativePoint[i].m_emLineKind;

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
                if (m_listRelativePoint.Count < 4)
                {
                    CNotice.printTrace("Face 의 좌표점이 4개 보다 작은 상태에서 Line List 를 얻으려 하고 있다.");
                    return null;
                }

                List<CLine> listLine = new List<CLine>();
                CPoint startPoint = null;
                CPoint endPoint = null;
                
                for (int i = 0; i < m_listRelativePoint.Count; i++)
                {
                    // 기준 점인 Base Point 의 좌표와 상대좌표인 PointList 를 더해서
                    // 절대좌표값을 계산한 후에 형상을 그리기 위한 LineList 를 만든다
                    startPoint = new CPoint();
                    startPoint.m_dX = m_listRelativePoint[i].m_dX + m_basePoint.m_dX;
                    startPoint.m_dY = m_listRelativePoint[i].m_dY + m_basePoint.m_dY;
                    startPoint.m_emDirectionArc = m_listRelativePoint[i].m_emDirectionArc;
                    startPoint.m_emLineKind = m_listRelativePoint[i].m_emLineKind;

                    // 마지막 Point 을 제외한 나머지 경우
                    if (i < m_listRelativePoint.Count - 1)
                    {
                        // 기준 점인 Base Point 의 좌표와 상대좌표인 PointList 를 더해서
                        // 절대좌표값을 계산한 후에 형상을 그리기 위한 LineList 를 만든다
                        endPoint = new CPoint();
                        endPoint.m_dX = m_listRelativePoint[i + 1].m_dX + m_basePoint.m_dX;
                        endPoint.m_dY = m_listRelativePoint[i + 1].m_dY + m_basePoint.m_dY;
                        endPoint.m_emDirectionArc = m_listRelativePoint[i + 1].m_emDirectionArc;
                        endPoint.m_emLineKind = m_listRelativePoint[i + 1].m_emLineKind;
                    }
                    /// 마지막인 Point 경우
                    else
                    {
                        endPoint = new CPoint();
                        endPoint.m_dX = m_listRelativePoint[0].m_dX + m_basePoint.m_dX;
                        endPoint.m_dY = m_listRelativePoint[0].m_dY + m_basePoint.m_dY;
                        endPoint.m_emDirectionArc = m_listRelativePoint[0].m_emDirectionArc;
                        endPoint.m_emLineKind = m_listRelativePoint[0].m_emLineKind;
                    }

                    listLine.Add(new CLine(startPoint, endPoint));
                }

                return listLine;
            }
        }

        public CFace()
        {
            m_basePoint.m_dX = 0.0f;
            m_basePoint.m_dY = 0.0f;
        }

        public bool addPoint(CPoint point)
        {
            m_listRelativePoint.Add(point);

            return true;
        }

        /// <summary>
        /// 하나의 Face 안의 라인 간섭이 있는지 확인한다.
        /// 
        /// Face 내부의 간섭은 상대좌표 사용해도 가능한다.
        /// </summary>
        public bool checkFaceShape()
        {
            /// 축대칭 모델만 사용하기 때문에 
            /// 내부 포인트의 X 좌표값은 항상 영보다 커야 한다.
            foreach(CPoint point in m_listRelativePoint)
            {
                if(point.m_dX < 0)
                    return false;
            }

            /// 매번 생성하는 Property 이기 때문에 
            /// LineList 는 새로운 List에  담는 동작 한번만 호출하고, 사용은 새로운 List 를 사용한다.
            List<CLine> listAbsoluteLine = new List<CLine>();
            listAbsoluteLine = AbsoluteLineList;

            /// 아래와 같이 라인의 교차간섭을 비교한다.
            /// 1 line <=> 2 line ... last line
            /// 2 line <=> 3 line ... last line
            /// ...
            /// lsat-2 line <=> last-1 line , last line
            /// last-1 line <=> last line 
            for (int i = 0; i < listAbsoluteLine.Count - 1; i++)
            {
                for (int j = i + 1; j < listAbsoluteLine.Count; j++)
                {
                    if (true == checkIntersectionLine(listAbsoluteLine[i], listAbsoluteLine[j]))
                    {
                        /// 사용 중지
                        ///
                        /// 교차 알림창 전에 교차형상이 표시되어 사용자가 당황하게 한다.를 당황시킨다.
                        ///
                        ///--------------------------------------------------
                        /// 교차가 발생하는 라인들을 표시한다.
                        ///--------------------------------------------------
                        //CProgramFEMM.exitFEMM();

                        //CScriptFEMM femm = new CScriptFEMM();

                        //femm.drawLine(listLine[i]);
                        //femm.drawLine(listLine[j]);

                        //femm.zoomFit();
                        /////--------------------------------------------------
                        
                        /// 교차 간섭 함
                        return false;
                    }
                }
            }

            /// 교차 간섭하지 않음
            return true;
        }

        /// <summary>
        /// 라인 간의 교차를 확인한다.
        /// 참조 사이트 : http://www5d.biglobe.ne.jp/~tomoya03/shtml/algorithm/IntersectionEX.htm
        /// </summary>
        /// If 문 안에서 
        /// < 와 > 를 사용하면 끝점이 만나는 것도 교차를 판단하고,
        /// <= 와 >= 를 사용하면 끝점이 만다는 것은 교차로 인정하지 않고 두 직선이 완전히 교차할 때만 인정한다.
        public bool checkIntersectionLine(CLine first, CLine second)
        {
            CPoint start1 = new CPoint();
            CPoint end1 = new CPoint();
            CPoint start2 = new CPoint();
            CPoint end2 = new CPoint();

            start1.m_dX = first.m_startPoint.m_dX;
            start1.m_dY = first.m_startPoint.m_dY;

            end1.m_dX = first.m_endPoint.m_dX;
            end1.m_dY = first.m_endPoint.m_dY;

            start2.m_dX = second.m_startPoint.m_dX;
            start2.m_dY = second.m_startPoint.m_dY;

            end2.m_dX = second.m_endPoint.m_dX;
            end2.m_dY = second.m_endPoint.m_dY;

            ///// 교차 했음에도 교차가 없는 것으로 리턴이 되어서 교차 알고리즘 1안은 사용을 중지한 상태이다.
            /////
            ///// x 좌표에 의한 체크 
            //if (start1.m_dX >= end1.m_dX)
            //{
            //    if ((start1.m_dX <= start2.m_dX && start1.m_dX <= end2.m_dX) || (end1.m_dX >= start2.m_dX && end1.m_dX >= end2.m_dX))
            //        return false;
            //}
            //else
            //{
            //    if ((end1.m_dX <= start2.m_dX && end1.m_dX <= end2.m_dX) || (start1.m_dX >= start2.m_dX && start1.m_dX >= end2.m_dX))
            //        return false;
            //}

            ///// y 좌표에 의한 체크 
            //if (start1.m_dY >= end1.m_dY)
            //{
            //    if ((start1.m_dY <= start2.m_dY && start1.m_dY <= end2.m_dY) || (end1.m_dY >= start2.m_dY && end1.m_dY >= end2.m_dY))
            //        return false;
            //}
            //else
            //{
            //    if ((end1.m_dY <= start2.m_dY && end1.m_dY <= end2.m_dY) || (start1.m_dY >= start2.m_dY && start1.m_dY >= end2.m_dY))
            //        return false;
            //}

            if (((start1.m_dX - end1.m_dX) * (start2.m_dY - start1.m_dY) + (start1.m_dY - end1.m_dY) * (start1.m_dX - start2.m_dX)) *
                ((start1.m_dX - end1.m_dX) * (end2.m_dY - start1.m_dY) + (start1.m_dY - end1.m_dY) * (start1.m_dX - end2.m_dX)) >= 0.0)
                return false;

            if (((start2.m_dX - end2.m_dX) * (start1.m_dY - start2.m_dY) + (start2.m_dY - end2.m_dY) * (start2.m_dX - start1.m_dX)) *
                ((start2.m_dX - end2.m_dX) * (end1.m_dY - start2.m_dY) + (start2.m_dY - end2.m_dY) * (start2.m_dX - end1.m_dX)) >= 0.0)
                return false;

            return true;
        }

        /// <summary>
        ///  사각형의 4 포인트를 생성하고 저장한다.
        /// </summary>
        public bool setRectanglePoints(double x1, double y1, double x2, double y2)
        {
            double bigX, smallX;
            double bigY, smallY;

            if(x1 == x2 || y1 == y2)
                return false;

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

            if(m_listRelativePoint.Count < 4)
            {
                CNotice.printTrace("4 개보다 작은점을 가진 Face 의 형상을 그리려고 하고 있다.");
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
                // 마지막 PointLine 만 제외한다.
                if (i < listAbsolutePoint.Count - 1)
                {
                    x1 = listAbsolutePoint[i].m_dX;
                    y1 = listAbsolutePoint[i].m_dY;
                    x2 = listAbsolutePoint[i + 1].m_dX;
                    y2 = listAbsolutePoint[i + 1].m_dY;
                }
                // 마지막 선은 끝점과 첫점을 있는다
                else
                {
                    x1 = listAbsolutePoint[i].m_dX;
                    y1 = listAbsolutePoint[i].m_dY;
                    x2 = listAbsolutePoint[0].m_dX;
                    y2 = listAbsolutePoint[0].m_dY;
                }

                if (listAbsolutePoint[i].m_emLineKind == EMLineKind.ARC)
                {
                    bDirectionArc = (listAbsolutePoint[i].m_emDirectionArc == EMDirectionArc.BACKWARD ? true : false);
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
            if(listPoint.Count < 4)
            {
                CNotice.printTrace("4개 미만의 좌표점으로 다각형을 만들려고 하고 있습니다.");
                return false;
            }

            m_listRelativePoint.Clear();

            m_emFaceType = EMFaceType.POLYGON;

            foreach (CPoint pointLine in listPoint)
                m_listRelativePoint.Add(pointLine);

            return true;
        }

        /// <summary>
        /// 짝수인지 홀수인지 아니며 영인지를 판단한다.
        /// </summary>
        private EMNumberKind checkNumber(int num)
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
        /// 재귀호출을 사용해서 FACE 내부점을 찾아낸다.
        /// 재질 블럭점을 찾는 것이기 때문에 절대좌표를 사용해야 한다.
        /// 
        /// 참고 사이트 : http://bowbowbow.tistory.com/24
        /// </summary>
        /// <param name="minX">내부점을 찾는 구간의 X 점 최소값</param>
        /// <param name="maxX">내부점을 찾는 구간의 X 점 최대값</param>
        /// <param name="baseY">내부 좌표값을 찾은 Y 위치 </param>
        /// <returns></returns>
        private CPoint findInsidePoint(double minX, double maxX, double baseY)
        {
            int nRightIntersection = 0;
            int nLeftIntersection = 0;

            double centerX = (minX + maxX) / 2.0f;

            CLine rightLine = new CLine();
            CLine leftLine = new CLine();

            /// create a right check line
            rightLine.m_startPoint.m_dX = centerX;
            rightLine.m_startPoint.m_dY = baseY;
            rightLine.m_endPoint.m_dX = 1e300;
            rightLine.m_endPoint.m_dY = baseY;

            /// create a left check line
            leftLine.m_startPoint.m_dX = centerX;
            leftLine.m_startPoint.m_dY = baseY;
            leftLine.m_endPoint.m_dX = -1e300;
            leftLine.m_endPoint.m_dY = baseY;

            /// 매번 생성하는 Property 이기 때문에 
            /// LineList 는 새로운 List에  담는 동작 한번만 호출하고, 사용은 새로운 List 를 사용한다.
            List<CLine> listAbsoluteLine = new List<CLine>();
            listAbsoluteLine = AbsoluteLineList;

            foreach (CLine line in listAbsoluteLine)
            {
                if (true == checkIntersectionLine(line, rightLine))
                    nRightIntersection ++;

                if (true == checkIntersectionLine(line, leftLine))
                    nLeftIntersection ++;
            }
            
            CPoint point = new CPoint();

            /// 양측이 홀수이면 Inside Point 이다.
            if (EMNumberKind.ODD == checkNumber(nRightIntersection) && EMNumberKind.ODD == checkNumber(nLeftIntersection))
            {
                point.m_dX = centerX;
                point.m_dY = baseY;

                return point;
            }
            /// 왼쪽이 짝수이면 X 값의 최소값과 중심값 사이의 중점을 다시 확인한다.
            else if (EMNumberKind.EVEN == checkNumber(nLeftIntersection))
            {
                return findInsidePoint(minX, centerX, baseY);
            }
            /// 오른쪽이 짝수이면 X 값의 중심값과 최대값 사이의 중점을 다시 확인한다.
            else if (EMNumberKind.EVEN == checkNumber(nRightIntersection))
            {
                return findInsidePoint(centerX, maxX, baseY);
            }
            else
            {
                CNotice.printTrace("Inside Point 를 찾지 못하는 알고리즘 오류가 발생했습니다.");

                // 어쩔수 없이 중심값을 리턴한다.
                point.m_dX = centerX;
                point.m_dY = baseY;

                return point;
            }
        }

        /// <summary>
        /// Face 의 면적을 계산한다.
        /// 단, 다각형의 면적 계산은 정확하지 않다. (개선 필요함)
        ///
        /// 면적계산을 상대좌표를 상관이 없기 때문에 사용한다.
        /// </summary>
        /// <returns>Face 면적</returns>
        public double calcArea()
        {
            double dArea = 0;

            double minX = 0;
            double maxX = 0;
            double minY = 0;
            double maxY = 0;

            /// Rectangle 은 4 좌표의 평균점을 사용한다.
            if (FaceType == EMFaceType.RECTANGLE)
            {
                getMinMaxX(ref minX, ref maxX);
                getMinMaxY(ref minY, ref maxY);

                /// 가로, 세로 곱
                return (maxX - minX) * (maxY - minY);
            }
            /// Polygon 의 다각형 면적 계산은 단순한 형상에서만 가능하고 오차가 있는 것으로 판단된다.
            /// 추후 : 검증하라.
            else
            {
                /// The last vertex is the 'previous' one to the first
                int j = m_listRelativePoint.Count - 1;  
                for (int i = 0; i < m_listRelativePoint.Count; i++)
                {
                    dArea += (m_listRelativePoint[j].m_dX + m_listRelativePoint[i].m_dX) * (m_listRelativePoint[j].m_dY - m_listRelativePoint[i].m_dY);

                    j = i;  /// j is previous vertex to i
                }
                return Math.Abs(dArea / 2.0f);
            }
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
                    sumX += point.m_dX;
                    sumY += point.m_dY;
                }

                blockPoint.m_dX = sumX / listAbsolutePoint.Count;
                blockPoint.m_dY = sumY / listAbsolutePoint.Count;
            }
            else
            {
                double minX = 0;
                double maxX = 0;
                double minY = 0;
                double maxY = 0;

                getMinMaxX(ref minX, ref maxX);
                getMinMaxY(ref minY, ref maxY);

                blockPoint = findInsidePoint(minX, maxX, (minY + maxY) / 2.0f);
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
                if (point.m_dX < minX)  minX = point.m_dX;
                if (point.m_dX > maxX)  maxX = point.m_dX;
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
                if (point.m_dY < minY) minY = point.m_dY;
                if (point.m_dY > maxY) maxY = point.m_dY;
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

        public bool insertPoint(int nIndex, CPoint pointLine)
        {
            // nIndex 가 List 크기보다 크면 바로 리턴한다.
            if (nIndex >= m_listRelativePoint.Count)
                return false;

            //**************************************************
            // 동작 확인이 필요함
            //**************************************************
            m_listRelativePoint.Insert(nIndex + 1, pointLine);
            return true;
        }

        public bool deletePoint(int nIndex)
        {
            // nIndex 가 List 크기보다 크면 바로 리턴한다.
            if (nIndex >= m_listRelativePoint.Count)
                return false;

            m_listRelativePoint.RemoveAt(nIndex);
            return true;
        }

        public void deleteAllPoint()
        {
            m_listRelativePoint.Clear();
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
        internal void setOutsideBoundary(CScriptFEMM femm, double x1, double y1, double x2, double y2, double dMeshSize = 0)
        {            
            femm.addBoundaryConditon();

            setRectanglePoints(x1, y1, x2, y2);

            double maxX = (x1 > x2) ? x1 : x2;
            double maxY = (y1 > y2) ? y1 : y2;
            double width = Math.Abs(x2 - x1);
            double height = Math.Abs(y2 - y1);

            CPoint blockPoint = new CPoint();

            // 좌상단 구석에 block point 좌표를 얻어낸다.
            blockPoint.m_dX = maxX - width / 100.0f;
            blockPoint.m_dY = maxY - height / 100.0f;

            femm.setRegionBlockProp(blockPoint, dMeshSize);

            double sx, sy, ex, ey;

            /// 매번 생성하는 Property 이기 때문에 
            /// LineList 는 새로운 List에  담는 동작 한번만 호출하고, 사용은 새로운 List 를 사용한다.
            List<CPoint> listAbsolutePoint = new List<CPoint>();
            listAbsolutePoint = AbsolutePointList;

            // 좌변을 제외하고 하변, 상변, 우변에만 경계조건이 부여된다.
            for (int i = 0; i < listAbsolutePoint.Count; i++)
            {
                // 마지막 PointLine 만 제외한다.
                if (i < listAbsolutePoint.Count - 1)
                {
                    sx = listAbsolutePoint[i].m_dX;
                    sy = listAbsolutePoint[i].m_dY;
                    ex = listAbsolutePoint[i + 1].m_dX;
                    ey = listAbsolutePoint[i + 1].m_dY;

                    /// 좌변 라인 (축대칭 지점으로 경계조건이 필요 없음)
                    if(i == 0)
                        femm.drawLine(sx, sy, ex, ey);
                    else
                        femm.drawBoundaryLine(sx, sy, ex, ey);
                }
                // 마지막 선은 끝점과 첫점을 있는다
                else
                {
                    sx = listAbsolutePoint[i].m_dX;
                    sy = listAbsolutePoint[i].m_dY;
                    ex = listAbsolutePoint[0].m_dX;
                    ey = listAbsolutePoint[0].m_dY;

                    femm.drawBoundaryLine(sx, sy, ex, ey);
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
        internal void setInsideBoundary(CScriptFEMM femm, double x1, double y1, double x2, double y2, double dMeshSize = 0)
        {
            setRectanglePoints(x1, y1, x2, y2);

            double maxX = (x1 > x2) ? x1 : x2;
            double maxY = (y1 > y2) ? y1 : y2;
            double width = Math.Abs(x2 - x1);
            double height = Math.Abs(y2 - y1);

            CPoint blockPoint = new CPoint();

            // 좌상단 구석에 block point 좌표를 얻어낸다.
            blockPoint.m_dX = maxX - width / 100.0f;
            blockPoint.m_dY = maxY - height / 100.0f;

            femm.setRegionBlockProp(blockPoint, dMeshSize);

            double sx, sy, ex, ey;

            /// 매번 생성하는 Property 이기 때문에 
            /// LineList 는 새로운 List에  담는 동작 한번만 호출하고, 사용은 새로운 List 를 사용한다.
            List<CPoint> listAbsolutePoint = new List<CPoint>();
            listAbsolutePoint = AbsolutePointList;

            // 좌변을 제외하고 하변, 상변, 우변에만 경계조건이 부여된다.
            for (int i = 0; i < listAbsolutePoint.Count; i++)
            {
                // 마지막 PointLine 만 제외한다.
                if (i < listAbsolutePoint.Count - 1)
                {
                    sx = listAbsolutePoint[i].m_dX;
                    sy = listAbsolutePoint[i].m_dY;
                    ex = listAbsolutePoint[i + 1].m_dX;
                    ey = listAbsolutePoint[i + 1].m_dY;

                    /// 경계조건은 부여하지 않음
                    femm.drawLine(sx, sy, ex, ey);

                }
                // 마지막 선은 끝점과 첫점을 있는다
                else
                {
                    sx = listAbsolutePoint[i].m_dX;
                    sy = listAbsolutePoint[i].m_dY;
                    ex = listAbsolutePoint[0].m_dX;
                    ey = listAbsolutePoint[0].m_dY;

                    femm.drawLine(sx, sy, ex, ey);
                }
            }
        }

        /// <summary>
        /// FEMM 의 Parts Face 색상을 변경하여 선택됨을 표시한다.
        /// 
        /// FEMM 에 표시함으로 절대좌표를 사용해야 한다
        /// </summary>
        internal void selectFace(CScriptFEMM femm)
        {
            if(femm == null)
            {
                CNotice.printTrace("Null femm 을 호출했습니다.");
                return;
            }

            /// 매번 생성하는 Property 이기 때문에 
            /// LineList 는 새로운 List에  담는 동작 한번만 호출하고, 사용은 새로운 List 를 사용한다.
            List<CPoint> listAbsolutePoint = new List<CPoint>();
            listAbsolutePoint = AbsolutePointList;

            for (int i = 0; i < listAbsolutePoint.Count; i++)
            {
                CPoint selectPoint = new CPoint();

                if(listAbsolutePoint[i].m_emLineKind == EMLineKind.STRAIGHT)
                {
                    if (i < listAbsolutePoint.Count - 1)
                    {
                        selectPoint.m_dX = (listAbsolutePoint[i].m_dX + listAbsolutePoint[i + 1].m_dX) / 2.0f;
                        selectPoint.m_dY = (listAbsolutePoint[i].m_dY + listAbsolutePoint[i + 1].m_dY) / 2.0f;
                    }
                    else
                    {
                        selectPoint.m_dX = (listAbsolutePoint[i].m_dX + listAbsolutePoint[0].m_dX) / 2.0f;
                        selectPoint.m_dY = (listAbsolutePoint[i].m_dY + listAbsolutePoint[0].m_dY) / 2.0f;
                    }

                    femm.selectLine(selectPoint);
                }
                else if(listAbsolutePoint[i].m_emLineKind == EMLineKind.ARC)
                {
                    selectPoint.m_dX = listAbsolutePoint[i].m_dX;
                    selectPoint.m_dY = listAbsolutePoint[i].m_dY;

                    femm.selectArc(selectPoint);
                }
            }            
        }

        /// <summary>
        /// FEMM 의 Parts 선택 표시를 제거한다.
        /// </summary>
        /// <param name="femm"></param>
        internal void clearSelected(CScriptFEMM femm)
        {
            femm.clearSelected();
        }

        internal void writeObject(System.IO.StreamWriter writeStream)
        {
            CWriteFile writeFile = new CWriteFile();

            writeFile.writeBeginLine(writeStream, "Shape", 3);

            writeFile.writeDataLine(writeStream, "BasePointX", m_basePoint.m_dX.ToString(), 4);
            writeFile.writeDataLine(writeStream, "BasePointY", m_basePoint.m_dY.ToString(), 4);

            writeFile.writeDataLine(writeStream, "FaceType", m_emFaceType, 4);

            foreach (CPoint pointLine in m_listRelativePoint)
            {
                writeFile.writeDataLine(writeStream, "PointX", pointLine.m_dX.ToString(), 4);
                writeFile.writeDataLine(writeStream, "PointY", pointLine.m_dY.ToString(), 4);
                writeFile.writeDataLine(writeStream, "LineKind", pointLine.m_emLineKind.ToString(), 4);
                writeFile.writeDataLine(writeStream, "ArcDriction", pointLine.m_emDirectionArc.ToString(), 4);
            }

            writeFile.writeEndLine(writeStream, "Shape", 3);
        }
    }
    
    /// <summary>
    /// 포인트 객체
    /// [유의 사항]
    ///  - 포인트 정보와 라인 정보를 모두 가지고 있다.
    ///    * 포인트 정보 : X,Y 좌표값
    ///    * 라인 정보 : 현 포인트에서 시작되는 라인의 종류와 Arc 그리기 방향
    ///  - 관련 라인은 현 포인터의 X,Y 좌표값으로 시작해서, CPointLine List 다음 포인트의 X,Y 좌표값으로 끝난다.
    /// </summary>
    public class CPoint
    {
        public double m_dX;
        public double m_dY;

        public EMLineKind m_emLineKind;
        public EMDirectionArc m_emDirectionArc;

        public CPoint()
        {
            m_dX = 0.0;
            m_dY = 0.0;

            m_emLineKind = EMLineKind.STRAIGHT;
            m_emDirectionArc = EMDirectionArc.FORWARD;
        }

        public CPoint(double x, double y, EMLineKind kind, EMDirectionArc direction)
        {
            m_dX = x;
            m_dY = y;

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
            m_startPoint.m_dX = 0;
            m_startPoint.m_dY = 0;
            m_endPoint.m_dX = 0;
            m_endPoint.m_dY = 0;

            m_startPoint.m_emLineKind = EMLineKind.STRAIGHT;
            m_startPoint.m_emDirectionArc = EMDirectionArc.FORWARD;
        }

        public CLine(double x1, double y1, double x2, double y2, EMLineKind kind, EMDirectionArc direction)
        {
            m_startPoint.m_dX = x1;
            m_startPoint.m_dY = y1;
            m_endPoint.m_dX = x2;
            m_endPoint.m_dY = y2;

            /// Line 정보는 Start 점에 담겨있는 정보를 사용한다.
            m_startPoint.m_emLineKind = kind;
            m_startPoint.m_emDirectionArc = direction;
        }

        public CLine(CPoint start, CPoint end)
        {
            m_startPoint.m_dX = start.m_dX;
            m_startPoint.m_dY = start.m_dY;
            m_endPoint.m_dX = end.m_dX;
            m_endPoint.m_dY = end.m_dY;

            /// Line 정보는 Start 점에 담겨있는 정보를 사용한다.
            m_startPoint.m_emLineKind = start.m_emLineKind;
            m_startPoint.m_emDirectionArc = start.m_emDirectionArc;
        }
    }
}
