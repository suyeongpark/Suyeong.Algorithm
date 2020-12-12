using System.Collections.Generic;

namespace Suyeong.Algorithm.Detect.Table
{
    public struct ImageTable
    {
        public ImageTable(
            int index, 
            int x, 
            int y, 
            int width, 
            int height, 
            ImageCellCollection cells
        )
        {
            this.Index = index;
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
            this.Cells = cells;
        }

        public int Index { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public ImageCellCollection Cells { get; private set; }
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
