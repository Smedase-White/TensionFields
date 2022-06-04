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

        private readonly int _N, _M;

        private (int, int)? LastFoundSegment;

        public double MaxR { get; private set; }
        public double MinR { get; private set; }

        public double MaxZ { get; private set; }
        public double MinZ { get; private set; }

        public double MaxSI { get; private set; }
        public double MinSI { get; private set; }

        public Field(int N, int M, double[] R, double[] Z, double[] SI)
        {
            _N = N;
            _M = M;

            _vertices = new Point[N + 1, M + 1];
            _segments = new Segment[N, M];

            double tempR, tempZ;
            for (int r = 0; r < N + 1; r++)
                for (int z = 0; z < M + 1; z++)
                {
                    tempR = R[r * (M + 1) + z];
                    tempZ = Z[r * (M + 1) + z];
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
            for (int r = 0; r < N; r++)
                for (int z = 0; z < M; z++)
                {
                    tempSI = SI[r * M + z];
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
            if (r < 0 || r >= _N || z < 0 || z >= _M)
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
            if (r < 0 || r >= _N)
            {
                r -= shiftX;
                shiftX = 0;
            }
            if (z < 0 || z >= _M)
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

            double xp = 1 - (x - MinR) / (MaxR - MinR);
            double yp = 1 - (y - MinZ) / (MaxZ - MinZ);

            int r = (int)((_N - 1) * xp);
            int z = (int)((_M - 1) * yp);

            if (LastFoundSegment != null)
            {
                if (Math.Abs(r - LastFoundSegment?.Item1 ?? 0) > _N / 2)
                    LastFoundSegment = null;
                if (Math.Abs(z - LastFoundSegment?.Item2 ?? 0) > _M / 2)
                    LastFoundSegment = null;
            }

            (int, int)? segment = FindSegment(x, y, LastFoundSegment?.Item1 ?? r, LastFoundSegment?.Item2 ?? z);
            if (segment == null)
                return null;
            LastFoundSegment = segment;
            return _segments[segment.Value.Item1, segment.Value.Item2].GetValue(x, y);
        }
    }
}
