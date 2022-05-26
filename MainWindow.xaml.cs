using System;
using System.IO;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using TensionFields.Utils;

namespace TensionFields
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CanvasDrawer _drawer;

        private Field field;

        public MainWindow()
        {
            InitializeComponent();
            field = new Field(1, 1, new double[] { 0 }, new double[] { 0 }, new double[] { });
            _drawer = new CanvasDrawer(FieldImage);
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                field = TextFileParser.Parse(openFileDialog.FileName)[0];
            _drawer.Draw(field.GetValue, field.MinR, field.MinZ, field.MaxR - field.MinR, field.MaxZ - field.MinZ);
        }

        public void DrawField(Field field)
        {
            int N = field.Values.GetLength(0);
            int M = field.Values.GetLength(1);
            GeometryGroup polygons = new GeometryGroup();
            for (int r = 0; r < N; r++)
            {
                for (int z = 0; z < M; z++)
                {
                    PathFigure figure = new PathFigure() { IsClosed = true, StartPoint = field.Vertices[r, z] };
                    figure.Segments.Add(new LineSegment());
                    polygons.Children.Add(new PathGeometry(new PathFigure[] { new PathFigure()}));
                }
            }
        }
    }
}
