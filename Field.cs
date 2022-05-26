using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TensionFields
{

    public class Field
    {
        private readonly Point[,] _vertices;
        private readonly double[,] _values;

        public double MaxR { get; private set; }
        public double MinR { get; private set; }
        public double MaxZ { get; private set; }
        public double MinZ { get; private set; }


        public Field(int N, int M, double[] R, double[] Z, double[] SI)
        {
            _vertices = new Point[N, M];
            _values = new double[N - 1, M - 1];

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

            for (int r = 0; r < N - 1; r++)
                for (int z = 0; z < M - 1; z++)
                    _values[r, z] = SI[r * (M - 1) + z];
        }

        public Field(double[,] R, double[,] Z, double[,] SI) :this(R.GetLength(0), R.GetLength(1), 
                 R.Cast<double>().ToArray(), Z.Cast<double>().ToArray(), SI.Cast<double>().ToArray())
        { }

        public Point[,] Vertices { get => _vertices; }
        public double[,] Values { get => _values; }

        private bool PointInSeegment(double x, double y, int r, int z)
        {
            Point[] v = new Point[4] { _vertices[r, z], _vertices[r + 1, z], _vertices[r + 1, z + 1], _vertices[r, z + 1] };
            return ((x >= v[0].X && x <= v[2].X && y >= v[0].Y && y <= v[2].Y) ||
                (x >= v[2].X && x <= v[0].X && y >= v[2].Y && y <= v[0].Y));
        }

        public double? GetValue(double x, double y)
        {
            int N = _values.GetLength(0);
            int M = _values.GetLength(1);

            for (int r = 0; r < N; r++)
                for (int z = 0; z < M; z++)
                    if (PointInSeegment(x, y, r, z))
                        return _values[r, z];
            return null;
        }
    }
}
