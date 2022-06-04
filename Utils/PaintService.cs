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

using TensionFields.API;

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

        public static ImageSource CreateImageFromFunction(Size imageSize, function func, Size funcS, Point funcP, double minValue, double maxValue,
            double? graphShift = null, bool? withStretch = null, double? colorExp = null, System.Drawing.Color? background = null)
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
                if (colorExp == null || colorExp == 1)
                    return relative;
                return Math.Sign(relative) * Math.Pow(Math.Sign(relative) * relative, colorExp.Value);
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

        public static List<Polygon> CreateSegments(Size imageSize, Size segmentsS, Point segmentsP, Segment[,] segments, double minValue, double maxValue,
            double? graphShift = null, bool? withStretch = null, double? colorExp = null)
        {
            Transform transform = new Transform(imageSize, segmentsS, segmentsP, graphShift, withStretch);

            List<Polygon> polygons = new List<Polygon>();
            for (int r = 0; r < segments.GetLength(0); r++)
                for (int z = 0; z < segments.GetLength(1); z++)
                    polygons.Add(new Polygon()
                    {
                        Points = new PointCollection() {
                        CreatePoint(segments[r, z].RightTop, transform),
                        CreatePoint(segments[r, z].LeftTop, transform),
                        CreatePoint(segments[r, z].LeftBottom, transform),
                        CreatePoint(segments[r, z].RightBottom, transform),},
                        Fill = CreateBrushByValue(minValue, maxValue, segments[r, z].GetValue() ?? 0, colorExp: colorExp),
                        Stroke = Brushes.Gray,
                        Visibility = Visibility.Hidden,
                        RenderTransform = new ScaleTransform(),
                    });
            return polygons;
        }

        private static Brush CreateBrushByValue(double minValue, double maxValue, double currentValue, double? colorExp = null)
        {
            double relativeValue = 2 * (currentValue - minValue) / (maxValue - minValue) - 1;
            relativeValue = Math.Sign(relativeValue) * Math.Pow(Math.Sign(relativeValue) * relativeValue, colorExp ?? 1);
            Color color;
            if (relativeValue < 0)
                color = Color.FromRgb(0, (byte)(255 * (1 + relativeValue)), (byte)(255 * -relativeValue));
            else
                color = Color.FromRgb((byte)(255 * relativeValue), (byte)(255 * (1 - relativeValue)), 0);
            return new SolidColorBrush(color);
        }

        private static Point CreatePoint(Point point, Transform? transform)
        {
            return new Point()
            {
                X = transform?.ReverseX(point.X) ?? point.X,
                Y = transform?.ReverseY(point.Y) ?? point.Y,
            };
        }
    }
}
