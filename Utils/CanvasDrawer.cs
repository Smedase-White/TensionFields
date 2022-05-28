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
    public class CanvasDrawer
    {
        private WriteableBitmap _writeableBitmap;
        private Image _img;

        public CanvasDrawer(Image image)
        {
            _img = image;
            RenderOptions.SetBitmapScalingMode(_img, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(_img, EdgeMode.Aliased);

            _writeableBitmap = new WriteableBitmap(
                (int)image.Width, (int)image.Height,
                96, 96,
                PixelFormats.Bgr32,
                null);

            _img.Source = _writeableBitmap;

            _img.Stretch = Stretch.None;
            _img.HorizontalAlignment = HorizontalAlignment.Left;
            _img.VerticalAlignment = VerticalAlignment.Top;
        }

        public void Draw(function func, double X, double Y, double Width, double Height, double minValue, double maxValue, bool autoScale)
        {

            Func<double, double> getX;
            Func<double, double> getY;
            if (autoScale)
            {
                double scale = Width / _img.Width;
                if (Height / _img.Height > scale)
                    scale = Height / _img.Height;
                getX = (double x) => X + x * scale;
                getY = (double y) => Y + y * scale;
            }
            else
            {
                getX = (double x) => X + x * Width / _img.Width;
                getY = (double y) => Y + y * Height / _img.Height;
            }
            double midValue = (maxValue + minValue) / 2;

            _writeableBitmap.Lock();
            unsafe
            {
                IntPtr pBackBuffer = _writeableBitmap.BackBuffer;
                for (int y = 0; y < _img.Height; y++)
                {
                    for (int x = 0; x < _img.Width; x++)
                    {
                        double? funcValue = func(getX(x), getY(y));
                        if (funcValue == null)
                        {
                            *(int*)pBackBuffer = 0;
                        }
                        else
                        {
                            int color_data;
                            double red = 0, green = 0, blue = 0;
                            /*double value = 255 / (1 + Math.Exp((-funcValue.Value) * 1000));
                            if (value < 128)
                            { green = 2 * value; blue = 255 - 2 * value; }
                            else
                            { red = 2 * (value - 128); green = 2 * (255 - value); }*/
                            double value = (minValue - funcValue.Value) / (minValue - maxValue);
                            if (value < 0.5)
                            { blue = 255 - 510 * value; green = 510 * value; }
                            else
                            { red = 510 * (value - 0.5); green = 510 * (1 - value); }
                            color_data = (int)red << 16;
                            color_data |= (int)green << 8;
                            color_data |= (int)blue << 0;
                            *(int*)pBackBuffer = color_data;
                        }
                        pBackBuffer += 4;
                    }
                }
            }
            _writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, (int)_img.Width, (int)_img.Height));
            _writeableBitmap.Unlock();
        }
    }
}