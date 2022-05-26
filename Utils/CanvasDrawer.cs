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

        public void Draw(function func, double X, double Y, double Width, double Height)
        {
            var getX = (double x) => X + x * Width / _img.Width;
            var getY = (double y) => Y + y * Height / _img.Height;

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
                            double value = 255 / (1 + Math.Exp((-funcValue ?? 0) * 1000));
                            int color_data;
                            double red = 0, green = 0, blue = 0;
                            if (value < 128)
                            { green = 2 * value; blue = 255 - 2 * value; }
                            else
                            { red = 2 * (value - 128); green = 2 * (255 - value); }
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
