using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Serialization;
using Hardcodet.Wpf.TaskbarNotification;
using WallpaperTime_;
using Xceed.Wpf.DataGrid;
using Button = System.Windows.Controls.Button;
using DataGrid = System.Windows.Controls.DataGrid;
using DataGridCell = System.Windows.Controls.DataGridCell;
using TextBox = System.Windows.Controls.TextBox;

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
                wall.Path = FileDialog.FileName;
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
            var w = button.DataContext as WallpaperTrigger;
            w.StopTimer();
            WallpaperTriggers.Remove(w);
        }

        private void Save() {
            try {
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
            catch (Exception e) {
                Console.WriteLine(e);
                
            }
        }

        private void LoadData() {
            try {
                var serialiser = new XmlSerializer(typeof(ObservableCollection<WallpaperTrigger>));
                var reader = new StreamReader(DataGridXmlPath);
                WallpaperTriggers = serialiser.Deserialize(reader) as ObservableCollection<WallpaperTrigger>;
                reader.Close();
            }
            catch (Exception e) {
                Console.WriteLine(e);
                WallpaperTriggers = new ObservableCollection<WallpaperTrigger>();
            }
        }

        private void ButtonSaveClick(object sender, RoutedEventArgs e) {
            Save();
        }

        private void ButtonSetWallpaperClick(object sender, RoutedEventArgs e) {
            var button = sender as Button;
            if (button == null)
            {
                return;
            }
            var w = button.DataContext as WallpaperTrigger;
            w.SetWallpaper();
        }

        private void OnClosing(object sender, CancelEventArgs e) {

            e.Cancel = true;
            TaskbarIcon.ShowBalloonTip(Properties.Resources.ProgramTitle, Properties.Resources.Running, BalloonIcon.Warning);
            Visibility = Visibility.Hidden;
        }

        private void OnTrayMouseDoubleClick(object sender, RoutedEventArgs e) {
            
            Visibility = Visibility.Visible;
        }
    }
}