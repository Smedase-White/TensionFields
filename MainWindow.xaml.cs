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
using TensionFields.API;

namespace TensionFields
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Field[] _fields;

        private List<Polygon>[] _fieldsPolygons;

        private int _currentField;

        private (double, double)[] _valuesIntervals;

        public MainWindow()
        {
            InitializeComponent();
            _fields = Array.Empty<Field>();
            _currentField = 0;
            Palette.Source = PaintService.CreateImageFromFunction(
                new Size(Palette.Width, Palette.Height),
                (double x, double y) => x, 
                new Size(2, 1), new Point(-1, 0),
                -1, 1,
                withStretch: true);
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                _fields = TextFileParser.Parse(openFileDialog.FileName);

            FieldCanvas.Children.Clear();

            _fieldsPolygons = new List<Polygon>[_fields.Length];
            _valuesIntervals = new (double, double)[_fields.Length];

            if (SameCheckBox.IsChecked == true)
            {
                (double, double) sameInterval = (_fields[0].MinSI, _fields[0].MaxSI);
                for (int f = 1; f < _fields.Length; f++)
                    sameInterval = (Math.Min(sameInterval.Item1, _fields[f].MinSI), Math.Max(sameInterval.Item2, _fields[f].MaxSI));
                if (ColorCheckBox.IsChecked == true)
                {
                    double max = Math.Max(Math.Abs(sameInterval.Item1), Math.Abs(sameInterval.Item2));
                    sameInterval = (-max, max);
                }
                for (int f = 0; f < _fields.Length; f++)
                    _valuesIntervals[f] = sameInterval;
            }
            else
            {
                if (ColorCheckBox.IsChecked == true)
                {
                    for (int f = 0; f < _fields.Length; f++)
                    {
                        double max = Math.Max(Math.Abs(_fields[f].MinSI), Math.Abs(_fields[f].MaxSI));
                        _valuesIntervals[f] = (-max, max);
                    }
                }
                else
                {
                    for (int f = 0; f < _fields.Length; f++)
                    {
                        _valuesIntervals[f] = (_fields[f].MinSI, _fields[f].MaxSI);
                    }
                }
            }

            for (int f = 0; f < _fields.Length; f++)
            {
                Size funcS = new Size(_fields[f].MaxR - _fields[f].MinR, _fields[f].MaxZ - _fields[f].MinZ);

                _fieldsPolygons[f] = PaintService.CreateSegments(
                    new Size(FieldCanvas.Width, FieldCanvas.Height),
                    new Size(funcS.Width, funcS.Height), new Point(_fields[f].MinR, _fields[f].MinZ),
                    _fields[f].Segments,
                    _valuesIntervals[f].Item1, _valuesIntervals[f].Item2,
                    withStretch: StretchCheckBox.IsChecked
                    );

                foreach (Polygon polygon in _fieldsPolygons[f])
                    FieldCanvas.Children.Add(polygon);
            }
            ChangeField(-1, 0);
        }

        private void PrevTime(object sender, RoutedEventArgs e)
        {
            if (_currentField <= 0)
                return;
            ChangeField(_currentField, _currentField - 1);
        }


        private void NextTime(object sender, RoutedEventArgs e)
        {
            if (_currentField >= _fields.Length - 1)
                return;
            ChangeField(_currentField, _currentField + 1);
        }

        public void ChangeField(int oldF, int newF)
        {
            if (oldF != -1)
            {
                _currentField = oldF;
                HideCurrentField();
            }

            _currentField = newF;
            ShowCurrentField();
            if (GridCheckBox.IsChecked == true)
                ShowCurrentGrid();
            else
                HideCurrentGrid();

            PaletteMinLabel.Content = $"{_valuesIntervals[_currentField].Item1:0.0000}";
            PaletteMaxLabel.Content = $"{_valuesIntervals[_currentField].Item2:0.0000}";
        }

        private void ShowCurrentField()
        {
            if (_fieldsPolygons == null)
                return;
            foreach (Polygon polygon in _fieldsPolygons[_currentField])
                polygon.Visibility = Visibility.Visible;
        }

        private void HideCurrentField()
        {
            if (_fieldsPolygons == null)
                return;
            foreach (Polygon polygon in _fieldsPolygons[_currentField])
                polygon.Visibility = Visibility.Hidden;
        }

        private void ShowCurrentGrid()
        {
            if (_fieldsPolygons == null)
                return;
            foreach (Polygon polygon in _fieldsPolygons[_currentField])
                polygon.Stroke = Brushes.Gray;
        }

        private void HideCurrentGrid()
        {
            if (_fieldsPolygons == null)
                return;
            foreach (Polygon polygon in _fieldsPolygons[_currentField])
                polygon.Stroke = polygon.Fill;
        }

        private void GridCheck(object sender, RoutedEventArgs e)
        {
            if (GridCheckBox.IsChecked == true)
                ShowCurrentGrid();
            else
                HideCurrentGrid();
        }

        private void ChangeScale(object sender, RoutedEventArgs e)
        {
            double scale = Convert.ToDouble(Scale.Text);
            double posX = Convert.ToDouble(PosX.Text);
            double posY = Convert.ToDouble(PosY.Text);

            for (int i = 0; i < _fields.Length; i++)
            {
                foreach (Polygon polygon in _fieldsPolygons[i])
                {
                    ((ScaleTransform)polygon.RenderTransform).ScaleX = scale;
                    ((ScaleTransform)polygon.RenderTransform).ScaleY = scale;
                    ((ScaleTransform)polygon.RenderTransform).CenterX = posX * FieldCanvas.Width;
                    ((ScaleTransform)polygon.RenderTransform).CenterY = posY * FieldCanvas.Height;
                }
            }
        }
    }
}