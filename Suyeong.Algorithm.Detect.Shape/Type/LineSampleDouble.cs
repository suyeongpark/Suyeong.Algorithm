using System;
using System.Collections.Generic;
using Suyeong.Lib.Type;

namespace Suyeong.Algorithm.Detect.Shape
{
    public struct LineSampleDouble : ILine<double> 
    {
        public LineSampleDouble(
            int index,
            LineOrientation orientation,
            int typeIndex,
            double startX,
            double startY,
            double endX,
            double endY
        )
        {
            this.Index = index;
            this.Orientation = orientation;
            this.TypeIndex = typeIndex;
            this.StartX = startX;
            this.StartY = startY;
            this.EndX = endX;
            this.EndY = endY;

            // x가 작은 쪽을 start로 강제한다. x가 동일하면 y가 작은 쪽은 start로 강제한다.
            if (startX > endX || (Math.Abs(Math.Abs(startX) - Math.Abs(endX)) <= double.Epsilon && startY > endY))
            {
                this.StartX = endX;
                this.StartY = endY;
                this.EndX = startX;
                this.EndY = startY;
            }
            else
            {
                this.StartX = startX;
                this.StartY = startY;
                this.EndX = endX;
                this.EndY = endY;
            }

            // x가 작은 쪽이 무조건 start이기 때문에 minX는 startX가 된다.
            this.MinX = this.StartX;
            this.MaxX = this.EndX;

            if (this.StartY < this.EndY)
            {
                this.MinY = this.StartY;
                this.MaxY = this.EndY;
            }
            else
            {
                this.MinY = this.EndY;
                this.MaxY = this.StartY;
            }

            this.CenterX = (int)Math.Round((this.StartX + this.EndX) * 0.5d, MidpointRounding.AwayFromZero);
            this.CenterY = (int)Math.Round((this.StartY + this.EndY) * 0.5d, MidpointRounding.AwayFromZero);
            this.DeltaX = this.EndX - this.StartX;
            this.DeltaY = this.EndY - this.StartY;
            this.LengthSquare = (this.DeltaX * this.DeltaX) + (this.DeltaY * this.DeltaY);
        }

        public int Index { get; private set; }
        public LineOrientation Orientation { get; private set; }
        public int TypeIndex { get; private set; }
        public double StartX { get; private set; }
        public double StartY { get; private set; }
        public double EndX { get; private set; }
        public double EndY { get; private set; }
        public double MinX { get; private set; }
        public double MinY { get; private set; }
        public double MaxX { get; private set; }
        public double MaxY { get; private set; }
        public double CenterX { get; private set; }
        public double CenterY { get; private set; }
        public double DeltaX { get; private set; }
        public double DeltaY { get; private set; }
        public double LengthSquare { get; private set; }
    }

    public class LineSampleDoubleCollection : List<LineSampleDouble> 
    {
        public LineSampleDoubleCollection()
        {

        }

        public LineSampleDoubleCollection(LineSampleDouble line) : base()
        {
            this.Add(line);
        }

        public LineSampleDoubleCollection(IEnumerable<LineSampleDouble> lines) : base()
        {
            if (lines != null)
            {
                this.AddRange(lines);
            }
        }
    }

    public class LineSampleDoubleGroup : Dictionary<int, LineSampleDoubleCollection> 
    {
        public LineSampleDoubleGroup()
        {

        }

        public LineSampleDoubleGroup(IEnumerable<KeyValuePair<int, LineSampleDoubleCollection>> dic) : base()
        {
            if (dic != null)
            {
                foreach (KeyValuePair<int, LineSampleDoubleCollection> kvp in dic)
                {
                    this.Add(kvp.Key, kvp.Value);
                }
            }
        }
    }
}
