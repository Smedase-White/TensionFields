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

        public Segment(double value, params Point[] vertices) : this((double x, double y) => value, vertices) { }

        public Point RightTop { get => _vertices[0]; private set => _vertices[0] = value; }
        public Point LeftTop { get => _vertices[1]; private set => _vertices[1] = value; }
        public Point LeftBottom { get => _vertices[2]; private set => _vertices[2] = value; }
        public Point RightBottom { get => _vertices[3]; private set => _vertices[3] = value; }

        public Vector Bottom { get => RightBottom - LeftBottom; }
        public Vector Right { get => RightTop - RightBottom; }
        public Vector Top { get => LeftTop - RightTop; }
        public Vector Left { get => LeftBottom - LeftTop; }

        public bool IsRightOfRight(double x, double y) => CalcSide(x, y, 0) > 0;
        public bool IsAboveTop(double x, double y) => CalcSide(x, y, 1) > 0;
        public bool IsLeftOfLeft(double x, double y) => CalcSide(x, y, 2) > 0;
        public bool IsBellowBottom(double x, double y) => CalcSide(x, y, 3) > 0;

        public bool HasPoint(double x, double y)
        {
            /*
            double d = Right.X * Top.Y - Top.X * Right.Y;
            double dx = (Right.X * (y - RightTop.Y) - (x - RightTop.X) * Right.Y) / d;
            double dy = (Top.X * (y - RightTop.Y) - (x - RightTop.X) * Top.Y) / d;
            return 0 <= dx && dx <= 1 && 0 <= dy && dy <= 1;
            */

            /*
            for (int i = 0; i < 4; i++)
                if (x == _vertices[i].X && y == _vertices[i].Y)
                    return true;
            
            int result = 0;
            for (int i = 0; i < 4; i++)
                result += IsIntersectLine(x, y, i) ? 1 : 0;
            return result % 2 == 1;
            */


            for (int i = 0; i < 4; i++)
                if (CalcSide(x, y, i) > 0)
                    return false;
            return true;
        }

        private double CalcSide(double x, double y, int i)
        {
            int j = (i + 3) % 4;
            return (x - _vertices[j].X) * (_vertices[i].Y - _vertices[j].Y) - (y - _vertices[j].Y) * (_vertices[i].X - _vertices[j].X);
        }

        private bool IsIntersectLine(double x, double y, int i)
        {
            int j = (i + 3) % 4;

            bool isBetweenY = (_vertices[i].Y < y && y <= _vertices[j].Y) || (_vertices[j].Y < y && y <= _vertices[i].Y);
            if (isBetweenY == false)
                return false;

            bool isHorizontalLine = _vertices[i].Y == _vertices[j].Y;
            if (isHorizontalLine == true)
                return false;

            double intersection = _vertices[i].X + (y - _vertices[i].Y) * (_vertices[j].X - _vertices[i].X) / (_vertices[j].Y - _vertices[i].Y);
            return intersection < x;
        }

        public double? GetValue()
        {
            return _function((_vertices[0].X + _vertices[2].X) / 2, (_vertices[1].Y + _vertices[3].Y) / 2);
        }

        public double? GetValue(double x, double y)
        {
            /*if (HasPoint(x, y) == false)
                return null;*/
            return _function(x, y);
        }
    }
}
