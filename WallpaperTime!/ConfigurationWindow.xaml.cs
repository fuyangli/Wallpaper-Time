using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using WallpaperTime_.Annotations;
using WallpaperTime_.Controls;
using Xceed.Wpf.Toolkit;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using TextBox = System.Windows.Controls.TextBox;

namespace WallpaperTime_
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : INotifyPropertyChanged
    {
        private WallpaperTrigger _wallpaperTrigger;
        private readonly OpenFileDialog _fileDialog = new OpenFileDialog();

        public WallpaperTrigger WallpaperTrigger {
            get { return _wallpaperTrigger; }
            set {
                _wallpaperTrigger = value;
                OnPropertyChanged();
            }
        }

        public ConfigurationWindow(WallpaperTrigger item) {
            try
            {
                InitializeComponent();
                WallpaperTrigger = item;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

        private void ButtonUrlPicker(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button == null)
                {
                    return;
                }
                var codecs = String.Join(";", ImageCodecInfo.GetImageEncoders().Select(c => c.FilenameExtension).ToList());
                _fileDialog.Filter = String.Format("{0} ({1}) | {1}", Properties.Resources.ImageFiles, codecs);
                if (_fileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
                TextBoxImagePath.Text = _fileDialog.FileName;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            //CanSave = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ButtonSaveOnClick(object sender, RoutedEventArgs e) {
            try
            {
                TextBoxImagePath.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                TimePicker.GetBindingExpression(TimePicker.ValueProperty)?.UpdateSource();
                ComboBoxStyle.GetBindingExpression(ComboBox.SelectedValueProperty)?.UpdateSource();
                Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void ButtonOpenImageClick(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo() {
                FileName = TextBoxImagePath.Text
            });
        }

        private void ButtonOpenContainingFolder(object sender, RoutedEventArgs e) {
            var path = Path.GetDirectoryName(TextBoxImagePath.Text);
            if (String.IsNullOrEmpty(path)) {
                return;
            }
            if (TextBoxImagePath.Text != null) {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = path
                });
            }
        }

        private void ImageOnDrop(object sender, DragEventArgs e) {
            var data = e.Data.GetData(DataFormats.FileDrop) as String[];
            TextBoxImagePath.Text = data[0];
        }
    }
}
