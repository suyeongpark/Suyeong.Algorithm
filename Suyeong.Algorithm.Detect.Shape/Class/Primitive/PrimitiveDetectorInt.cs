using System;
using System.Collections.Generic;
using Suyeong.Lib.Type;

namespace Suyeong.Algorithm.Detect.Shape
{
    public static partial class PrimitiveDetector
    {
        public static PdfLineTriangleSampleCollection DetectTriangleSamples(
            IEnumerable<PdfLine> lines,
            double marginContact
        )
        {
            PdfLineTriangleSampleCollection triangles = new PdfLineTriangleSampleCollection();

            PdfLineCollection borders;
            bool isMatched;
            int index = 0;

            Dictionary<int, int> usedIdDic = new Dictionary<int, int>();

            foreach (PdfLine lineA in lines)
            {
                if (!usedIdDic.ContainsKey(lineA.Index))
                {
                    isMatched = false;

                    foreach (PdfLine lineB in lines)
                    {
                        if (!usedIdDic.ContainsKey(lineB.Index) &&
                            lineA.Index != lineB.Index &&
                            CoreMathUtils.IsContactLineEndToEnd(lineA: lineA, lineB: lineB, margin: marginContact))
                        {
                            foreach (PdfLine lineC in lines)
                            {
                                if (!usedIdDic.ContainsKey(lineC.Index) &&
                                    lineC.Index != lineA.Index &&
                                    lineC.Index != lineB.Index &&
                                    CoreMathUtils.IsContactLineEndToEnd(lineA: lineC, lineB: lineA, margin: marginContact) &&
                                    CoreMathUtils.IsContactLineEndToEnd(lineA: lineC, lineB: lineB, margin: marginContact))
                                {
                                    borders = new PdfLineCollection();
                                    borders.Add(lineA);
                                    borders.Add(lineB);
                                    borders.Add(lineC);

                                    triangles.Add(new PdfLineTriangleSample(
                                        index: index++,
                                        lines: borders
                                    ));

                                    foreach (PdfLine line in borders)
                                    {
                                        usedIdDic.Add(line.Index, line.Index);
                                    }

                                    isMatched = true;
                                    break;
                                }
                            }
                        }

                        if (isMatched)
                        {
                            break;
                        }
                    }
                }
            }

            return triangles;
        }

        public static PdfLineRectSampleCollection DetectRectSamples(
            IEnumerable<PdfLine> mainLines,
            IEnumerable<PdfLine> subLines,
            double marginContact
        )
        {
            PdfLineRectSampleCollection rects = new PdfLineRectSampleCollection();

            bool isMatched;
            int index = 0;
            Dictionary<int, int> usedIdDic = new Dictionary<int, int>();

            foreach (PdfLine mainMain in mainLines)
            {
                if (!usedIdDic.ContainsKey(mainMain.Index))
                {
                    isMatched = false;

                    foreach (PdfLine subMain in subLines)
                    {
                        if (!usedIdDic.ContainsKey(subMain.Index) &&
                            CoreMathUtils.IsContactLineEndToEnd(lineA: mainMain, lineB: subMain, margin: marginContact))
                        {
                            foreach (PdfLine mainSub in mainLines)
                            {
                                if (mainSub.Index != mainMain.Index &&
                                    !usedIdDic.ContainsKey(mainSub.Index) &&
                                    CoreMathUtils.IsContactLineEndToEnd(lineA: mainSub, lineB: subMain, margin: marginContact))
                                {
                                    foreach (PdfLine subSub in subLines)
                                    {
                                        if (subSub.Index != subMain.Index &&
                                            !usedIdDic.ContainsKey(subSub.Index) &&
                                            CoreMathUtils.IsContactLineEndToEnd(lineA: mainSub, lineB: subSub, margin: marginContact) &&
                                            CoreMathUtils.IsContactLineEndToEnd(lineA: mainMain, lineB: subSub, margin: marginContact))
                                        {
                                            rects.Add(new PdfLineRectSample(
                                                index: index,
                                                mainLine1: mainMain,
                                                mainLine2: mainSub,
                                                subLine1: subMain,
                                                subLine2: subSub
                                            ));

                                            usedIdDic.Add(mainMain.Index, mainMain.Index);
                                            usedIdDic.Add(mainSub.Index, mainSub.Index);
                                            usedIdDic.Add(subMain.Index, subMain.Index);
                                            usedIdDic.Add(subSub.Index, subSub.Index);

                                            index++;

                                            isMatched = true;
                                            break;
                                        }
                                    }
                                }

                                if (isMatched)
                                {
                                    break;
                                }
                            }
                        }

                        if (isMatched)
                        {
                            break;
                        }
                    }
                }
            }

            return rects;
        }
    }
}
