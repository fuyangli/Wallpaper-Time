using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Serialization;
using WallpaperTime_.Annotations;
using WallpaperTime_.Utils;

namespace WallpaperTime_ {
    [Serializable]
    public class WallpaperTrigger : INotifyPropertyChanged {


        public WallpaperTrigger() {

        }

        [XmlIgnore]
        private Uri _url;

        [XmlIgnore]
        public Uri Url {
            get { return _url; }
            set {
                _url = value;
                OnPropertyChanged();
            }
        }

        [XmlElement("Path")]
        public string Path {
            get { return _url?.AbsolutePath ?? (_url = new Uri("C:\\")).AbsolutePath; }
            set {
                Url = new Uri(value);
                OnPropertyChanged();
                OnPropertyChanged("Name");
            }
        }

        [XmlIgnore]
        public string Name {
            get {
                
                return new string(new FileInfo(_url.AbsolutePath).Name.Where(c => (char.IsLetterOrDigit(c) ||
                             char.IsWhiteSpace(c) ||
                             c == '-' || c == ',' || c == '.')).ToArray());
            }
        }


        [XmlIgnore]
        private DateTime _time;



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
            _timer.Elapsed += (sender, args) => {
                Wallpaper.SetWithFade(Url, Wallpaper.Style.Stretched);
            };
            _timer.AutoReset = true;
            _timer.Start();
        }

        public void StopTimer() {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
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