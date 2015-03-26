using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml.Serialization;
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
        public static ObservableCollection<WallpaperTrigger> WallpaperTriggers { get; set; }
        public string DataGridXmlPath = Path.Combine(Path.GetTempPath(), "wallpapertriggers.xml");

        public MainWindow()
        {
            InitializeComponent();
            FileDialog = new OpenFileDialog();
            LoadData();
            DataGridConfig.ItemsSource = WallpaperTriggers;
            DataGridConfig.DataContext = WallpaperTriggers;
        }
        

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

        private void ButtonSetWallpaper2Click(object sender, RoutedEventArgs e) {
            var button = sender as Button;
            if (button == null)
            {
                return;
            }
            var wall = button.DataContext as WallpaperTrigger;
            Wallpaper.SetViaShLobj(new Uri(wall.Url), Wallpaper.Style.Stretched);
        }

        private void Save() {
            var lockObject = new object();
            var thread = new Thread(() => {
                lock (lockObject) {
                    var serialiser = new XmlSerializer(typeof(ObservableCollection<WallpaperTrigger>));
                    var writer = new StreamWriter(DataGridXmlPath);
                    serialiser.Serialize(writer, DataGridConfig.ItemsSource);
                    writer.Close();
                }
                
            });
            thread.Start();
        }

        private void LoadData() {
            var serialiser = new XmlSerializer(typeof(ObservableCollection<WallpaperTrigger>));
            var reader = new StreamReader(DataGridXmlPath);
            WallpaperTriggers = serialiser.Deserialize(reader) as ObservableCollection<WallpaperTrigger>;
            reader.Close();
        }

        private void ButtonSaveClick(object sender, RoutedEventArgs e) {
            Save();
        }

    }
}