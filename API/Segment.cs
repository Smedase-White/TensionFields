using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TensionFields.API
{
    public delegate double function(double x, double y);

    public class Segment
    {
        private readonly Point[] _vertices;
        private function _function;
        
        public Segment(function func, params Point[] vertices)
        {
            if (vertices.Length != 4)
                throw new ArgumentException("Invalid points array.");
            _vertices = new Point[4];
            for (int i = 0; i < vertices.Length; i++)
                _vertices[i] = vertices[i];
            _function = func;
        }

        public Point RightTop { get => _vertices[0]; private set => _vertices[0] = value; }
        public Point LeftTop { get => _vertices[1]; private set => _vertices[1] = value; }
        public Point LeftBottom { get => _vertices[2]; private set => _vertices[2] = value; }
        public Point RightBottom { get => _vertices[3]; private set => _vertices[3] = value; }

        public Vector Bottom { get => RightBottom - LeftBottom; }
        public Vector Right { get => RightTop - RightBottom; }
        public Vector Top { get => LeftTop - RightTop; }
        public Vector Left { get => LeftBottom - LeftTop; }

        public Segment(double value, params Point[] vertices) : this((double x, double y) => value, vertices) { }

        public bool HasPoint(double x, double y)
        {
            /*double d = Right.X * Top.Y - Top.X * Right.Y;
            double dx = (Right.X * (y - RightTop.Y) - (x - RightTop.X) * Right.Y) / d;
            double dy = (Top.X * (y - RightTop.Y) - (x - RightTop.X) * Top.Y) / d;
            return 0 <= dx && dx <= 1 && 0 <= dy && dy <= 1;*/

            bool result = false;
            int j = 3;
            for (int i = 0; i < 4; i++)
            {
                if ((_vertices[i].Y < y && _vertices[j].Y >= y || _vertices[j].Y < y && _vertices[i].Y >= y) &&
                    (_vertices[i].X + (y - _vertices[i].Y) / (_vertices[j].Y - _vertices[i].Y) * (_vertices[j].X - _vertices[i].X) < x))
                    result = !result;
                j = i;
            }
            return result;
        }

        public double? GetValue(double x, double y)
        {
            if (HasPoint(x, y) == false)
                return null;
            return _function(x, y);
        }
    }
}
