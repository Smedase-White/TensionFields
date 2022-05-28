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
        private CanvasDrawer _palette;

        private Field[] field;
        private Image[] fieldImages;
        private int _currentField;

        public MainWindow()
        {
            InitializeComponent();
            field = new Field[0];
            fieldImages = new Image[0];
            _currentField = 0;
            _drawer = new CanvasDrawer(FieldImage);
            _palette = new CanvasDrawer(Palette);
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                field = TextFileParser.Parse(openFileDialog.FileName);
            _currentField = 0;
            DrawField(field[_currentField]);
        }

        private void PrevTime(object sender, RoutedEventArgs e)
        {
            if (_currentField <= 0)
                return;
            _currentField--;
            DrawField(field[_currentField]);
        }


        private void NextTime(object sender, RoutedEventArgs e)
        {
            if (_currentField >= field.Length - 1)
                return;
            _currentField++;
            DrawField(field[_currentField]);
        }

        public void DrawField(Field field)
        {
            double maxShift = Math.Abs(field.MinSI);
            if (Math.Abs(field.MaxSI) > maxShift)
                maxShift = Math.Abs(field.MaxSI);
            _drawer.Draw(field.GetValue, field.MinR, field.MinZ, (field.MaxR - field.MinR) * 1.05, (field.MaxZ - field.MinZ) * 1.05, -maxShift, maxShift, true);
            _palette.Draw((double x, double y) => x * (field.MaxSI - field.MinSI), -1, 0, 2, 1, -maxShift, maxShift, false);
            PaletteMinLabel.Content = $"{-maxShift:0.0000}";
            PaletteMaxLabel.Content = $"{maxShift:0.0000}";
        }
    }
}