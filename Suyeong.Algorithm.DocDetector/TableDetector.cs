using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;

namespace Suyeong.Algorithm.DocDetector
{
    public static class TableDetector
    {
        const int THRESHOLD_THRES = 128;
        const int THRESHOLD_MAX_VAL = 255;

        const int SIZE_LINE = 1;
        const int SIZE_DILATE = 5;

        const double RATIO_LINE_LENGTH = 0.01d;
        const double RATIO_LINE_SIZE = 3d;
        const double LIMIT_CELL_WIDTH = 0.5d;
        const double LIMIT_CELL_HEIGHT = 0.5d;
        const double RATIO_TEXT_HEIGHT = 0.5d;

        public static ImageTables GetTables(string imagePath, int textSize, bool drawTable = false, bool drawLine = false)
        {
            CellLines horizontals, verticals;
            int pageWidth, pageHeight;
            DetectLines(imagePath: imagePath, textSize: textSize, drawLine: drawLine, horizontals: out horizontals, verticals: out verticals, pageWidth: out pageWidth, pageHeight: out pageHeight);

            int lineDistance = GetLineDistanceByModeThickness(horizontals: ref horizontals, verticals: ref verticals);
            CellBoxes cellBoxes = DetectCellBoxes(lineDistance: lineDistance, horizontals: ref horizontals, verticals: ref verticals);

            ImageTables tables = DetectTables(cellBoxes: cellBoxes, pageWidth: pageWidth, pageHeight: pageHeight, lineDistance: lineDistance, textSize: textSize);

            if (drawLine)
            {
                DrawLines(horizontals: horizontals, verticals: verticals, imagePath: imagePath);
            }

            if (drawTable)
            {
                DrawTables(tables: tables, imagePath: imagePath);
            }

            return tables;
        }

        static void DetectLines(string imagePath, int textSize, bool drawLine, out CellLines horizontals, out CellLines verticals, out int pageWidth, out int pageHeight)
        {
            horizontals = new CellLines();
            verticals = new CellLines();
            pageWidth = 0;
            pageHeight = 0;

            using (Mat origin = Cv2.ImRead(fileName: imagePath, flags: ImreadModes.Color))
            using (Mat gray = new Mat(origin.Size(), type: MatType.CV_8UC1, s: new Scalar(0)))
            using (Mat binary = new Mat(origin.Size(), type: MatType.CV_8UC1, s: new Scalar(0)))
            {
                // 처리를 위해 일단 회색 처리
                Cv2.CvtColor(src: origin, dst: gray, code: ColorConversionCodes.BGR2GRAY);

                // contour를 찾기 좋게 이진화 하고, 잡음을 없애기 위해 블러를 적용한다.
                Cv2.Threshold(src: ~gray, dst: binary, thresh: THRESHOLD_THRES, maxval: THRESHOLD_MAX_VAL, type: ThresholdTypes.Binary);

                int hThreshold = Convert.ToInt32(origin.Width * RATIO_LINE_LENGTH);
                int vThreshold = Convert.ToInt32(origin.Height * RATIO_LINE_LENGTH);

                horizontals = DetectHorizontalLines(binary: binary, hThreshold: hThreshold, textSize: textSize, startIndex: 0);
                verticals = DetectVerticalLines(binary: binary, vThreshold: vThreshold, textSize: textSize, startIndex: horizontals.Count);
                pageWidth = origin.Width;
                pageHeight = origin.Height;
            }
        }

        static CellLines DetectHorizontalLines(Mat binary, int hThreshold, int textSize, int startIndex)
        {
            CellLines horizontals = new CellLines();

            using (Mat horizontal = binary.Clone())
            using (Mat hKernel = Cv2.GetStructuringElement(shape: MorphShapes.Rect, ksize: new Size(hThreshold, SIZE_LINE)))
            using (Mat kernelDilate = Cv2.GetStructuringElement(shape: MorphShapes.Rect, ksize: new Size(SIZE_DILATE, SIZE_DILATE)))
            {
                // horizontal
                Cv2.Erode(src: horizontal, dst: horizontal, element: hKernel, anchor: new Point(-1, -1));
                Cv2.Dilate(src: horizontal, dst: horizontal, element: hKernel, anchor: new Point(-1, -1));

                // 중간에 살짝 끊어진 라인을 잇기 위해 라인을 확장시킨다.
                Cv2.Dilate(src: horizontal, dst: horizontal, element: kernelDilate, anchor: new Point(-1, -1));

                Point[][] horizontalContours;
                HierarchyIndex[] horizontalHierarchy;
                Cv2.FindContours(image: horizontal, contours: out horizontalContours, hierarchy: out horizontalHierarchy, mode: RetrievalModes.External, method: ContourApproximationModes.ApproxSimple, offset: new Point(0, 0));

                int startX, startY, endX, endY, index = 0;
                Rect rect;

                for (int i = 0; i < horizontalHierarchy.Length; i++)
                {
                    rect = Cv2.BoundingRect(curve: horizontalContours[i]);

                    startX = rect.X;
                    startY = rect.Y + (int)(rect.Height * 0.5);
                    endX = rect.X + rect.Width;
                    endY = startY;

                    if (rect.Width > textSize)
                    {
                        horizontals.Add(new CellLine(index: index++, startX: startX, startY: startY, endX: endX, endY: endY, thickness: rect.Height));
                    }
                }
            }

            return new CellLines(horizontals.OrderBy(line => line.CenterY));
        }

        static CellLines DetectVerticalLines(Mat binary, int vThreshold, int textSize, int startIndex)
        {
            CellLines verticals = new CellLines();

            using (Mat vertical = binary.Clone())
            using (Mat vKernel = Cv2.GetStructuringElement(shape: MorphShapes.Rect, ksize: new Size(SIZE_LINE, vThreshold)))
            using (Mat kernelDilate = Cv2.GetStructuringElement(shape: MorphShapes.Rect, ksize: new Size(SIZE_DILATE, SIZE_DILATE)))
            {
                // vertical
                Cv2.Erode(src: vertical, dst: vertical, element: vKernel, anchor: new Point(-1, -1));
                Cv2.Dilate(src: vertical, dst: vertical, element: vKernel, anchor: new Point(-1, -1));

                // 중간에 살짝 끊어진 라인을 잇기 위해 라인을 확장시킨다.
                Cv2.Dilate(src: vertical, dst: vertical, element: kernelDilate, anchor: new Point(-1, -1));

                Point[][] verticalContours;
                HierarchyIndex[] verticalHierarchy;
                Cv2.FindContours(image: vertical, contours: out verticalContours, hierarchy: out verticalHierarchy, mode: RetrievalModes.External, method: ContourApproximationModes.ApproxSimple, offset: new OpenCvSharp.Point(0, 0));

                int startX, startY, endX, endY, index = startIndex;
                Rect rect;

                for (int i = 0; i < verticalHierarchy.Length; i++)
                {
                    rect = Cv2.BoundingRect(curve: verticalContours[i]);

                    startX = rect.X + (int)(rect.Width * 0.5);
                    startY = rect.Y;
                    endX = startX;
                    endY = rect.Y + rect.Height;

                    if (rect.Height > textSize)
                    {
                        verticals.Add(new CellLine(index: index++, startX: startX, startY: startY, endX: endX, endY: endY, thickness: rect.Width));
                    }
                }
            }

            return new CellLines(verticals.OrderBy(line => line.CenterX));
        }


        // 최빈값 기준으로 찾기
        static int GetLineDistanceByModeThickness(ref CellLines horizontals, ref CellLines verticals)
        {
            Dictionary<int, int> dic = new Dictionary<int, int>();

            int count;

            foreach (CellLine rawLine in horizontals)
            {
                if (dic.TryGetValue(rawLine.Thickness, out count))
                {
                    dic[rawLine.Thickness]++;
                }
                else
                {
                    dic.Add(rawLine.Thickness, 1);
                }
            }

            foreach (CellLine rawLine in verticals)
            {
                if (dic.TryGetValue(rawLine.Thickness, out count))
                {
                    dic[rawLine.Thickness]++;
                }
                else
                {
                    dic.Add(rawLine.Thickness, 1);
                }
            }

            if (dic.Count > 0)
            {
                int countMax = dic.Max(kvp => kvp.Value);
                return (int)(dic.FirstOrDefault(kvp => kvp.Value == countMax).Key * RATIO_LINE_SIZE);
            }
            else
            {
                return 0;
            }
        }

        static CellBoxes DetectCellBoxes(int lineDistance, ref CellLines horizontals, ref CellLines verticals)
        {
            CellBoxes cellBoxes = new CellBoxes();

            CellLines horizontalsFinal, verticalsFinal;
            CellLine left = new CellLine();
            CellLine right = new CellLine();
            CellLine top, bottom;
            bool findLeft, findRight;
            int index = 0;

            Dictionary<int, int> duplicateDic = new Dictionary<int, int>();

            // 위에서부터 아래로 찾는다.
            foreach (CellLine horizontal in horizontals)
            {
                if (!duplicateDic.ContainsKey(horizontal.Index))
                {
                    findLeft = findRight = false;

                    foreach (CellLine vertical in verticals)
                    {
                        if (!duplicateDic.ContainsKey(vertical.Index))
                        {
                            if (horizontal.MinX <= vertical.MaxX && horizontal.MaxX >= vertical.MinX && horizontal.MinY <= vertical.MaxY && horizontal.MaxY >= vertical.MinY)
                            {
                                // 현재 라인의 왼쪽과 같은 위치에서 시작되는 수직 라인을 찾는다.
                                if (Math.Abs(horizontal.StartX - vertical.StartX) <= lineDistance)
                                {
                                    findLeft = true;
                                    left = vertical;
                                }
                                // 현재 라인의 오른쪽과 같은 위치에서 시작되는 수직 라인을 찾는다.
                                else if (Math.Abs(horizontal.EndX - vertical.StartX) <= lineDistance)
                                {
                                    findRight = true;
                                    right = vertical;
                                }
                            }

                            // 두 라인을 찾으면
                            if (findLeft && findRight)
                            {
                                // 두 라인을 이용해서 두 수직 라인의 하단과 이어지는 수평 라인을 찾는다.
                                if (ExistBottomLine(topIndex: horizontal.Index, lineDistance: lineDistance, verticalLeft: left, verticalRight: right, horizontals: horizontals, duplicateDic: ref duplicateDic, bottom: out bottom))
                                {
                                    top = horizontal;

                                    duplicateDic.Add(top.Index, top.Index);
                                    duplicateDic.Add(bottom.Index, bottom.Index);
                                    duplicateDic.Add(left.Index, left.Index);
                                    duplicateDic.Add(right.Index, right.Index);

                                    horizontals = GetHorizonsAllInBox(lineDistance: lineDistance, top: top, bottom: bottom, left: left, right: right, horizontals: horizontals, duplicateDic: ref duplicateDic);
                                    verticals = GetVerticalAllInBox(lineDistance: lineDistance, top: top, bottom: bottom, left: left, right: right, verticals: verticals, duplicateDic: ref duplicateDic);

                                    RemoveNoiseLine(lineDistance: lineDistance, top: top, bottom: bottom, left: left, right: right, innerHorizontals: horizontals, innerVerticals: verticals, duplicateDic: ref duplicateDic, horizontalsFinal: out horizontalsFinal, verticalsFinal: out verticalsFinal);

                                    cellBoxes.Add(new CellBox(index: index++, horizontals: new CellLines(horizontalsFinal.OrderBy(line => line.StartY)), verticals: new CellLines(verticalsFinal.OrderBy(line => line.StartX))));
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return cellBoxes;
        }

        static bool ExistBottomLine(int topIndex, CellLine verticalLeft, CellLine verticalRight, CellLines horizontals, int lineDistance, ref Dictionary<int, int> duplicateDic, out CellLine bottom)
        {
            bottom = new CellLine();

            foreach (CellLine horizontal in horizontals)
            {
                if (!duplicateDic.ContainsKey(horizontal.Index) && horizontal.Index != topIndex)
                {
                    // 두 수직선과 끝점이 만나야 함
                    if (Math.Abs(horizontal.StartX - verticalLeft.StartX) <= lineDistance &&
                        Math.Abs(horizontal.EndX - verticalRight.EndX) <= lineDistance &&
                        Math.Abs(horizontal.StartY - verticalLeft.EndY) <= lineDistance &&
                        Math.Abs(horizontal.EndY - verticalRight.EndY) <= lineDistance)
                    {
                        bottom = horizontal;
                        return true;
                    }
                }
            }

            return false;
        }

        static CellLines GetHorizonsAllInBox(CellLine top, CellLine bottom, CellLine left, CellLine right, CellLines horizontals, int lineDistance, ref Dictionary<int, int> duplicateDic)
        {
            CellLines horizontalsAll = new CellLines();

            foreach (CellLine line in horizontals)
            {
                if (!duplicateDic.ContainsKey(line.Index) &&
                    line.StartY >= top.StartY &&
                    line.EndY <= bottom.EndY &&
                    line.StartX >= left.StartX - lineDistance &&
                    line.EndX <= right.EndX + lineDistance)
                {
                    horizontalsAll.Add(line);
                }
            }

            return horizontalsAll;
        }

        static CellLines GetVerticalAllInBox(CellLine top, CellLine bottom, CellLine left, CellLine right, CellLines verticals, int lineDistance, ref Dictionary<int, int> duplicateDic)
        {
            CellLines verticalsAll = new CellLines();

            foreach (CellLine line in verticals)
            {
                if (!duplicateDic.ContainsKey(line.Index) &&
                    line.StartX >= left.StartX &&
                    line.EndX <= right.EndX &&
                    line.StartY >= top.StartY - lineDistance &&
                    line.EndY <= bottom.StartY + lineDistance)
                {
                    verticalsAll.Add(line);
                }
            }

            return verticalsAll;
        }

        static void RemoveNoiseLine(CellLine top, CellLine bottom, CellLine left, CellLine right, CellLines innerHorizontals, CellLines innerVerticals, int lineDistance, ref Dictionary<int, int> duplicateDic, out CellLines horizontalsFinal, out CellLines verticalsFinal)
        {
            // 최종적으로 찾은 것을 duplicateDic에 추가한다. (이렇게 하면 박스 안의 박스는 별도로 처리 가능)
            horizontalsFinal = new CellLines();
            verticalsFinal = new CellLines();

            CellLines horizontals = new CellLines();
            CellLines verticals = new CellLines();
            CellLines horizontalFirst = new CellLines();
            CellLines verticalFirst = new CellLines();

            // 일단 가장 외각선과 맞닿는 부분이 있는 라인만 인정한다.
            foreach (CellLine line in innerHorizontals)
            {
                if (Math.Abs(line.StartX - left.StartX) <= lineDistance || Math.Abs(line.EndX - right.EndX) <= lineDistance)
                {
                    horizontals.Add(line);
                }
                else
                {
                    horizontalFirst.Add(line);
                }
            }

            // 일단 가장 외각선과 맞닿는 부분이 있는 라인만 인정한다.
            foreach (CellLine line in innerVerticals)
            {
                if (Math.Abs(line.StartY - top.StartY) <= lineDistance || Math.Abs(line.EndY - bottom.EndY) <= lineDistance)
                {
                    verticals.Add(line);
                }
                else
                {
                    verticalFirst.Add(line);
                }
            }

            // 가장 외각선과 맞닿는 부분이 있는 라인과 맞닿은 라인은 인정한다.
            // Data Sheet 같이 복잡한 테이블을 처리하기 위해 4번 돌린다.
            CellLines horizontalSecond, verticalSecond, horizontalThird, verticalThird, horizontalFourth, verticalFourth;

            GetInsideLine(horizontalSamples: horizontalFirst, verticalSamples: verticalFirst, lineDistance: lineDistance, horizontals: ref horizontals, verticals: ref verticals, horizontalRests: out horizontalSecond, verticalRests: out verticalSecond);
            GetInsideLine(horizontalSamples: horizontalSecond, verticalSamples: verticalSecond, lineDistance: lineDistance, horizontals: ref horizontals, verticals: ref verticals, horizontalRests: out horizontalThird, verticalRests: out verticalThird);
            GetInsideLine(horizontalSamples: horizontalThird, verticalSamples: verticalThird, lineDistance: lineDistance, horizontals: ref horizontals, verticals: ref verticals, horizontalRests: out horizontalFourth, verticalRests: out verticalFourth);

            bool matchStart, matchEnd;

            // top, bottom을 수평선에 추가한다.
            horizontals.Add(top);
            horizontals.Add(bottom);

            // left, right를 수직선에 추가한다.
            verticals.Add(left);
            verticals.Add(right);

            // 양 끝점이 모두 vertical과 붙어 있어야 line으로 인정한다. Data Sheet의 경우 단순 밑줄이 마치 Line처럼 잡힘
            foreach (CellLine horizontal in horizontals)
            {
                matchStart = false;
                matchEnd = false;

                foreach (CellLine vertical in verticals)
                {
                    if (horizontal.StartY >= vertical.StartY && horizontal.EndY <= vertical.EndY)
                    {
                        if (Math.Abs(horizontal.StartX - vertical.StartX) <= lineDistance)
                        {
                            matchStart = true;
                        }
                        else if (Math.Abs(horizontal.EndX - vertical.EndX) <= lineDistance)
                        {
                            matchEnd = true;
                        }
                    }

                    if (matchStart && matchEnd)
                    {
                        horizontalsFinal.Add(horizontal);

                        // top, bottom이 추가 되었기 때문에 체크
                        if (!duplicateDic.ContainsKey(horizontal.Index))
                        {
                            duplicateDic.Add(horizontal.Index, horizontal.Index);
                        }

                        break;
                    }
                }
            }

            // 양 끝점이 모두 horizontal과 붙어 있어야 line으로 인정한다. Data Sheet의 경우 단순 밑줄이 마치 Line처럼 잡힘
            foreach (CellLine vertical in verticals)
            {
                matchStart = false;
                matchEnd = false;

                foreach (CellLine horizontal in horizontals)
                {
                    if (vertical.StartX >= horizontal.StartX && vertical.EndX <= horizontal.EndX)
                    {
                        if (Math.Abs(vertical.StartY - horizontal.StartY) <= lineDistance)
                        {
                            matchStart = true;
                        }
                        else if (Math.Abs(vertical.EndY - horizontal.EndY) <= lineDistance)
                        {
                            matchEnd = true;
                        }
                    }

                    if (matchStart && matchEnd)
                    {
                        verticalsFinal.Add(vertical);

                        // left, right가 추가 되었기 때문에 체크
                        if (!duplicateDic.ContainsKey(vertical.Index))
                        {
                            duplicateDic.Add(vertical.Index, vertical.Index);
                        }

                        break;
                    }
                }
            }
        }

        static void GetInsideLine(CellLines horizontalSamples, CellLines verticalSamples, int lineDistance, ref CellLines horizontals, ref CellLines verticals, out CellLines horizontalRests, out CellLines verticalRests)
        {
            horizontalRests = new CellLines();
            verticalRests = new CellLines();

            bool isContain;

            // 가장 외각선과 맞닿는 부분이 있는 라인과 맞닿은 라인은 인정한다.
            foreach (CellLine horizontal in horizontalSamples)
            {
                isContain = false;

                foreach (CellLine vertical in verticals)
                {
                    if (horizontal.StartY >= vertical.StartY && horizontal.EndY <= vertical.EndY)
                    {
                        if (Math.Abs(horizontal.StartX - vertical.StartX) <= lineDistance || Math.Abs(horizontal.EndX - vertical.EndX) <= lineDistance)
                        {
                            isContain = true;
                            horizontals.Add(horizontal);
                            break;
                        }
                    }
                }

                if (!isContain)
                {
                    horizontalRests.Add(horizontal);
                }
            }

            // 가장 외각선과 맞닿는 부분이 있는 라인과 맞닿은 라인은 인정한다.
            foreach (CellLine vertical in verticalSamples)
            {
                isContain = false;

                foreach (CellLine horizontal in horizontals)
                {
                    if (vertical.StartX >= horizontal.StartX && vertical.EndX <= horizontal.EndX)
                    {
                        if (Math.Abs(vertical.StartY - horizontal.StartY) <= lineDistance || Math.Abs(vertical.EndY - horizontal.EndY) <= lineDistance)
                        {
                            isContain = true;
                            verticals.Add(vertical);
                            break;
                        }
                    }
                }

                if (!isContain)
                {
                    verticalRests.Add(vertical);
                }
            }
        }

        static ImageTables DetectTables(CellBoxes cellBoxes, int pageWidth, int pageHeight, int lineDistance, int textSize)
        {
            ImageTables tables = new ImageTables();

            CrossPoints crossPoints;
            ImageCells cells;
            int[,] crossPointIndexes;
            int index = 0;

            int spacingMin = (int)(textSize * RATIO_TEXT_HEIGHT);
            double widthRatio = 1d / pageWidth;
            double heightRatio = 1d / pageHeight;

            foreach (CellBox box in cellBoxes)
            {
                // box 안의 line이 5개가 넘어야 table로 인정한다. top, bottom, left, right가 있으므로 나머지 line이 적어도 1개 이상 있어야 함.
                if (box.Horizontals.Count + box.Verticals.Count > 0)
                {
                    // 교차점을 구한다.
                    crossPoints = DetectRawCrossPoints(horizontals: box.Horizontals, verticals: box.Verticals, lineDistance: lineDistance, crossPointIndexes: out crossPointIndexes);

                    // 일단 cell을 구한다.
                    cells = DetectRawCells(crossPoints: crossPoints, crossPointIndexes: crossPointIndexes, spacingMin: spacingMin, widthRatio: widthRatio, heightRatio: heightRatio);

                    if (cells.Count > 0)
                    {
                        tables.Add(new ImageTable(index: index++, cells: cells));
                    }
                }
            }

            return tables;
        }

        static CrossPoints DetectRawCrossPoints(CellLines horizontals, CellLines verticals, int lineDistance, out int[,] crossPointIndexes)
        {
            crossPointIndexes = new int[horizontals.Count, verticals.Count];

            CrossPoints rawCrossPoints = new CrossPoints();

            bool existTopLine, existLeftLine;
            int index = 0;
            CellLine horizontal, vertical;

            for (int i = 0; i < horizontals.Count; i++)
            {
                horizontal = horizontals[i];

                for (int j = 0; j < verticals.Count; j++)
                {
                    vertical = verticals[j];

                    // 두선이 교차하는지 확인
                    if (horizontal.StartX <= vertical.StartX && horizontal.EndX >= vertical.EndX && horizontal.StartY >= vertical.StartY && horizontal.EndY <= vertical.EndY)
                    {
                        // 현재 점이 위로 선이 있는지, 좌로 선이 있는지 확인. 향후 cell 구성할 때 사용
                        existTopLine = Math.Abs(vertical.StartY - horizontal.StartY) > lineDistance;
                        existLeftLine = Math.Abs(horizontal.StartX - vertical.StartX) > lineDistance;

                        rawCrossPoints.Add(new CrossPoint(index: index, existTopLine: existTopLine, existLeftLine: existLeftLine, rowIndex: i, columnIndex: j, x: vertical.StartX, y: horizontal.StartY, horizontal: horizontal, vertical: vertical));
                        crossPointIndexes[i, j] = index;
                        index++;
                    }
                    else
                    {
                        crossPointIndexes[i, j] = -1;
                    }
                }
            }

            return rawCrossPoints;
        }

        static ImageCells DetectRawCells(CrossPoints crossPoints, int[,] crossPointIndexes, int spacingMin, double widthRatio, double heightRatio)
        {
            ImageCells rawCells = new ImageCells();

            if (crossPoints.Count > 0)
            {
                int rowIndex, columnIndex, leftColumnIndex, topRowIndex, leftTopIndex;
                int leftX, rightX, topY, bottomY, width, height, rowSize, columnSize, index = 0;
                CrossPoint leftTop;

                foreach (CrossPoint current in crossPoints)
                {
                    if (current.RowIndex > 0 && current.ColumnIndex > 0)
                    {
                        if (current.ExistLeftLine && current.ExistTopLine)
                        {
                            leftColumnIndex = FindLeftColumnIndex(rowIndex: current.RowIndex, columnIndex: current.ColumnIndex, crossPointIndexes: ref crossPointIndexes, rawCrossPoints: ref crossPoints);
                            topRowIndex = FindTopRowIndex(rowIndex: current.RowIndex, columnIndex: current.ColumnIndex, crossPointIndexes: ref crossPointIndexes, rawCrossPoints: ref crossPoints);

                            if (leftColumnIndex > -1 && topRowIndex > -1)
                            {
                                leftTopIndex = crossPointIndexes[topRowIndex, leftColumnIndex];

                                if (leftTopIndex > -1)
                                {
                                    leftTop = crossPoints[leftTopIndex];

                                    leftX = leftTop.X;
                                    rightX = current.X;
                                    topY = leftTop.Y;
                                    bottomY = current.Y;
                                    width = rightX - leftX;
                                    height = bottomY - topY;

                                    // 가로 간격이나 세로 간격이 최소 간격 작은 것은 cell로 인정하지 않는다.
                                    // 셀의 가로, 세로가 모두 페이지의 절반을 넘어서면 cell로 인정하지 않는다.
                                    if (width > spacingMin && height > spacingMin &&
                                        !(width * widthRatio > LIMIT_CELL_WIDTH && height * heightRatio > LIMIT_CELL_HEIGHT))
                                    {
                                        rowIndex = leftTop.RowIndex;
                                        columnIndex = leftTop.ColumnIndex;
                                        columnSize = current.ColumnIndex - leftTop.ColumnIndex;
                                        rowSize = current.RowIndex - leftTop.RowIndex;

                                        // 최종적으로 좌표는 백분율로 된 상대값이기 때문에 여기서 바꿔줘야 한다.
                                        rawCells.Add(new ImageCell(index: index++, rowIndex: rowIndex, columnIndex: columnIndex, rowSize: rowSize, columnSize: columnSize, topY: topY, bottomY: bottomY, leftX: leftX, rightX: rightX));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // 병합된 경우 index가 뒤죽박죽이 되므로 여기서 정렬한다.
            return new ImageCells(rawCells.OrderBy(cell => cell.RowIndex).ThenBy(cell => cell.ColumnIndex));
        }

        static int FindLeftColumnIndex(int rowIndex, int columnIndex, ref int[,] crossPointIndexes, ref CrossPoints rawCrossPoints)
        {
            CrossPoint leftPoint;
            int index;

            for (int i = columnIndex - 1; i > -1; i--)
            {
                index = crossPointIndexes[rowIndex, i];

                if (index > -1)
                {
                    leftPoint = rawCrossPoints[index];

                    if (leftPoint.ExistTopLine)
                    {
                        return leftPoint.ColumnIndex;
                    }
                }
            }

            return -1;
        }

        static int FindTopRowIndex(int rowIndex, int columnIndex, ref int[,] crossPointIndexes, ref CrossPoints rawCrossPoints)
        {
            CrossPoint topPoint;
            int index = 0;

            for (int i = rowIndex - 1; i > -1; i--)
            {
                index = crossPointIndexes[i, columnIndex];

                if (index > -1)
                {
                    topPoint = rawCrossPoints[index];

                    if (topPoint.ExistLeftLine)
                    {
                        return topPoint.RowIndex;
                    }
                }
            }

            return -1;
        }

        static void DrawTables(ImageTables tables, string imagePath)
        {
            using (Mat img = Cv2.ImRead(fileName: imagePath, flags: ImreadModes.Color))
            {
                foreach (ImageTable table in tables)
                {
                    foreach (ImageCell cell in table.cells)
                    {
                        Cv2.Rectangle(img: img, rect: new Rect(x: cell.LeftX, y: cell.TopY, width: cell.RightX - cell.LeftX, height: cell.BottomY - cell.TopY), color: new Scalar(0, 0, 255), thickness: 2);
                    }

                    Cv2.Rectangle(img: img, rect: new Rect(x: table.X, y: table.Y, width: table.Width, height: table.Height), color: new Scalar(0, 0, 255), thickness: 2);
                }

                Cv2.ImWrite($"{imagePath}_Tables.png", img);
            }
        }

        static void DrawLines(CellLines horizontals, CellLines verticals, string imagePath)
        {
            using (Mat img = Cv2.ImRead(fileName: imagePath, flags: ImreadModes.Color))
            {
                foreach (CellLine cellLine in horizontals)
                {
                    Cv2.Line(img: img, pt1X: cellLine.StartX, pt1Y: cellLine.StartY, pt2X: cellLine.EndX, pt2Y: cellLine.EndY, color: new Scalar(0, 0, 255), thickness: 2);
                }

                foreach (CellLine cellLine in verticals)
                {
                    Cv2.Line(img: img, pt1X: cellLine.StartX, pt1Y: cellLine.StartY, pt2X: cellLine.EndX, pt2Y: cellLine.EndY, color: new Scalar(0, 0, 255), thickness: 2);
                }

                Cv2.ImWrite($"{imagePath}_Detect_Lines.png", img);
            }
        }

    }
}
