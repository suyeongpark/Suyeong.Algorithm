using System.Collections.Generic;

namespace Suyeong.Algorithm.DocDetector
{
    public struct ImageTable
    {
        public ImageTable(int index, int x, int y, int width, int height, ImageCellCollection cells)
        {
            this.Index = index;
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
            this.cells = cells;
        }

        public int Index { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public ImageCellCollection cells { get; private set; }
    }

    public class ImageTableCollection : List<ImageTable>
    {
        public ImageTableCollection()
        {

        }

        public ImageTableCollection(IEnumerable<ImageTable> tables) : base()
        {
            this.AddRange(tables);
        }
    }
}
