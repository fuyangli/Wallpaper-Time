using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using MahApps.Metro.Controls;
using WallpaperTime_.Annotations;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using DragEventArgs = System.Windows.DragEventArgs;

namespace WallpaperTime_
{
    /// <summary>
    /// Interaction logic for MetroWindow.xaml
    /// </summary>
    public partial class MetroWindow : INotifyPropertyChanged
    {
        private bool _canSave;

        public bool CanSave
        {
            get { return _canSave; }
            set
            {
                _canSave = value;
                OnPropertyChanged();
            }
        }

        public bool IsOnStart
        {
            get { return App.IsStartingWithWindows(); }
            set
            {
                App.RegisterInStartup(value);
                OnPropertyChanged();
            }
        }

        
        public SaveFileDialog SaveDialog = new SaveFileDialog();
        public OpenFileDialog FileDialog = new OpenFileDialog();

        private ObservableCollection<WallpaperTrigger> _wallpaperTriggers;

        public ObservableCollection<WallpaperTrigger> WallpaperTriggers
        {
            get { return _wallpaperTriggers; }
            set
            {
                _wallpaperTriggers = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView WallpaperTriggersView { get; set; }

        public string DataGridXmlPath = Path.Combine(Path.GetTempPath(), "wallpapertriggers.xml");
        public MetroWindow()
        {
            InitializeComponent();
            WallpaperTriggers = new ObservableCollection<WallpaperTrigger>();
            LoadData(DataGridXmlPath);
            DataContext = this;
        }

        

        private void ButtonNewRowClick(object sender, RoutedEventArgs e) {
            WallpaperTriggers.Add(new WallpaperTrigger());
            ScrollViewerTimers.ScrollToBottom();
        }

        private void ButtonRemoveRowClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
            {
                return;
            }
            var w = button.DataContext as WallpaperTrigger;
            if (w == null)
            {
                return;
            }
            w.StopTimer();
            WallpaperTriggers.Remove(w);
            //CanSave = true;
        }

        private void Save()
        {
            try
            {
                SaveDialog.Filter = String.Format("{0} ({1}) | {1}", Properties.Resources.XmlConfig, "*.xml");
                if (SaveDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
                var lockObject = new object();
                var thread = new Thread(() => {
                    lock (lockObject)
                    {
                        var serialiser = new XmlSerializer(typeof(BindingList<WallpaperTrigger>));
                        var writer = new StreamWriter(DataGridXmlPath);
                        serialiser.Serialize(writer, ItemsControl.ItemsSource);
                        writer.Close();
                        writer = new StreamWriter(SaveDialog.FileName);
                        serialiser.Serialize(writer, ItemsControl.ItemsSource);
                        CanSave = false;
                    }
                });
                thread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void LoadData(string path)
        {
            try
            {
                var serialiser = new XmlSerializer(typeof(ObservableCollection<WallpaperTrigger>));
                var reader = new StreamReader(path);
                WallpaperTriggers = serialiser.Deserialize(reader) as ObservableCollection<WallpaperTrigger>;
                if (WallpaperTriggers != null)
                {
                    WallpaperTriggers.CollectionChanged += (sender, args) => { CanSave = true; };
                }
                reader.Close();
                ItemsControl.ItemsSource = null;
                ItemsControl.ItemsSource = WallpaperTriggers;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ButtonSaveClick(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void ButtonSetWallpaperClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
            {
                return;
            }
            var w = button.DataContext as WallpaperTrigger;
            if (w != null)
            {
                w.SetWallpaper();
            }
        }

        private void OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Show();
        }

        private void ExitMenuItemClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void ButtonCopyClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
            {
                return;
            }
            var w = (button.DataContext as WallpaperTrigger);
            if (w == null)
            {
                return;
            }
            var newW = w.DeepClone();

            WallpaperTriggers.Insert(WallpaperTriggers.IndexOf(w), newW);
            CanSave = true;
        }

        private void ButtonLoadClick(object sender, RoutedEventArgs e)
        {
            if (FileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadData(FileDialog.FileName);
                CanSave = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnTileClick(object sender, RoutedEventArgs e) {
            var configWindow = new ConfigurationWindow((sender as Tile).DataContext as WallpaperTrigger);
            configWindow.ShowDialog();
        }

        private void TileOnDrop(object sender, DragEventArgs e) {
            var item = (sender as Tile).DataContext as WallpaperTrigger;
            var data = e.Data.GetData(System.Windows.DataFormats.FileDrop) as String[];
            item.Path = data[0];
        }
    }
}
