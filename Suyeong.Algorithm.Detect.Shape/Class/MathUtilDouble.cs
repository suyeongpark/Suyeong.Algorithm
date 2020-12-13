using System;
using System.Collections.Generic;
using Suyeong.Lib.Mathematics;
using Suyeong.Lib.Type;

namespace Suyeong.Algorithm.Detect.Shape
{
    public static partial class MathUtils
    {
        public static bool IsSameLine(
            LineSampleDouble lineA,
            LineSampleDouble lineB,
            double margin = 0d
        ) 
        {
            return Math.Abs(lineA.StartX - lineB.StartX) <= margin &&
                Math.Abs(lineA.StartY - lineB.StartY) <= margin &&
                Math.Abs(lineA.EndX - lineB.EndX) <= margin &&
                Math.Abs(lineA.EndY - lineB.EndY) <= margin;
        }

        public static bool IsSameAngle(
            LineSampleDouble lineA,
            LineSampleDouble lineB,
            double margin = 0d
        ) 
        {
            return IsSameAngle(
                deltaX1: lineA.EndX - lineA.StartX,
                deltaY1: lineA.EndY - lineA.StartY,
                deltaX2: lineB.EndX - lineB.StartX,
                deltaY2: lineB.EndY - lineB.StartY,
                margin: margin
            );
        }

        public static bool IsSameAngle(
            double deltaX1,
            double deltaY1,
            double deltaX2,
            double deltaY2,
            double margin
        )
        {
            // 두 delta가 같은 방향
            if (MathUtil.IsNegative(val1: deltaX1, val2: deltaY1) == MathUtil.IsNegative(val1: deltaX2, val2: deltaY2))
            {
                //delta의 크기가 동일
                return MathUtil.IsEqual(val1: deltaX1, val2: deltaX2, epsilon: margin) &&
                    MathUtil.IsEqual(val1: deltaY1, val2: deltaY2, epsilon: margin);
            }

            return false;
        }

        public static bool IsCrossLine(
            LineSampleDouble lineA,
            LineSampleDouble lineB,
            double margin = 0d
        ) 
        {
            // 수평/수직 선이면 간단히 교차를 검증할 수 있다.
            if ((lineA.Orientation == LineOrientation.Horizontal || lineA.Orientation == LineOrientation.Vertical) &&
                (lineB.Orientation == LineOrientation.Horizontal || lineB.Orientation == LineOrientation.Vertical))
            {
                return lineA.MinX - lineB.MaxX <= margin &&
                    lineB.MinX - lineA.MaxX <= margin &&
                    lineA.MinY - lineB.MaxY <= margin &&
                    lineB.MinY - lineA.MaxY <= margin;
            }
            else
            {
                // 선이 하나라도 수평/수직이 아니라면 계산이 복잡하다.
                return LinearAlgebraUtil.IsCrossLine(
                    ax1: lineA.StartX,
                    ay1: lineA.StartY,
                    ax2: lineA.EndX,
                    ay2: lineA.EndY,
                    bx1: lineB.StartX,
                    by1: lineB.StartY,
                    bx2: lineB.EndX,
                    by2: lineB.EndY
                );
            }
        }

        public static bool IsContactEndToEnd(
            LineSampleDouble line,
            double minX,
            double minY,
            double maxX,
            double maxY,
            double margin
        ) 
        {
            return (Math.Abs(minX - line.StartX) <= margin && Math.Abs(minY - line.StartY) <= margin) ||
                (Math.Abs(minX - line.EndX) <= margin && Math.Abs(minY - line.EndY) <= margin) ||
                (Math.Abs(maxX - line.StartX) <= margin && Math.Abs(maxY - line.StartY) <= margin) ||
                (Math.Abs(maxX - line.EndX) <= margin && Math.Abs(maxY - line.EndY) <= margin);
        }

        public static bool IsContactLineEndToEnd(
            LineSampleDouble lineA,
            LineSampleDouble lineB,
            double margin = 0d
        ) 
        {
            return (Math.Abs(lineA.StartX - lineB.StartX) <= margin && Math.Abs(lineA.StartY - lineB.StartY) <= margin) ||
                (Math.Abs(lineA.StartX - lineB.EndX) <= margin && Math.Abs(lineA.StartY - lineB.EndY) <= margin) ||
                (Math.Abs(lineA.EndX - lineB.StartX) <= margin && Math.Abs(lineA.EndY - lineB.StartY) <= margin) ||
                (Math.Abs(lineA.EndX - lineB.EndX) <= margin && Math.Abs(lineA.EndY - lineB.EndY) <= margin);
        }

        public static bool IsEndPointOnHorizontalOrVerticalLine(
            LineSampleDouble line,
            LineSampleDouble horizontalOrVerticalLine,
            double margin = 0d
        ) 
        {
            return (horizontalOrVerticalLine.MinX - line.StartX <= margin && line.StartX - horizontalOrVerticalLine.MaxX <= margin && horizontalOrVerticalLine.MinY - line.StartY <= margin && line.StartY - horizontalOrVerticalLine.MaxY <= margin) ||
                (horizontalOrVerticalLine.MinX - line.EndX <= margin && line.EndX - horizontalOrVerticalLine.MaxX <= margin && horizontalOrVerticalLine.MinY - line.EndY <= margin && line.EndY - horizontalOrVerticalLine.MaxY <= margin);
        }

        public static bool IsPointInLineBoundary(
            LineSampleDouble line,
            double x,
            double y,
            double margin = 0d
        ) 
        {
            return line.MinX - x <= margin &&
                x - line.MaxX <= margin &&
                line.MinY - y <= margin &&
                y - line.MaxY <= margin;
        }

        public static bool IsPointOnLineEndPoint(
            LineSampleDouble line,
            double x,
            double y,
            double margin = 0d
        ) 
        {
            return (Math.Abs(x - line.StartX) <= margin && Math.Abs(y - line.StartY) <= margin) ||
                (Math.Abs(x - line.EndX) <= margin && Math.Abs(y - line.EndY) <= margin);
        }

        public static bool IsPointOnLine(
            LineSampleDouble line,
            double x,
            double y,
            double marginContact = 0d,
            double marginDistance = 0d
        ) 
        {
            if (line.MinX - x <= marginContact &&
                x - line.MaxX <= marginContact &&
                line.MinY - y <= marginContact &&
                y - line.MaxY <= marginContact)
            {
                if (line.Orientation == LineOrientation.Horizontal || line.Orientation == LineOrientation.Vertical)
                {
                    return true;
                }
                else
                {
                    // 끝점이 서로 닿지 않았으면 복잡한 처리를 해야 한다.
                    // 실제 이미지 상에서 오차가 있기 때문에 방정식을 통해 구하기 어렵다.
                    // 1. line의 boundary 안에 들어 오는지 확인한다.
                    // 2. 두 벡터의 각도가 수평인지 확인한다.
                    return IsPointOnLineWithProjection(line: line, x: x, y: y, margin: marginDistance);
                }
            }

            return false;
        }

        public static bool IsPointOnLineWithProjection(
            LineSampleDouble line,
            double x,
            double y,
            double margin
        ) 
        {
            double v1X = x - line.StartX;
            double v1Y = y - line.StartY;
            double v2X = line.EndX - line.StartX;
            double v2Y = line.EndY - line.StartY;

            // 사영 벡터를 구한다.
            Tuple<double, double> projection = LinearAlgebraUtil.Projection(ax: v1X, ay: v1Y, bx: v2X, by: v2Y);

            // 사영벡터와 비교하고자 하는 점의 벡터와의 거리가 margin보다 작으면 line 위에 있는 것으로 인정한다.
            double distanceX = v1X - projection.Item1;
            double distanceY = v1Y - projection.Item2;

            return distanceX * distanceX + distanceY * distanceY <= margin * margin;
        }

        public static bool TryGetLineDirectionToRectCrossPoint<T>(
            LineSampleDouble line,
            T rect,
            double maxX,
            out double x,
            out double y
        ) where T : IRect<double>
        {
            x = y = 0d;

            // 수직이나 수평이면 간단하게 처리 가능
            if (line.Orientation == LineOrientation.Vertical)
            {
                return line.CenterX >= rect.MinX && line.CenterY <= rect.MaxX;
            }
            else if (line.Orientation == LineOrientation.Horizontal)
            {
                return line.CenterY >= rect.MinY && line.CenterY <= rect.MaxY;
            }
            else
            {
                // 사선인 경우에는 직선의 방정식을 이용해서 처리한다.
                // 직선의 방정식을 이용해서 line을 끝까지 연장한다.
                double slope, intercept;
                LinearAlgebraUtil.TryGetLineSlopeAndIntercept(
                    ax: line.StartX,
                    ay: line.StartY,
                    bx: line.EndX,
                    by: line.EndY,
                    slope: out slope,
                    intercept: out intercept
                );

                double x1 = 0;
                double y1 = intercept;
                double x2 = maxX;
                double y2 = maxX * slope + intercept;

                List<Tuple<double, double, double, double>> rectLines = new List<Tuple<double, double, double, double>>();

                // 사각형의 topLine
                rectLines.Add(Tuple.Create(rect.MinX, rect.MinY, rect.MaxX, rect.MinY));

                // 사각형의 bottoLine
                rectLines.Add(Tuple.Create(rect.MinX, rect.MaxY, rect.MaxX, rect.MaxY));

                // 사각형의 leftLine
                rectLines.Add(Tuple.Create(rect.MinX, rect.MinY, rect.MinX, rect.MaxY));

                // 사각형의 rightLine
                rectLines.Add(Tuple.Create(rect.MaxX, rect.MinY, rect.MaxX, rect.MaxY));

                //앞서 구한 직선이 사각형의 4 line 중 하나와 교차하면 true 아니면 false
                foreach (Tuple<double, double, double, double> rectLine in rectLines)
                {
                    if (LinearAlgebraUtil.TryGetCrossPoint(
                        ax1: x1,
                        ay1: y1,
                        ax2: x2,
                        ay2: y2,
                        bx1: rectLine.Item1,
                        by1: rectLine.Item2,
                        bx2: rectLine.Item3,
                        by2: rectLine.Item4,
                        x: out x,
                        y: out y))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsPointInRect<T>(
            T rect,
            double x,
            double y,
            double margin = 0d
        ) where T : IRect<double>
        {
            return rect.MinX - x <= margin &&
                x - rect.MaxX <= margin &&
                rect.MinY - y <= margin &&
                y - rect.MaxY <= margin;
        }

        public static void GetMinMax(
            double x1, 
            double y1, 
            double x2, 
            double y2, 
            out double minX, 
            out double minY, 
            out double maxX, 
            out double maxY
        )
        {
            minX = minY = maxX = maxY = 0d;

            // line의 boundary를 찾는다.
            if (x1 < x2)
            {
                minX = x1;
                maxX = x2;
            }
            else
            {
                minX = x2;
                maxX = x1;
            }

            if (y1 < y2)
            {
                minY = y1;
                maxY = y2;
            }
            else
            {
                minY = y2;
                maxY = y1;
            }
        }

        // cross product에서 z가 0인 경우에 방향만 취하는 것
        public static double GetCCW(double ax, double ay, double bx, double by)
        {
            return ax * by - ay * bx;
        }

        /// 나눗셈 연산이 필요하므로 교점을 구하는 것 단순 교차 판정만 하는거라면 CCW를 이용한 Cross Line을 이용하자
        public static bool TryGetCrossPoint(
            LineSampleDouble lineA,
            LineSampleDouble lineB,
            out double x,
            out double y
        ) 
        {
            x = y = 0d;

            // 두 직선이 동일한 경우
            if (IsSameLine(lineA: lineA, lineB: lineB))
            {
                // 중점을 반환한다.
                x = lineA.CenterX;
                y = lineA.CenterY;

                return true;
            }

            // 우선 교차하는지 본다.
            if (IsCrossLine(lineA: lineA, lineB: lineB))
            {
                double v1X = lineA.EndX - lineA.StartX;
                double v1Y = lineA.EndY - lineA.StartY;
                double v2X = lineB.EndX - lineB.StartX;
                double v2Y = lineB.EndY - lineB.StartY;

                // 두 라인이 수평-수직인 경우 수평의 y, 수직의 x를 쓴다.
                if (lineA.Orientation == LineOrientation.Horizontal && lineB.Orientation == LineOrientation.Vertical)
                {
                    x = lineB.CenterX;
                    y = lineA.CenterY;
                }
                else if (lineA.Orientation == LineOrientation.Vertical && lineB.Orientation == LineOrientation.Horizontal)
                {
                    x = lineA.CenterX;
                    y = lineB.CenterY;
                }
                // 한쪽 선만 수평인 경우 수평의 y를 쓰고 x는 수평이 아닌 라인에 직선의 방정식을 쓴다.
                else if (lineA.Orientation == LineOrientation.Horizontal && lineB.Orientation != LineOrientation.Horizontal)
                {
                    double a2 = v2Y / v2X;
                    double b2 = lineB.StartY - (a2 * lineB.StartX);

                    y = lineA.StartY;
                    x = (y - b2) / a2;
                }
                else if (lineA.Orientation != LineOrientation.Horizontal && lineB.Orientation == LineOrientation.Horizontal)
                {
                    double a1 = v1Y / v1X;
                    double b1 = lineA.StartY - (a1 * lineA.StartX);

                    y = lineB.StartY;
                    x = (y - b1) / a1;
                }
                // 한쪽 선만 수직인 경우 수직의 x를 쓰고 y는 수직이 아닌 라인에 직선의 방정식을 쓴다.
                else if (lineA.Orientation == LineOrientation.Vertical && lineB.Orientation != LineOrientation.Vertical)
                {
                    double a2 = v2Y / v2X;
                    double b2 = lineB.StartY - (a2 * lineB.StartX);

                    x = lineA.StartX;
                    y = a2 * x + b2;
                }
                else if (lineA.Orientation != LineOrientation.Vertical && lineB.Orientation == LineOrientation.Vertical)
                {
                    double a1 = v1Y / v1X;
                    double b1 = lineA.StartY - (a1 * lineA.StartX);

                    x = lineB.StartX;
                    y = a1 * x + b1;
                }
                else
                {
                    // 두 라인이 모두 수평/ 수직이 아닌 경우 두 라인 모두 직선의 방정식으로 푼다.
                    double a1 = v1Y / v1X;
                    double b1 = lineA.StartY - (a1 * lineA.StartX);

                    double a2 = v2Y / v2X;
                    double b2 = lineB.StartY - (a2 * lineB.StartX);

                    x = -(b1 - b2) / (a1 - a2);
                    y = a1 * x + b1;
                }

                return true;
            }

            return false;
        }
    }
}
