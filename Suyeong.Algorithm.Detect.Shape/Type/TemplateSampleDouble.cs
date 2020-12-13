using System;
using System.Collections.Generic;
using System.Linq;
using Suyeong.Lib.Type;

namespace Suyeong.Algorithm.Detect.Shape
{
    public struct TemplateSampleDouble : IRect<double>
    {
        public TemplateSampleDouble(
            int index,
            int typeIndex,
            double rotate,
            LineSampleDoubleCollection lines
        )
        {
            this.Index = index;
            this.TypeIndex = typeIndex;
            this.Rotate = rotate;
            this.Lines = lines;
            this.BaseLine = this.Lines.FirstOrDefault();

            this.MinX = int.MaxValue;
            this.MinY = int.MaxValue;
            this.MaxX = int.MinValue;
            this.MaxY = int.MinValue;

            foreach (LineSampleDouble line in this.Lines)
            {
                if (line.MinX < this.MinX)
                {
                    this.MinX = line.MinX;
                }

                if (line.MaxX > this.MaxX)
                {
                    this.MaxX = line.MaxX;
                }

                if (line.MinY < this.MinY)
                {
                    this.MinY = line.MinY;
                }

                if (line.MaxY > this.MaxY)
                {
                    this.MaxY = line.MaxY;
                }
            }

            this.LeftX = this.MinX;
            this.RightX = this.MaxX;
            this.TopY = this.MaxY;
            this.BottomY = this.MinY;
            this.CenterX = (int)Math.Round((this.MinX + this.MaxX) * 0.5d, MidpointRounding.AwayFromZero);
            this.CenterY = (int)Math.Round((this.MinY + this.MaxY) * 0.5d, MidpointRounding.AwayFromZero);
            this.X = this.LeftX;
            this.Y = this.BottomY;
            this.Width = this.RightX - this.LeftX;
            this.Height = this.TopY - this.BottomY;
            this.Area = this.Width * this.Height;
            this.DiagonalSquare = this.Width * this.Width + this.Height * this.Height;
        }

        public int Index { get; private set; }
        public int TypeIndex { get; private set; }
        public double LeftX { get; private set; }
        public double RightX { get; private set; }
        public double TopY { get; private set; }
        public double BottomY { get; private set; }
        public double MinX { get; private set; }
        public double MinY { get; private set; }
        public double MaxX { get; private set; }
        public double MaxY { get; private set; }
        public double CenterX { get; private set; }
        public double CenterY { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public double Rotate { get; private set; }
        public double Area { get; private set; }
        public double DiagonalSquare { get; private set; }
        public LineSampleDouble BaseLine { get; private set; }
        public LineSampleDoubleCollection Lines { get; private set; }
    }

    public class TemplateSampleDoubleCollection : List<TemplateSampleDouble>
    {
        public TemplateSampleDoubleCollection()
        {

        }

        public TemplateSampleDoubleCollection(TemplateSampleDouble line) : base()
        {
            this.Add(line);
        }

        public TemplateSampleDoubleCollection(IEnumerable<TemplateSampleDouble> lines) : base()
        {
            if (lines != null)
            {
                this.AddRange(lines);
            }
        }
    }

    public class TemplateSampleDoubleGroup : Dictionary<int, TemplateSampleDoubleCollection>
    {
        public TemplateSampleDoubleGroup()
        {

        }

        public TemplateSampleDoubleGroup(IEnumerable<KeyValuePair<int, TemplateSampleDoubleCollection>> dic) : base()
        {
            if (dic != null)
            {
                foreach (KeyValuePair<int, TemplateSampleDoubleCollection> kvp in dic)
                {
                    this.Add(kvp.Key, kvp.Value);
                }
            }
        }
    }
}
