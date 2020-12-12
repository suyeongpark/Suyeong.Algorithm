using System;
using System.Collections.Generic;

namespace Suyeong.Algorithm.Detect.Table
{
    internal struct CellLine
    {
        internal CellLine(
            int index, 
            int startX, 
            int startY, 
            int endX, 
            int endY, 
            int thickness
        )
        {
            this.Index = index;
            this.StartX = startX;
            this.StartY = startY;
            this.EndX = endX;
            this.EndY = endY;
            this.Thickness = thickness;

            this.CenterX = (int)Math.Round((this.StartX + this.EndX) * 0.5d, MidpointRounding.AwayFromZero);
            this.CenterY = (int)Math.Round((this.StartY + this.EndY) * 0.5d, MidpointRounding.AwayFromZero);

            if (this.StartX <= this.EndX)
            {
                this.MinX = this.StartX;
                this.MaxX = this.EndX;
            }
            else
            {
                this.MinX = this.EndX;
                this.MaxX = this.StartX;
            }

            if (this.StartY <= this.EndY)
            {
                this.MinY = this.StartY;
                this.MaxY = this.EndY;
            }
            else
            {
                this.MinY = this.EndY;
                this.MaxY = this.StartY;
            }
        }

        public int Index { get; private set; }
        public int StartX { get; private set; }
        public int StartY { get; private set; }
        public int EndX { get; private set; }
        public int EndY { get; private set; }
        public int CenterX { get; private set; }
        public int CenterY { get; private set; }
        public int MinX { get; private set; }
        public int MinY { get; private set; }
        public int MaxX { get; private set; }
        public int MaxY { get; private set; }
        public int Thickness { get; private set; }
    }

    internal class CellLineCollection : List<CellLine>
    {
        internal CellLineCollection()
        {
        }

        internal CellLineCollection(IEnumerable<CellLine> lines) : base()
        {
            this.AddRange(lines);
        }
    }
}
