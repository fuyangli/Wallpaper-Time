using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Serialization;
using Hardcodet.Wpf.TaskbarNotification;
using WallpaperTime_;
using WallpaperTime_.Annotations;
using WallpaperTime_.Controls;
using Xceed.Wpf.DataGrid;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using DataFormats = System.Windows.DataFormats;
using DataGrid = System.Windows.Controls.DataGrid;
using DataGridCell = System.Windows.Controls.DataGridCell;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using TextBox = System.Windows.Controls.TextBox;

namespace WallpaperTime_ {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        private bool _canSave;

        public bool CanSave {
            get { return _canSave; }
            set {
                _canSave = value;
                OnPropertyChanged();
            }
        }

        public bool IsOnStart {
            get { return App.IsStartingWithWindows(); }
            set {
                App.RegisterInStartup(value);
                OnPropertyChanged();
            }
        }

        public OpenFileDialog FileDialog = new OpenFileDialog();
        public SaveFileDialog SaveDialog = new SaveFileDialog();

        private BindingList<WallpaperTrigger> _wallpaperTriggers;

        public BindingList<WallpaperTrigger> WallpaperTriggers {
            get { return _wallpaperTriggers; }
            set {
                _wallpaperTriggers = value;
                OnPropertyChanged();
            }
        }

        public string DataGridXmlPath = Path.Combine(Path.GetTempPath(), "wallpapertriggers.xml");


        public MainWindow() {
            InitializeComponent();
            WallpaperTriggers = new BindingList<WallpaperTrigger>();
            LoadData(DataGridXmlPath);
            DataContext = this;
        }


        private void ButtonUrlPicker(object sender, RoutedEventArgs e) {
            var button = sender as Button;
            if (button == null) {
                return;
            }
            var wall = (button.DataContext as WallpaperTrigger);
            var codecs = String.Join(";", ImageCodecInfo.GetImageEncoders().Select(c => c.FilenameExtension).ToList());
            FileDialog.Filter = String.Format("{0} ({1}) | {1}", Properties.Resources.ImageFiles, codecs);
            if (FileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) {
                return;
            }
            wall.Path = FileDialog.FileName;
            //CanSave = true;
        }

        private void ButtonNewRowClick(object sender, RoutedEventArgs e) {
            WallpaperTriggers.Add(new WallpaperTrigger());
            //CanSave = true;
        }

        private void ButtonRemoveRowClick(object sender, RoutedEventArgs e) {
            var button = sender as Button;
            if (button == null) {
                return;
            }
            var w = button.DataContext as WallpaperTrigger;
            if (w == null) {
                return;
            }
            w.StopTimer();
            WallpaperTriggers.Remove(w);
            //CanSave = true;
        }

        private void Save() {
            try {
                SaveDialog.Filter = String.Format("{0} ({1}) | {1}", Properties.Resources.XmlConfig, "*.xml");
                if (SaveDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) {
                    return;
                }
                var lockObject = new object();
                var thread = new Thread(() => {
                    lock (lockObject) {
                        var serialiser = new XmlSerializer(typeof (BindingList<WallpaperTrigger>));
                        var writer = new StreamWriter(DataGridXmlPath);
                        serialiser.Serialize(writer, DataGridConfig.ItemsSource);
                        writer.Close();
                        writer = new StreamWriter(SaveDialog.FileName);
                        serialiser.Serialize(writer, DataGridConfig.ItemsSource);
                        CanSave = false;
                    }
                });
                thread.Start();
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        private void LoadData(string path) {
            try {
                var serialiser = new XmlSerializer(typeof (BindingList<WallpaperTrigger>));
                var reader = new StreamReader(path);
                WallpaperTriggers = serialiser.Deserialize(reader) as BindingList<WallpaperTrigger>;
                if (WallpaperTriggers != null) {
                    WallpaperTriggers.ListChanged += (sender, args) => { CanSave = true; };
                }
                reader.Close();
                DataGridConfig.ItemsSource = null;
                DataGridConfig.ItemsSource = WallpaperTriggers;
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        private void ButtonSaveClick(object sender, RoutedEventArgs e) {
            Save();
        }

        private void ButtonSetWallpaperClick(object sender, RoutedEventArgs e) {
            var button = sender as Button;
            if (button == null) {
                return;
            }
            var w = button.DataContext as WallpaperTrigger;
            if (w != null) {
                w.SetWallpaper();
            }
        }

        private void OnTrayMouseDoubleClick(object sender, RoutedEventArgs e) {
            Show();
        }

        private void ExitMenuItemClick(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void OnClosing(object sender, CancelEventArgs e) {
            e.Cancel = true;
            Hide();
        }

        private void ButtonCopyClick(object sender, RoutedEventArgs e) {
            var button = sender as Button;
            if (button == null) {
                return;
            }
            var w = (button.DataContext as WallpaperTrigger);
            if (w == null) {
                return;
            }
            var newW = w.DeepClone();

            WallpaperTriggers.Insert(WallpaperTriggers.IndexOf(w), newW);
            CanSave = true;
        }

        private void ButtonLoadClick(object sender, RoutedEventArgs e) {
            if (FileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                LoadData(FileDialog.FileName);
                CanSave = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnTextBoxDrop(object sender, DragEventArgs e) {
            var s = (sender as MyTextBox);
            var data = e.Data.GetData(DataFormats.FileDrop) as String[];
            s.ShownValue = data[0];
        }

        private void TextBoxOnDragOver(object sender, DragEventArgs e) {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }
    }
}