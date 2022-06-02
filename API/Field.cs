using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text;
using System.Threading.Tasks;

namespace TensionFields.API
{
    public class Field
    {
        private readonly Segment[,] _segments;

        public double MaxR { get; private set; }
        public double MinR { get; private set; }
        public double MaxZ { get; private set; }
        public double MinZ { get; private set; }
        public double MaxSI { get; private set; }
        public double MinSI { get; private set; }


        public Field(int N, int M, double[] R, double[] Z, double[] SI)
        {
            Point[,] vertices = new Point[N, M];
            _segments = new Segment[N - 1, M - 1];

            double tempR, tempZ;
            for (int r = 0; r < N; r++)
                for (int z = 0; z < M; z++)
                {
                    tempR = R[r * M + z];
                    tempZ = Z[r * M + z];
                    if (tempR > MaxR)
                        MaxR = tempR;
                    if (tempR < MinR)
                        MinR = tempR;
                    if (tempZ > MaxZ)
                        MaxZ = tempZ;
                    if (tempZ < MinZ)
                        MinZ = tempZ;
                    vertices[r, z] = new Point(tempR, tempZ);
                }

            double tempSI;
            for (int r = 0; r < N - 1; r++)
                for (int z = 0; z < M - 1; z++)
                {
                    tempSI = SI[r * (M - 1) + z];
                    if (tempSI > MaxSI)
                        MaxSI = tempSI;
                    if (tempSI < MinSI)
                        MinSI = tempSI;

                    _segments[r, z] = new Segment(tempSI, vertices[r, z], vertices[r + 1, z], vertices[r + 1, z + 1], vertices[r, z + 1]);
                }
        }

        public Field(double[,] R, double[,] Z, double[,] SI) : this(R.GetLength(0), R.GetLength(1),
                 R.Cast<double>().ToArray(), Z.Cast<double>().ToArray(), SI.Cast<double>().ToArray())
        { }

        //public Point[,] Vertices { get => _vertices; }
       // public double[,] Values { get => _values; }

        private (int, int)? FindSegment(double x, double y, int r, int z)
        {
            int N = _segments.GetLength(0);
            int M = _segments.GetLength(1);
            if (r < 0 || r >= N || z < 0 || z >= M)
                return null;

            Segment s = _segments[r, z];
            if (s.HasPoint(x, y))
                return (r, z);

            int shiftX = 0, shiftY = 0;

            if (x < s.LeftBottom.X || x < s.LeftTop.X)
                shiftX = 1;
            if (x > s.RightBottom.X || x > s.RightTop.X)
                shiftX = -1;

            if (y < s.LeftBottom.Y || y < s.RightBottom.Y)
                shiftY = 1;
            if (y > s.LeftTop.Y || y > s.RightTop.Y)
                shiftY = -1;

            return FindSegment(x, y, r + shiftX, z + shiftY);
        }

        public double? GetValue(double x, double y)
        {
            int N = _segments.GetLength(0);
            int M = _segments.GetLength(1);

            double xp = (x - MinR) / (MaxR - MinR);
            double yp = (y - MinZ) / (MaxZ - MinZ);

            (int, int)? segment = FindSegment(x, y, (int)(N * xp), (int)(M * yp));
            if (segment == null)
                return null;
            return _segments[segment.Value.Item1, segment.Value.Item2].GetValue(x, y);
        }
    }
}
