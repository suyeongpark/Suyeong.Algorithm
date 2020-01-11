using System;
using System.Collections.Generic;

namespace Suyeong.Algorithm.DocDetector
{
    public struct ImageCell
    {
        public ImageCell(int index, int rowIndex, int columnIndex, int rowSize, int columnSize, int topY, int bottomY, int leftX, int rightX)
        {
            this.Index = index;
            this.RowIndex = rowIndex;
            this.ColumnIndex = columnIndex;
            this.RowSize = rowSize;
            this.ColumnSize = columnSize;
            this.TopY = topY;
            this.BottomY = bottomY;
            this.LeftX = leftX;
            this.RightX = rightX;
            this.Width = rightX - leftX;
            this.Height = bottomY - topY;

            this.CenterX = (int)Math.Round((this.LeftX + this.RightX) * 0.5d, MidpointRounding.AwayFromZero);
            this.CenterY = (int)Math.Round((this.TopY + this.BottomY) * 0.5d, MidpointRounding.AwayFromZero);
        }

        public int Index { get; private set; }
        public int RowIndex { get; private set; }
        public int ColumnIndex { get; private set; }
        public int RowSize { get; private set; }
        public int ColumnSize { get; private set; }
        public int TopY { get; private set; }
        public int BottomY { get; private set; }
        public int LeftX { get; private set; }
        public int RightX { get; private set; }
        public int CenterX { get; private set; }
        public int CenterY { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
    }

    public class ImageCellCollection : List<ImageCell>
    {
        public ImageCellCollection()
        {
        }

        public ImageCellCollection(IEnumerable<ImageCell> cells) : base()
        {
            this.AddRange(cells);
        }
    }
}
