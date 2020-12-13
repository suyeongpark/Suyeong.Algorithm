using System;
using System.Collections.Generic;
using Suyeong.Lib.Mathematics;
using Suyeong.Lib.Type;

namespace Suyeong.Algorithm.Detect.Shape
{

    // line to line, line to symbol, symbol to symbol

    public static partial class LinkGenerator
    {
        public static PdfLineCollection GetPdfNonEndToEndLines(
            IEnumerable<PdfLine> sampleLines,
            IEnumerable<PdfLine> targetLines,
            double marginContactLine,
            double marginPointOnLineProjection
        )
        {
            PdfLineCollection nonEndToEndLines = new PdfLineCollection();

            object sync = new object();

            ParallelLoopResult result = Parallel.ForEach(sampleLines, (Action<PdfLine>)((PdfLine main) =>
            {
                bool isContact = false;

                foreach (PdfLine sub in targetLines)
                {
                    if (main.Index != sub.Index)
                    {
                        if (IsEndPointOnLine(
                            targetLine: sub,
                            x: main.StartX,
                            y: main.StartY,
                            marginContactLine: marginContactLine,
                            marginPointOnLineProjection: marginPointOnLineProjection
                        ))
                        {
                            isContact = true;
                            break;
                        }
                        else if (IsEndPointOnLine(
                            targetLine: sub,
                            x: main.EndX,
                            y: main.EndY,
                            marginContactLine: marginContactLine,
                            marginPointOnLineProjection: marginPointOnLineProjection
                        ))
                        {
                            isContact = true;
                            break;
                        }
                    }
                }

                if (!isContact)
                {
                    lock (sync)
                    {
                        nonEndToEndLines.Add(main);
                    }
                }
            }));

            return nonEndToEndLines;
        }

        static bool IsEndPointOnLine(
            PdfLine targetLine,
            double x,
            double y,
            double marginContactLine,
            double marginPointOnLineProjection
        )
        {
            // 일단 끝점이 서로 닿는지 확인한다.
            if (CoreMathUtils.IsPointOnLineEndPoint(line: targetLine, x: x, y: y, margin: marginContactLine))
            {
                return true;
            }
            else
            {
                // 끝점이 서로 닿지 않았으면 복잡한 처리를 해야 한다.
                if (targetLine.Orientation == Orientation.Horizontal || targetLine.Orientation == Orientation.Vertical)
                {
                    if (CoreMathUtils.IsPointInLineBoundary(line: targetLine, x: x, y: y, margin: marginContactLine))
                    {
                        return true;
                    }
                }
                else
                {
                    // 실제 이미지 상에서 오차가 있기 때문에 방정식을 통해 구하기 어렵다.
                    // 1. line의 boundary 안에 들어 오는지 확인한다.
                    // 2. 두 벡터의 각도가 수평인지 확인한다.
                    if (CoreMathUtils.IsPointInLineBoundary(line: targetLine, x: x, y: y) &&
                        CoreMathUtils.IsPointOnLineWithProjection(line: targetLine, x: x, y: y, margin: marginPointOnLineProjection))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
