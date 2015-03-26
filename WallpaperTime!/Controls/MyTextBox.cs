using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WallpaperTime_.Annotations;

namespace WallpaperTime_.Controls
{
    public class MyTextBox : TextBox, INotifyPropertyChanged {

        //public new string Text { get; private set; }

        public string ShownValue {
            get { return GetValue(ShownValueProperty).ToString(); }
            set {
                SetValue(ShownValueProperty, value);
                if (!IsFocused) {
                    Text = value;
                    return;
                }
                Text = HiddenValue;
            }
        }
        public static readonly DependencyProperty ShownValueProperty = DependencyProperty.Register("ShownValue",
            typeof(String), typeof(MyTextBox), new PropertyMetadata(string.Empty, OnShownValuePropertyChanged));


        public string HiddenValue {
            get { return GetValue(HiddenValueProperty).ToString(); }
            set
            {
                SetValue(HiddenValueProperty, value);
            }
        }
        public static readonly DependencyProperty HiddenValueProperty = DependencyProperty.Register("HiddenValue",
            typeof(String), typeof(MyTextBox), new PropertyMetadata(string.Empty, OnHiddenValuePropertyChanged));
        
        public MyTextBox() {
            MouseEnter += (sender, args) => {
                 Text = ShownValue;
            };
            MouseLeave += (sender, args) => {
                Text = HiddenValue;
            };
            Loaded += (sender, args) => {
                Text = ShownValue;
            };
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static void OnShownValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var textbox = d as MyTextBox;
            textbox.OnPropertyChanged("ShownValue");
            textbox.OnShownValuePropertyChanged(e);
        }

        private void OnShownValuePropertyChanged(DependencyPropertyChangedEventArgs e) {
            Text = ShownValue;
        }

        private static void OnHiddenValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textbox = d as MyTextBox;
            textbox.OnPropertyChanged("HiddenValue");
            textbox.OnHiddenValuePropertyChanged(e);
        }

        private void OnHiddenValuePropertyChanged(DependencyPropertyChangedEventArgs e) {
            Text = HiddenValue;
        }
    }
}
