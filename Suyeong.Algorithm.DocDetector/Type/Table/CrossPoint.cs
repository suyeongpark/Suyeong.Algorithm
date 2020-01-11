using System.Collections.Generic;

namespace Suyeong.Algorithm.DocDetector
{
    struct CrossPoint
    {
        public CrossPoint(bool existTopLine, bool existLeftLine, int index, int rowIndex, int columnIndex, int x, int y, CellLine horizontal, CellLine vertical)
        {
            ExistTopLine = existTopLine;
            ExistLeftLine = existLeftLine;
            Index = index;
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            X = x;
            Y = y;
            Horizontal = horizontal;
            Vertical = vertical;
        }

        public bool ExistTopLine { get; private set; }
        public bool ExistLeftLine { get; private set; }
        public int Index { get; private set; }
        public int RowIndex { get; private set; }
        public int ColumnIndex { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public CellLine Horizontal { get; private set; }
        public CellLine Vertical { get; private set; }
    }

    class CrossPoints : List<CrossPoint>
    {
        public CrossPoints()
        {
        }

        public CrossPoints(IEnumerable<CrossPoint> crossPoints)
        {
            this.AddRange(crossPoints);
        }
    }
}
