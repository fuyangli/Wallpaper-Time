using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using WallpaperTime_.Utils;
using Xceed.Wpf.DataGrid;
using Button = System.Windows.Controls.Button;
using DataGrid = System.Windows.Controls.DataGrid;

namespace WallpaperTime_
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public OpenFileDialog FileDialog;

        public MainWindow()
        {
            InitializeComponent();
            FileDialog = new OpenFileDialog();
            WallpaperTriggers = new ObservableCollection<WallpaperTrigger>()
            {
                new WallpaperTrigger()
            };
            DataGridConfig.ItemsSource = WallpaperTriggers;
            DataGridConfig.DataContext = WallpaperTriggers;
        }

        public static ObservableCollection<WallpaperTrigger> WallpaperTriggers { get; set; }

        private void ButtonUrlPicker(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
            {
                return;
            }
            var wall = (button.DataContext as WallpaperTrigger);
            if (FileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                wall.Url = FileDialog.FileName;
            }
        }

        private void ButtonNewRowClick(object sender, RoutedEventArgs e)
        {
            WallpaperTriggers.Add(new WallpaperTrigger());
        }

        private void ButtonRemoveRowClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
            {
                return;
            }
            WallpaperTriggers.Remove(button.DataContext as WallpaperTrigger);
        }

        private void ButtonSetWallpaperClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
            {
                return;
            }
            var wall = button.DataContext as WallpaperTrigger;
            Wallpaper.Set(new Uri(wall.Url), Wallpaper.Style.Stretched);
        }
    }
}