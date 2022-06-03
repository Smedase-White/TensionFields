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
        private readonly Point[,] _vertices;
        private readonly Segment[,] _segments;

        public double MaxR { get; private set; }
        public double MinR { get; private set; }

        public double MaxZ { get; private set; }
        public double MinZ { get; private set; }

        public double MaxSI { get; private set; }
        public double MinSI { get; private set; }

        public Field(int N, int M, double[] R, double[] Z, double[] SI)
        {
            _vertices = new Point[N, M];
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
                    _vertices[r, z] = new Point(tempR, tempZ);
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

                    _segments[r, z] = new Segment(tempSI, _vertices[r, z], _vertices[r + 1, z], _vertices[r + 1, z + 1], _vertices[r, z + 1]);
                }
        }

        public Field(double[,] R, double[,] Z, double[,] SI) : this(R.GetLength(0), R.GetLength(1),
                 R.Cast<double>().ToArray(), Z.Cast<double>().ToArray(), SI.Cast<double>().ToArray())
        { }

        public Segment[,] Segments { get => _segments; }
        public Point[,] Vertices { get => _vertices; }
        //public double[,] Values { get => _values; }

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

            if (s.IsLeftOfLeft(x, y))
                shiftX = 1;
            if (s.IsRightOfRight(x, y))
                shiftX = -1;

            if (s.IsBellowBottom(x, y))
                shiftY = 1;
            if (s.IsAboveTop(x, y))
                shiftY = -1;

            r += shiftX;
            z += shiftY;
            if (r < 0 || r >= N)
            {
                r -= shiftX;
                shiftX = 0;
            }
            if (z < 0 || z >= M)
            {
                z -= shiftY;
                shiftY = 0;
            }


            if (shiftX == 0 && shiftY == 0)
                return null;

            return FindSegment(x, y, r, z);
        }

        public double? GetValue(double x, double y)
        {
            if ((x < MinR || x > MaxR) || (y < MinZ || y > MaxZ))
                return null;

            int N = _segments.GetLength(0);
            int M = _segments.GetLength(1);

            double xp = 1 - (x - MinR) / (MaxR - MinR);
            double yp = 1 - (y - MinZ) / (MaxZ - MinZ);

            if (x == 0.6765 && y == 2.882326825)
                xp = xp;

            (int, int)? segment = FindSegment(x, y, (int)((N - 1) * xp), (int)((M - 1) * yp));
            if (segment == null)
                return null;
            return _segments[segment.Value.Item1, segment.Value.Item2].GetValue(x, y);
        }
    }
}
