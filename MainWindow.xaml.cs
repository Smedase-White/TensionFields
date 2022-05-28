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
        private Field[] _fields;
        private ImageSource[] _fieldImageSources;
        private int _currentField;

        public MainWindow()
        {
            InitializeComponent();
            _fields = Array.Empty<Field>();
            _fieldImageSources = Array.Empty<ImageSource>();
            _currentField = 0;
            Palette.Source = PaintService.CreateImageFromFunction(
                new Size(Palette.Width, Palette.Height),
                (double x, double y) => x, 
                new Point(-1, 0), new Size(2, 1),
                -1, 1,
                withStretch: true);
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                _fields = TextFileParser.Parse(openFileDialog.FileName);
            _fieldImageSources = new ImageSource[_fields.Length];
            for (int f = 0; f < _fields.Length; f++)
                _fieldImageSources[f] = PaintService.CreateImageFromFunction(
                    new Size(FieldImage.Width, FieldImage.Height),
                    _fields[f].GetValue, 
                    new Point(_fields[f].MinR, _fields[f].MinZ), new Size(_fields[f].MaxR - _fields[f].MinR, _fields[f].MaxZ - _fields[f].MinZ),
                    _fields[f].MinSI, _fields[f].MaxSI,
                    graphShift: 0.05);
            _currentField = 0;
            DrawField();
        }

        private void PrevTime(object sender, RoutedEventArgs e)
        {
            if (_currentField <= 0)
                return;
            _currentField--;
            DrawField();
        }


        private void NextTime(object sender, RoutedEventArgs e)
        {
            if (_currentField >= _fields.Length - 1)
                return;
            _currentField++;
            DrawField();
        }

        public void DrawField()
        {
            FieldImage.Source = _fieldImageSources[_currentField];
            PaletteMinLabel.Content = $"{_fields[_currentField].MinSI:0.0000}";
            PaletteMaxLabel.Content = $"{_fields[_currentField].MaxSI:0.0000}";
        }
    }
}