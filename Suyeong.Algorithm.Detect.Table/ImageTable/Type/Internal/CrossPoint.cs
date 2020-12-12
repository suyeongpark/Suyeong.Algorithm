using System.Collections.Generic;

namespace Suyeong.Algorithm.Detect.Table
{
    internal struct CrossPoint
    {
        internal CrossPoint(
            int index, 
            int rowIndex, 
            int columnIndex, 
            int x, 
            int y, 
            CellLine horizontal, 
            CellLine vertical,
            bool existTopLine, 
            bool existLeftLine
        )
        {
            this.Index = index;
            this.RowIndex = rowIndex;
            this.ColumnIndex = columnIndex;
            this.X = x;
            this.Y = y;
            this.Horizontal = horizontal;
            this.Vertical = vertical;
            this.ExistTopLine = existTopLine;
            this.ExistLeftLine = existLeftLine;
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

    internal class CrossPointCollection : List<CrossPoint>
    {
        internal CrossPointCollection()
        {
        }

        internal CrossPointCollection(IEnumerable<CrossPoint> crossPoints)
        {
            this.AddRange(crossPoints);
        }
    }
}
