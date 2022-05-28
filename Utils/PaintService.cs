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

namespace TensionFields.Utils
{
    public static class PaintService
    {
        public static double DPI = 96;

        public static ImageSource CreateImageFromFunction(Size imageSize, function func, Point funcP, Size funcS, double minValue, double maxValue, double? graphShift = null, double? colorExc = null, bool? withStretch = null, System.Drawing.Color? background = null)
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap(
                (int)imageSize.Width, (int)imageSize.Height,
                DPI, DPI,
                PixelFormats.Bgr32,
                null);

            double scaleX = funcS.Width * (1 + (2 * graphShift ?? 0)) / imageSize.Width;
            double scaleY = funcS.Height * (1 + (2 * graphShift ?? 0)) / imageSize.Height;

            if (withStretch == null || withStretch == false)
            {
                scaleX = Math.Max(scaleX, scaleY);
                scaleY = scaleX;
            }

            Func<double, double> transformX = (double x) => funcP.X - funcS.Width * (graphShift ?? 0) + x * scaleX;
            Func<double, double> transformY = (double y) => funcP.Y - funcS.Height * (graphShift ?? 0) + y * scaleY;
            Func<double, double> transformValue = (double value) =>
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
                        double? funcValue = func(transformX(x), transformY(y));
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
    }
}
