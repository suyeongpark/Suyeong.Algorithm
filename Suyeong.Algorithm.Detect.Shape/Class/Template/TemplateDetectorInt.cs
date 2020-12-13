using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suyeong.Algorithm.Detect.Shape
{
    public static partial class TemplateDetector
    {
        // 기본 값
        const int SAMPLING_MAX = 1000;

        const int MATCHING_TEMPLATE_MIN_INT = 0;
        const int MATCHING_TEMPLATE_MAX_INT = 2;
        const int SAMPLING_START_POINT_INT = 1;
        const int CONTAIN_POSITION_INT = 0;

        public static TemplateSampleIntCollection DetectTemplateSymbolSamples(
            IEnumerable<LineSampleInt> sampleLines,
            IEnumerable<TemplateSampleInt> templates,
            int sizeSampling,
            int marginDetect,
            int scaleTemplateMin = MATCHING_TEMPLATE_MIN_INT,
            int scaleTemplateMax = MATCHING_TEMPLATE_MAX_INT,
            int marginSamplingStartPoint = SAMPLING_START_POINT_INT,
            int marginContain = CONTAIN_POSITION_INT,
            int countSamplingMax = SAMPLING_MAX,
            bool removeContainSymbol = true
        ) 
        {
            // 매칭을 위해 line을 샘플링한다. 라인 주위에 있는 라인을 모음
            // readable하지 않은 pdf에서 글자를 sampling하면서 메모리 에러가 난다. 때문에 타입이 아니라 id만 사용하는 식으로 처리함.
            Dictionary<int, Dictionary<int, int>> samplesDic = GetSymbolSampleLines(
                sampleLines: sampleLines,
                sizeSampling: sizeSampling,
                marginSamplingStartPoint: marginSamplingStartPoint,
                countSamplingMax: countSamplingMax
            );

            // 샘플링된 라인을 이용해서 심볼을 매칭한다. --병렬로 돌기 때문에 중복이 있음
            TemplateSampleIntCollection templateSymbols = DetectSymbols(
                templates: templates,
                sampleLines: sampleLines,
                scaleTemplateMin: scaleTemplateMin,
                scaleTemplateMax: scaleTemplateMax,
                marginDetect: marginDetect,
                samplesDic: samplesDic
            );

            if (removeContainSymbol)
            {
                // 중복 제거
                return RemoveContainSymbol(symbols: templateSymbols, marginContain: marginContain);
            }
            else
            {
                return templateSymbols;
            }
        }

        static Dictionary<int, Dictionary<int, int>> GetSymbolSampleLines(
            IEnumerable<LineSampleInt> sampleLines,
            int sizeSampling,
            int marginSamplingStartPoint,
            int countSamplingMax
        ) 
        {
            Dictionary<int, Dictionary<int, int>> samplesDic = new Dictionary<int, Dictionary<int, int>>();

            object sync = new object();

            ParallelLoopResult result = Parallel.ForEach(sampleLines, (LineSampleInt sampleLine) =>
            {
                if (sampleLine.Index > 0)
                {
                    int leftX = sampleLine.CenterX - sizeSampling;
                    int rightX = sampleLine.CenterX + sizeSampling;
                    int bottomY = sampleLine.MinY - marginSamplingStartPoint; // 수치가 정확하지 않아서 같은 높이인데 샘플에서 빠지는 경우가 있다.
                    int topY = sampleLine.MinY + sizeSampling; // MaxY가 아니라 MinY인 이유는 최소점을 기준으로 sampleDistance 만큼 사각형을 잡기 때문

                    // sampling 영역 안에 있는 것 중에서 현재 라인과 layer(주석인지 본문인지)와 color가 동일한 것을 모은다.
                    // sampling 할 때만 유사한 것을 모으고, matching 할 때는 해당 사항은 고려하지 않는다.
                    LineSampleIntCollection samples = new LineSampleIntCollection(sampleLines.Where(sub =>
                       sub.Index != sampleLine.Index &&
                       sub.MinX >= leftX &&
                       sub.MaxX <= rightX &&
                       sub.MinY >= bottomY &&
                       sub.MaxY <= topY
                    ).OrderBy(line => line.MinY).ThenBy(line => line.MinX));

                    if (samples.Any())
                    {
                        IEnumerable<LineSampleInt> samplesRange = samples.GetRange(0, samples.Count <= countSamplingMax ? samples.Count : countSamplingMax);

                        lock (sync)
                        {
                            samplesDic.Add(sampleLine.Index, samplesRange.ToDictionary(line => line.Index, line => line.Index));
                        }
                    }
                }
            });

            return samplesDic;
        }

        static TemplateSampleIntCollection DetectSymbols(
            IEnumerable<TemplateSampleInt> templates,
            IEnumerable<LineSampleInt> sampleLines,
            Dictionary<int, Dictionary<int, int>> samplesDic,
            double marginDetect,
            double scaleTemplateMin,
            double scaleTemplateMax
        ) 
        {
            TemplateSampleIntCollection symbols = new TemplateSampleIntCollection();

            object sync = new object();

            ParallelLoopResult result = Parallel.ForEach(sampleLines, (LineSampleInt sampleLine) =>
            {
                Dictionary<int, int> sampleLineDic;
                LineSampleIntCollection samples, symbolLines;
                LineSampleInt templateLine;
                double scale, minX, minY, maxX, maxY;
                bool find;

                if (samplesDic.TryGetValue(sampleLine.Index, out sampleLineDic))
                {
                    samples = new LineSampleIntCollection(sampleLines.Where(line => sampleLineDic.ContainsKey(line.Index)));

                    foreach (TemplateSampleInt template in templates)
                    {
                        // sample에 자기 자신은 포함되고 있지 않기 때문
                        if (template.Lines.Count <= samples.Count + 1)
                        {
                            if (Math.Abs((sampleLine.DeltaY * template.BaseLine.DeltaX) - (sampleLine.DeltaX * template.BaseLine.DeltaY)) < marginDetect)
                            {
                                scale = template.BaseLine.DeltaX == 0 ? (double)sampleLine.DeltaY / (double)template.BaseLine.DeltaY : (double)sampleLine.DeltaX / (double)template.BaseLine.DeltaX;

                                if (scale >= scaleTemplateMin && scale <= scaleTemplateMax)
                                {
                                    if (MathUtils.IsSameAngle(
                                        deltaX1: sampleLine.DeltaX,
                                        deltaY1: sampleLine.DeltaY,
                                        deltaX2: template.BaseLine.DeltaX * scale,
                                        deltaY2: template.BaseLine.DeltaY * scale,
                                        margin: marginDetect))
                                    {
                                        symbolLines = new LineSampleIntCollection();

                                        // 1번부터 돌기 때문에 먼저 추가한다.
                                        symbolLines.Add(sampleLine);

                                        for (int i = 1; i < template.Lines.Count; i++)
                                        {
                                            templateLine = template.Lines[i];
                                            find = false;

                                            // A의 Start가 B의 End이고 B의 Start가 A의 End인 것 같이 같은 라인이 방향만 다른 경우가 있기 때문에 Min/Max로 비교
                                            // 이경우 positive와 negative가 같게 판명날 수 있는데, pdf line을 만들 때 이에 대해 정렬을 했기 때문에 문제 없다.
                                            minX = sampleLine.MinX - ((template.BaseLine.MinX - templateLine.MinX) * scale);
                                            minY = sampleLine.MinY - ((template.BaseLine.MinY - templateLine.MinY) * scale);
                                            maxX = sampleLine.MaxX - ((template.BaseLine.MaxX - templateLine.MaxX) * scale);
                                            maxY = sampleLine.MaxY - ((template.BaseLine.MaxY - templateLine.MaxY) * scale);

                                            // 순서는 따지지 않고 있는지 찾는다.
                                            foreach (LineSampleInt lineSub in samples)
                                            {
                                                if (lineSub.TypeIndex == templateLine.TypeIndex && 
                                                    Math.Abs(lineSub.MinX - minX) <= marginDetect &&
                                                    Math.Abs(lineSub.MinY - minY) <= marginDetect &&
                                                    Math.Abs(lineSub.MaxX - maxX) <= marginDetect &&
                                                    Math.Abs(lineSub.MaxY - maxY) <= marginDetect)
                                                {
                                                    symbolLines.Add(lineSub);
                                                    find = true;
                                                    break;
                                                }
                                            }

                                            if (!find)
                                            {
                                                break;
                                            }
                                        }

                                        if (symbolLines.Count == template.Lines.Count)
                                        {
                                            lock (sync)
                                            {
                                                symbols.Add(new TemplateSampleInt(
                                                    index: 0,
                                                    typeIndex: template.TypeIndex,
                                                    rotate: template.Rotate,
                                                    lines: symbolLines
                                                ));

                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            return symbols;
        }

        static TemplateSampleIntCollection RemoveContainSymbol(
            IEnumerable<TemplateSampleInt> symbols,
            double marginContain
        ) 
        {
            TemplateSampleIntCollection symbolsNew = new TemplateSampleIntCollection();

            TemplateSampleIntCollection orderByArea = new TemplateSampleIntCollection(symbols.OrderBy(symbol => symbol.Area).ThenBy(symbol => symbol.Lines.Count));
            TemplateSampleInt main, sub;
            bool isContain;
            int index = 0;

            for (int i = 0; i < orderByArea.Count; i++)
            {
                main = orderByArea[i];
                isContain = false;

                for (int j = i + 1; j < orderByArea.Count; j++)
                {
                    sub = orderByArea[j];

                    // 완전히 포함되는 다른 심볼이 있으면 추가하지 않는다.
                    if (sub.MinX - main.MinX <= marginContain &&
                        main.MaxX - sub.MaxX <= marginContain &&
                        sub.MinY - main.MinY <= marginContain &&
                        main.MaxY - sub.MaxY <= marginContain
                    )
                    {
                        isContain = true;
                        break;
                    }
                }

                if (!isContain)
                {
                    // 매칭은 parallel로 돌아서 index를 부여를 못했기 때문에 여기서 한다. - index는 type과 무관하게 고유한 값
                    symbolsNew.Add(new TemplateSampleInt(
                        index: index++,
                        typeIndex: main.TypeIndex,
                        rotate: main.Rotate,
                        lines: main.Lines
                    ));
                }
            }

            return symbolsNew;
        }
    }
}
