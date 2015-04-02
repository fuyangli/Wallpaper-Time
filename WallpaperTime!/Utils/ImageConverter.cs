using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WallpaperTime_.Utils
{
    public sealed class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            try
            {
                return new BitmapImage(new Uri((string)value));
            }
            catch
            {
                return new BitmapImage();
            }
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture) {
            return null;
        }
    }

    public sealed class TimeSpanToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            try {
                var time = (DateTime) value;
                return new TimeSpan(time.Hour, time.Minute, time.Second);
            }
            catch
            {
                return new TimeSpan();
            }
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            try
            {
                var time = (TimeSpan)value;
                return new DateTime(time.Hours, time.Minutes, time.Seconds);
            }
            catch
            {
                return DateTime.Now;
            }
        }
    }
}
