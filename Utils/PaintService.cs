using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TensionFields.Utils
{
    public static class PaintService
    {
        public static double DPI = 96;

        public delegate double trasnformFunction(double value);

        public struct Transform
        {
            public readonly trasnformFunction X;
            public readonly trasnformFunction Y;

            public readonly trasnformFunction ReverseX;
            public readonly trasnformFunction ReverseY;

            public Transform(Size imageSize, Size sorceSize, Point minSource, double? graphShift = null, bool? withStretch = null)
            {
                double scaleX = sorceSize.Width * (1 + (2 * graphShift ?? 0)) / imageSize.Width;
                double scaleY = sorceSize.Height * (1 + (2 * graphShift ?? 0)) / imageSize.Height;

                if (withStretch == null || withStretch == false)
                {
                    scaleX = Math.Max(scaleX, scaleY);
                    scaleY = scaleX;
                }

                X = (double x) => minSource.X - sorceSize.Width * (graphShift ?? 0) + x * scaleX;
                Y = (double y) => minSource.Y - sorceSize.Height * (graphShift ?? 0) + y * scaleY;

                ReverseX = (double x) => (x - minSource.X + sorceSize.Width * (graphShift ?? 0)) / scaleX;
                ReverseY = (double y) => (y - minSource.Y + sorceSize.Height * (graphShift ?? 0)) / scaleY;
            }
        }

        public static ImageSource CreateImageFromFunction(Size imageSize, function func, Size funcS, Point funcP, double minValue, double maxValue, double? graphShift = null, double? colorExc = null, bool? withStretch = null, System.Drawing.Color? background = null)
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap(
                (int)imageSize.Width, (int)imageSize.Height,
                DPI, DPI,
                PixelFormats.Bgr32,
                null);

            Transform transform = new Transform(imageSize, funcS, funcP, graphShift, withStretch);

            trasnformFunction transformValue = (double value) =>
            {
                double relative = 2 * (value - minValue) / (maxValue - minValue) - 1;
                if (colorExc == null || colorExc == 1)
                    return relative;
                return Math.Sign(relative) * Math.Pow(Math.Sign(relative) * relative, colorExc.Value);
            };

            int backgroundNum = (background?.R << 16 | background?.G << 8 | background?.B << 0) ?? int.MaxValue;

            writeableBitmap.Lock();
            unsafe
            {
                IntPtr pBackBuffer = writeableBitmap.BackBuffer;
                for (int y = 0; y < imageSize.Height; y++)
                {
                    for (int x = 0; x < imageSize.Width; x++)
                    {
                        double? funcValue = func(transform.X(x), transform.Y(y));
                        if (funcValue == null)
                        {
                            *(int*)pBackBuffer = backgroundNum;
                        }
                        else
                        {
                            double value = transformValue(funcValue.Value);
                            int colorData;
                            if (value < 0)
                                colorData = (int)(255 * -value) << 0 | (int)(255 * (1 + value)) << 8;
                            else
                                colorData = (int)(255 * (1 - value)) << 8 | (int)(255 * value) << 16;
                            *(int*)pBackBuffer = colorData;
                        }
                        pBackBuffer += 4;
                    }
                }
            }

            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, (int)imageSize.Width, (int)imageSize.Height));
            writeableBitmap.Unlock();

            return writeableBitmap;
        }

        public static List<Line> CreateBorders(Size imageSize, Size pointsS, Point pointsP, Point[,] points, double? graphShift = null, bool? withStretch = null)
        {
            Transform transform = new Transform(imageSize, pointsS, pointsP, graphShift, withStretch);

            List<Line> lines = new List<Line>();
            for (int r = 0; r < points.GetLength(0) - 1; r++)
                for (int z = 0; z < points.GetLength(1) - 1; z++)
                    lines.AddRange(new Line[] {
                            CreateLine(points[r, z], points[r + 1, z], transform),
                            CreateLine(points[r + 1, z], points[r + 1, z + 1], transform),
                            CreateLine(points[r + 1, z + 1], points[r, z + 1], transform),
                            CreateLine(points[r, z + 1], points[r, z], transform),
                        });
            return lines;
        }

        private static Line CreateLine(Point begin, Point end, Transform? transform)
        {
            return new Line()
            {
                Stroke = Brushes.Gray,
                X1 = transform?.ReverseX(begin.X) ?? begin.X,
                Y1 = transform?.ReverseY(begin.Y) ?? begin.Y,
                X2 = transform?.ReverseX(end.X) ?? end.X,
                Y2 = transform?.ReverseY(end.Y) ?? end.Y,
                Visibility = Visibility.Hidden,
            };
        }
    }
}
