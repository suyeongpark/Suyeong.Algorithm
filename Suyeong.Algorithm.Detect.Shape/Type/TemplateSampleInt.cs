using System;
using System.Collections.Generic;
using System.Linq;
using Suyeong.Lib.Type;

namespace Suyeong.Algorithm.Detect.Shape
{
    public struct TemplateSampleInt : IRect<int>
    {
        public TemplateSampleInt(
            int index,
            int typeIndex,
            int rotate,
            LineSampleIntCollection lines
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

            foreach (LineSampleInt line in this.Lines)
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
        public int LeftX { get; private set; }
        public int RightX { get; private set; }
        public int TopY { get; private set; }
        public int BottomY { get; private set; }
        public int MinX { get; private set; }
        public int MinY { get; private set; }
        public int MaxX { get; private set; }
        public int MaxY { get; private set; }
        public int CenterX { get; private set; }
        public int CenterY { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Rotate { get; private set; }
        public int Area { get; private set; }
        public int DiagonalSquare { get; private set; }
        public LineSampleInt BaseLine { get; private set; }
        public LineSampleIntCollection Lines { get; private set; }
    }

    public class TemplateSampleIntCollection : List<TemplateSampleInt>
    {
        public TemplateSampleIntCollection()
        {

        }

        public TemplateSampleIntCollection(TemplateSampleInt line) : base()
        {
            this.Add(line);
        }

        public TemplateSampleIntCollection(IEnumerable<TemplateSampleInt> lines) : base()
        {
            if (lines != null)
            {
                this.AddRange(lines);
            }
        }
    }

    public class TemplateSampleIntGroup : Dictionary<int, TemplateSampleIntCollection>
    {
        public TemplateSampleIntGroup()
        {

        }

        public TemplateSampleIntGroup(IEnumerable<KeyValuePair<int, TemplateSampleIntCollection>> dic) : base()
        {
            if (dic != null)
            {
                foreach (KeyValuePair<int, TemplateSampleIntCollection> kvp in dic)
                {
                    this.Add(kvp.Key, kvp.Value);
                }
            }
        }
    }
}
