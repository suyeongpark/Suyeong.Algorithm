using System.Collections.Generic;

namespace Suyeong.Algorithm.DocDetector
{
    public struct ImageTable
    {
        public ImageTable(int index, ImageCells cells)
        {
            this.Index = index;
            this.cells = cells;

            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;

            foreach (ImageCell cell in cells)
            {
                if (cell.LeftX < minX)
                {
                    minX = cell.LeftX;
                }

                if (cell.RightX > maxX)
                {
                    maxX = cell.RightX;
                }

                if (cell.TopY < minY)
                {
                    minY = cell.TopY;
                }

                if (cell.BottomY > maxY)
                {
                    maxY = cell.BottomY;
                }
            }

            X = minX;
            Y = minY;
            Width = maxX - minX;
            Height = maxY - minY;
        }

        public int Index { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public ImageCells cells { get; private set; }
    }

    public class ImageTables : List<ImageTable>
    {
        public ImageTables()
        {

        }

        public ImageTables(IEnumerable<ImageTable> tables) : base()
        {
            this.AddRange(tables);
        }
    }
}
