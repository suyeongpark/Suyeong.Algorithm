using System;
using System.Collections.Generic;
using Suyeong.Lib.Type;

namespace Suyeong.Algorithm.Detect.Shape
{
    public struct LineSampleInt : ILine<int>
    {
        public LineSampleInt(
            int index,
            LineOrientation orientation,
            int typeIndex,
            int startX,
            int startY,
            int endX,
            int endY
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
            if (startX > endX || (startX == endX && startY > endY))
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
        public int StartX { get; private set; }
        public int StartY { get; private set; }
        public int EndX { get; private set; }
        public int EndY { get; private set; }
        public int MinX { get; private set; }
        public int MinY { get; private set; }
        public int MaxX { get; private set; }
        public int MaxY { get; private set; }
        public int CenterX { get; private set; }
        public int CenterY { get; private set; }
        public int DeltaX { get; private set; }
        public int DeltaY { get; private set; }
        public int LengthSquare { get; private set; }
    }

    public class LineSampleIntCollection : List<LineSampleInt>
    {
        public LineSampleIntCollection()
        {

        }

        public LineSampleIntCollection(LineSampleInt line) : base()
        {
            this.Add(line);
        }

        public LineSampleIntCollection(IEnumerable<LineSampleInt> lines) : base()
        {
            if (lines != null)
            {
                this.AddRange(lines);
            }
        }
    }

    public class LineSampleIntGroup : Dictionary<int, LineSampleIntCollection> 
    {
        public LineSampleIntGroup()
        {

        }

        public LineSampleIntGroup(IEnumerable<KeyValuePair<int, LineSampleIntCollection>> dic) : base()
        {
            if (dic != null)
            {
                foreach (KeyValuePair<int, LineSampleIntCollection> kvp in dic)
                {
                    this.Add(kvp.Key, kvp.Value);
                }
            }
        }
    }
}
