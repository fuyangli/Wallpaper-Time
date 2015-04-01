using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Xml.Serialization;
using WallpaperTime_.Annotations;
using WallpaperTime_.Utils;
using Timer = System.Timers.Timer;

namespace WallpaperTime_ {
    [Serializable]
    public class WallpaperTrigger : INotifyPropertyChanged {
        public WallpaperTrigger() {}

        [XmlIgnore]
        private FileInfo _fileInfo;

        [XmlIgnore]
        public FileInfo FileInfo {
            get { return _fileInfo; }
            set {
                _fileInfo = value;
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
            get { return _fileInfo == null ? "C:\\" : _fileInfo.FullName; }
            set {
                _fileInfo = new FileInfo(value);
                OnPropertyChanged();
                OnPropertyChanged("Name");
            }
        }

        public string Name {
            get {
                return _fileInfo == null ? "" : _fileInfo.Name;
            }
        }


        [XmlIgnore]
        private DateTime _time;


        [XmlIgnore]
        public DateTime Time {
            get { return _time; }
            set {
                _time = value;
                OnPropertyChanged();
                StopTimer();
                StartTimer();
                
            }
        }

        [XmlElement("Time")]
        public string TimeString {
            get { return Time.ToString("G"); }
            set { Time = DateTime.Parse(value); }
        }

        [XmlIgnore]
        private Timer _timer;

        public void StartTimer() {
            _timer = new Timer();

            var now = DateTime.Now;
            var t = new DateTime(now.Year, now.Month, now.Day, _time.Hour, _time.Minute, _time.Second);
            var ts = t - now;
            while (ts.TotalMilliseconds < 0) {
                t = t.AddDays(1);
                ts = t - now;
            }
            _timer.Interval = ts.TotalMilliseconds;
            _timer.Elapsed += (sender, args) => {
                SetWallpaper();
                StopTimer();
                StartTimer();
            };
            _timer.AutoReset = false;
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
            new Thread(() => Wallpaper.SetWithFade(_fileInfo, Style)).Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public WallpaperTrigger DeepClone() {
            var serialiser = new XmlSerializer(typeof(WallpaperTrigger));
            var writer = new StringWriter();
            serialiser.Serialize(writer, this);
            var reader = new StringReader(writer.ToString());
            var obj = serialiser.Deserialize(reader);
            return obj as WallpaperTrigger;
        }
    }
}