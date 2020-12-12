using System.Collections.Generic;

namespace Suyeong.Algorithm.Detect.Table
{
    internal struct CellBox
    {
        internal CellBox(
            int index, 
            CellLineCollection horizontals, 
            CellLineCollection verticals
        )
        {
            this.Index = index;
            this.Horizontals = horizontals;
            this.Verticals = verticals;
        }

        public int Index { get; private set; }
        public CellLineCollection Horizontals { get; private set; }
        public CellLineCollection Verticals { get; private set; }
    }

    internal class CellBoxCollection : List<CellBox>
    {
        public CellBoxCollection()
        {

        }

        public CellBoxCollection(IEnumerable<CellBox> boxes) : base()
        {
            this.AddRange(boxes);
        }
    }
}
