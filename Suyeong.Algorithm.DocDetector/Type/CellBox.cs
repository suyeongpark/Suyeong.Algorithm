using System.Collections.Generic;

namespace Suyeong.Algorithm.DocDetector
{
    struct CellBox
    {
        public CellBox(int index, CellLines horizontals, CellLines verticals)
        {
            this.Index = index;
            this.Horizontals = horizontals;
            this.Verticals = verticals;
        }

        public int Index { get; private set; }
        public CellLines Horizontals { get; private set; }
        public CellLines Verticals { get; private set; }
    }

    class CellBoxes : List<CellBox>
    {
        public CellBoxes()
        {

        }

        public CellBoxes(IEnumerable<CellBox> boxes) : base()
        {
            this.AddRange(boxes);
        }
    }
}
