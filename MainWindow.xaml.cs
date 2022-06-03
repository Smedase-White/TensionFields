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
        private ImageSource[] _fieldImageSources;
        private List<Line>[] _grids; 
        private int _currentField;

        private (double, double)[] _valuesIntervals;

        public MainWindow()
        {
            InitializeComponent();
            _fields = Array.Empty<Field>();
            _fieldImageSources = Array.Empty<ImageSource>();
            _currentField = 0;
            Palette.Source = PaintService.CreateImageFromFunction(
                new Size(Palette.Width, Palette.Height),
                (double x, double y) => x, 
                new Size(2, 1), new Point(-1, 0),
                -1, 1,
                withStretch: true);
            try
            {
                FieldImage.Source = new BitmapImage(new Uri("C:\\Users\\watei\\Downloads\\smeshariki.jpeg"));
            }
            catch
            {
                
            }
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                _fields = TextFileParser.Parse(openFileDialog.FileName);

            if (GridCheckBox.IsChecked == true)
                HideGrid();


            _fieldImageSources = new ImageSource[_fields.Length];
            _grids = new List<Line>[_fields.Length];
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

            double scale = Convert.ToDouble(Scale.Text);
            double posX = Convert.ToDouble(PosX.Text);
            double posY = Convert.ToDouble(PosY.Text);

            for (int f = 0; f < _fields.Length; f++)
            {

                /*_fieldImageSources[f] = PaintService.CreateImageFromFunction(
                    new Size(FieldImage.Width, FieldImage.Height),
                    _fields[f].GetValue,
                    new Size(_fields[f].MaxR - _fields[f].MinR, _fields[f].MaxZ - _fields[f].MinZ), new Point(_fields[f].MinR, _fields[f].MinZ),
                    _valuesIntervals[f].Item1, _valuesIntervals[f].Item2,
                    withStretch: StretchCheckBox.IsChecked);*/

                Size funcS = new Size(_fields[f].MaxR - _fields[f].MinR, _fields[f].MaxZ - _fields[f].MinZ);

                _fieldImageSources[f] = PaintService.CreateImageFromFunction(
                    new Size(FieldImage.Width, FieldImage.Height),
                    _fields[f].GetValue,
                    new Size(funcS.Width * scale, funcS.Height * scale), new Point(funcS.Width * posX + _fields[f].MinR, funcS.Height *  posY + _fields[f].MinZ),
                    _valuesIntervals[f].Item1, _valuesIntervals[f].Item2,
                    withStretch: StretchCheckBox.IsChecked);

                _grids[f] = PaintService.CreateBorders(
                    new Size(FieldImage.Width, FieldImage.Height),
                    new Size(funcS.Width * scale, funcS.Height * scale), new Point(funcS.Width * posX + _fields[f].MinR, funcS.Height * posY + _fields[f].MinZ),
                    _fields[f].Vertices,
                    withStretch: StretchCheckBox.IsChecked);

                foreach (Line line in _grids[f])
                    FieldCanvas.Children.Add(line);
            }
            _currentField = 0;
            DrawField();
        }

        private void PrevTime(object sender, RoutedEventArgs e)
        {
            if (_currentField <= 0)
                return;
            if (GridCheckBox.IsChecked == true)
                HideGrid();
            _currentField--;
            DrawField();
        }


        private void NextTime(object sender, RoutedEventArgs e)
        {
            if (_currentField >= _fields.Length - 1)
                return;
            if (GridCheckBox.IsChecked == true)
                HideGrid();
            _currentField++;
            DrawField();
        }

        public void DrawField()
        {
            FieldImage.Source = _fieldImageSources[_currentField];

            PaletteMinLabel.Content = $"{_valuesIntervals[_currentField].Item1:0.0000}";
            PaletteMaxLabel.Content = $"{_valuesIntervals[_currentField].Item2:0.0000}";

            if (GridCheckBox.IsChecked == true)
                ShowGrid();

            //FieldCanvas.Width = 800;
            //FieldCanvas.Height = 580;
        }

        private void ShowGrid()
        {
            if (_grids == null)
                return;
            List<Line> lines = _grids[_currentField];
            foreach (Line line in lines)
                line.Visibility = Visibility.Visible;
        }

        private void HideGrid()
        {
            if (_grids == null)
                return;
            List<Line> lines = _grids[_currentField];
            foreach (Line line in lines)
                line.Visibility = Visibility.Hidden;
        }

        private void GridCheck(object sender, RoutedEventArgs e)
        {
            if (GridCheckBox.IsChecked == true)
                ShowGrid();
            else
                HideGrid();
        }
    }
}