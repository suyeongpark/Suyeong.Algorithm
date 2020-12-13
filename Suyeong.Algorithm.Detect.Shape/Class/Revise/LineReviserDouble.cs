using System;
using System.Collections.Generic;
using Suyeong.Lib.Mathematics;
using Suyeong.Lib.Type;

namespace Suyeong.Algorithm.Detect.Shape
{
    public static partial class LineReviser
    {
        public static PdfLineCollection MergePdfLines(
            IEnumerable<PdfLine> pdfLines,
            double marginVerticalDelta,
            double marginMergeLineRange,
            double marginContactLine
        )
        {
            PdfLineCollection mergeLines = new PdfLineCollection();

            // delta가 작은 것도 처리
            List<PdfLine> horizontals, verticals, positives, negatives;
            CoreUtils.SplitPdfLines(
                lines: pdfLines,
                horizontals: out horizontals,
                verticals: out verticals,
                positives: out positives,
                negatives: out negatives
            );

            Dictionary<int, PdfLine> lineDic = pdfLines.ToDictionary(line => line.Index, line => line);
            Dictionary<int, int> mergedLineIdDic = new Dictionary<int, int>();

            // 수평선끼리 합치기
            mergeLines.AddRange(MergeLines(
                pdfLines: horizontals,
                marginMergeLineRange: marginMergeLineRange,
                marginContactLine: marginContactLine,
                marginVerticalDelta: marginVerticalDelta,
                lineDic: lineDic,
                mergedLineIdDic: ref mergedLineIdDic
            ));

            // 수직선끼리 합치기
            mergeLines.AddRange(MergeLines(
                pdfLines: verticals,
                marginMergeLineRange: marginMergeLineRange,
                marginContactLine: marginContactLine,
                marginVerticalDelta: marginVerticalDelta,
                lineDic: lineDic,
                mergedLineIdDic: ref mergedLineIdDic
            ));

            // 합쳐지지 않은 라인을 담는다.
            mergeLines.AddRange(pdfLines.Where(line => !mergedLineIdDic.ContainsKey(line.Index)));

            return mergeLines;
        }

        static PdfLineCollection MergeLines(
            IEnumerable<PdfLine> pdfLines,
            double marginMergeLineRange,
            double marginContactLine,
            double marginVerticalDelta,
            Dictionary<int, PdfLine> lineDic,
            ref Dictionary<int, int> mergedLineIdDic
        )
        {
            PdfLineCollection mergeLines = new PdfLineCollection();

            PdfLineCollection mergeSamples, lines;
            PdfLine mergeLineSample, main, sub;
            List<List<int>> idSetGroup, mergeSets;
            List<int> idSet;

            // 주석/컬러를 기준으로 처리한다.
            foreach (IGrouping<bool, PdfLine> annotationGroup in pdfLines.GroupBy(line => line.IsAnnotation))
            {
                foreach (IGrouping<int, PdfLine> fillGroup in annotationGroup.GroupBy(line => line.FillArgb))
                {
                    foreach (IGrouping<int, PdfLine> borderGroup in fillGroup.GroupBy(line => line.StrokeArgb))
                    {
                        idSetGroup = new List<List<int>>();
                        idSet = new List<int>();

                        lines = new PdfLineCollection(borderGroup);

                        for (int i = 0; i < lines.Count; i++)
                        {
                            main = lines[i];

                            for (int j = i + 1; j < lines.Count; j++)
                            {
                                sub = lines[j];

                                // 수평/ 수직이면 간단하게 계산한다.
                                // 같은 수평선인데 y 값에 약간 오차가 있는 경우는 보정
                                if (main.Orientation == Orientation.Horizontal &&
                                    Math.Abs(main.CenterY - sub.CenterY) <= marginMergeLineRange &&
                                    main.MinX - sub.MaxX <= marginContactLine &&
                                    sub.MinX - main.MaxX <= marginContactLine)
                                {
                                    // 겹치는 라인을 발견하면 일단 그 쌍을 담는다.
                                    idSet = new List<int>();
                                    idSet.Add(main.Index);
                                    idSet.Add(sub.Index);
                                    idSetGroup.Add(idSet);

                                    idSet = new List<int>();
                                    idSet.Add(sub.Index);
                                    idSet.Add(main.Index);
                                    idSetGroup.Add(idSet);
                                }
                                // 수평/ 수직이면 간단하게 계산한다.
                                // 같은 수직선인데 x 값에 약간 오차가 있는 경우는 보정
                                else if (main.Orientation == Orientation.Vertical &&
                                    Math.Abs(main.CenterX - sub.CenterX) <= marginMergeLineRange &&
                                    main.MinY - sub.MaxY <= marginContactLine &&
                                    sub.MinY - main.MaxY <= marginContactLine)
                                {
                                    // 겹치는 라인을 발견하면 일단 그 쌍을 담는다.
                                    idSet = new List<int>();
                                    idSet.Add(main.Index);
                                    idSet.Add(sub.Index);
                                    idSetGroup.Add(idSet);

                                    idSet = new List<int>();
                                    idSet.Add(sub.Index);
                                    idSet.Add(main.Index);
                                    idSetGroup.Add(idSet);
                                }
                            }
                        }

                        if (idSetGroup.Count > 0)
                        {
                            mergeSets = CoreUtils.MergeIdSets(idSets: idSetGroup);

                            foreach (List<int> mergeSet in mergeSets)
                            {
                                mergeSamples = new PdfLineCollection();

                                foreach (int id in mergeSet)
                                {
                                    if (lineDic.TryGetValue(id, out mergeLineSample))
                                    {
                                        mergeSamples.Add(mergeLineSample);
                                        mergedLineIdDic.Add(mergeLineSample.Index, mergeLineSample.Index);
                                    }
                                }

                                // 하나의 line으로 합친다.
                                mergeLines.Add(MergeLine(mergeLines: mergeSamples, marginVerticalDelta: marginVerticalDelta));
                            }
                        }

                    }
                }
            }

            return mergeLines;
        }

        static PdfLine MergeLine(
            IEnumerable<PdfLine> mergeLines,
            double marginVerticalDelta
        )
        {
            PdfLine first = mergeLines.FirstOrDefault();

            double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;

            foreach (PdfLine line in mergeLines)
            {
                if (line.MinX < minX)
                {
                    minX = line.MinX;
                }

                if (line.MinY < minY)
                {
                    minY = line.MinY;
                }

                if (line.MaxX > maxX)
                {
                    maxX = line.MaxX;
                }

                if (line.MaxY > maxY)
                {
                    maxY = line.MaxY;
                }
            }

            double width = maxX - minX;
            double height = maxY - minY;

            // line의 두께 때문에 위치가 어긋나게 들어오는 경우가 있다. 이런 경우 가운데 라인으로 처리한다.
            // 수평선과 수직선만 합치기 때문에 무조건 둘 중 하나는 두께가 없어야 함.
            if (width < height)
            {
                double centerX = (minX + maxX) * 0.5d;
                minX = centerX;
                maxX = centerX;
            }
            else
            {
                double centerY = (minY + maxY) * 0.5d;
                minY = centerY;
                maxY = centerY;
            }

            Orientation orientation = CoreUtils.GetLineOrientation(
                startX: minX,
                startY: minY,
                endX: maxX,
                endY: maxY,
                margin: marginVerticalDelta
            );

            return new PdfLine(
                index: first.Index,
                groupIndex: first.GroupIndex,
                dashType: first.DashType,
                pointType: first.PointType,
                orientation: orientation,
                startX: minX,
                startY: minY,
                endX: maxX,
                endY: maxY,
                thickness: first.Thickness,
                fillArgb: first.FillArgb,
                strokeArgb: first.StrokeArgb,
                isAnnotation: first.IsAnnotation,
                isMasking: first.IsMasking,
                isMasked: first.IsMasked,
                hasPattern: first.HasPattern
            );
        }
    }
}
