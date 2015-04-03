using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Serialization;
using WallpaperTime_.Annotations;
using Application = System.Windows.Application;
using DataFormats = System.Windows.DataFormats;
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
            SetNearestWallpaper();
        }

        public void SetNearestWallpaper()
        {
            try
            {
                if (!WallpaperTriggers.Any()) return;
                //var lastItem = WallpaperTriggers.ToList().OrderBy(t => t.Time).TakeWhile(t => (t.Time - new DateTime(1, 1, 1, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second)).TotalMilliseconds < 0).LastOrDefault();
                var lastItem =
                    WallpaperTriggers.OrderBy(t => t.Time)
                        .LastOrDefault(
                            t =>
                                (t.Time - new DateTime(1, 1, 1, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second))
                                    .TotalMilliseconds < 0);
                lastItem?.SetWallpaper();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        private void ButtonNewRowClick(object sender, RoutedEventArgs e)
        {
            WallpaperTriggers.Add(new WallpaperTrigger());
            ScrollViewerTimers.ScrollToBottom();
        }

        private void ButtonRemoveRowClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as FrameworkElement;
                var w = button?.DataContext as WallpaperTrigger;
                w?.StopTimer();
                WallpaperTriggers.Remove(w);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
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
                SaveOnTemp();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SaveOnTemp()
        {
            try
            {
                var lockObject = new object();
                var thread = new Thread(() =>
                {
                    lock (lockObject)
                    {
                        var serialiser = new XmlSerializer(typeof (ObservableCollection<WallpaperTrigger>));
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
                if (!File.Exists(path)) return;
                var serialiser = new XmlSerializer(typeof (ObservableCollection<WallpaperTrigger>));
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
            var button = (sender as FrameworkElement);
            var w = button?.DataContext as WallpaperTrigger;
            w?.SetWallpaper();
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
            var button = sender as FrameworkElement;
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
            try
            {
                if (FileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                LoadData(FileDialog.FileName);
                SaveOnTemp();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            //CanSave = true;
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

        private void OnTileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var configWindow = new ConfigurationWindow((sender as FrameworkElement)?.DataContext as WallpaperTrigger);
                configWindow.ShowDialog();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void TileOnDrop(object sender, DragEventArgs e)
        {
            try
            {
                var item = (sender as FrameworkElement)?.DataContext as WallpaperTrigger;
                var data = e.Data.GetData(DataFormats.FileDrop) as String[];
                item.Path = data[0];
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void ButtonSetNearestWallpaper(object sender, RoutedEventArgs e)
        {
            try
            {
                SetNearestWallpaper();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}