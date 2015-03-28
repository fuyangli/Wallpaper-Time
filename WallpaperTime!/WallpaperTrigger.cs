using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Xml.Serialization;
using WallpaperTime_.Annotations;
using WallpaperTime_.Utils;

namespace WallpaperTime_ {
    [Serializable]
    public class WallpaperTrigger : INotifyPropertyChanged {
        public WallpaperTrigger() {}

        [XmlIgnore] private Uri _url;

        [XmlIgnore]
        public Uri Url {
            get { return _url; }
            set {
                _url = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        private Style _style;

        [XmlIgnore]
        public Style Style {
            get { return _style; }
            set {
                _style = value;
                OnPropertyChanged();
            }
        }

        [XmlElement("Style")]
        public string StyleString {
            get { return Style.ToString();  }
            set { Style = (Style)Enum.Parse(typeof (Style), value); }
        }

        [XmlElement("Path")]
        public string Path {
            get { return _url == null ? (_url = new Uri("C:\\")).AbsolutePath : _url.AbsolutePath; }
            set {
                Url = new Uri(value);
                OnPropertyChanged();
                OnPropertyChanged("Name");
            }
        }

        
        public string Name {
            get {
                return new string(new FileInfo(_url.AbsolutePath).Name.Where(c => (char.IsLetterOrDigit(c) ||
                                                                                   char.IsWhiteSpace(c) ||
                                                                                   c == '-' || c == ',' || c == '.'))
                    .ToArray());
            }
        }


        [XmlIgnore] private DateTime _time;


        [XmlIgnore]
        public DateTime Time {
            get { return _time; }
            set {
                _time = value;
                StopTimer();
                StartTimer();
                OnPropertyChanged();
            }
        }

        [XmlElement("Time")]
        public string TimeString {
            get { return Time.ToString("G"); }
            set { Time = DateTime.Parse(value); }
        }

        [XmlIgnore] private Timer _timer;

        public void StartTimer() {
            _timer = new Timer();

            var now = DateTime.Now;
            var t = new DateTime(now.Year, now.Month, now.Day, Time.Hour, Time.Minute, Time.Second);
            var ts = t - now;
            while (ts.TotalMilliseconds < 0) {
                t = t.AddDays(1);
                ts = t - now;
            }
            _timer.Interval = ts.TotalMilliseconds;
            _timer.Elapsed += (sender, args) => { SetWallpaper(); };
            _timer.AutoReset = true;
            _timer.Start();
        }

        public void StopTimer() {
            if (_timer == null) {
                return;
            }
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
        }

        public void SetWallpaper() {
            Wallpaper.SetWithFade(Url, Style);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}