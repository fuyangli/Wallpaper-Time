using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Xml.Serialization;
using NCrontab;
using WallpaperTime_.Annotations;
using WallpaperTime_.Utils;
using Timer = System.Timers.Timer;

namespace WallpaperTime_ {
    [Serializable]
    public class WallpaperTrigger : INotifyPropertyChanged
    {
        public WallpaperTrigger()
        {
            InstanceId = Guid.NewGuid();
            FileInfo = new FileInfo(Path);
            
        }

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
            get { return _fileInfo?.FullName ?? @"C:\"; }
            set {
                FileInfo = new FileInfo(value);
                OnPropertyChanged();
                OnPropertyChanged("Name");
            }
        }

        [XmlIgnore]
        public string Name => _fileInfo?.Name ?? "";


        //[XmlIgnore]
        //private DateTime _time;


        //public DateTime Time {
        //    get { return _time; }
        //    set {
        //        _time = value;
        //        OnPropertyChanged();

        //    }
        //}

        //[XmlElement("Time")]
        //public string TimeString {
        //    get { return Time.ToString("G"); }
        //    set { Time = DateTime.Parse(value); }
        //}

        private DateTime _lastTime;
        public DateTime LastTime
        {
            get { return _lastTime; }
            set
            {
                _lastTime = value;
                OnPropertyChanged();
                
            }
        }

        private DateTime _nextTime;
        public DateTime NextTime
        {
            get { return _nextTime; }
            set
            {
                _nextTime = value;
                OnPropertyChanged();
            }
        }

        private string _cronExpression;
        [XmlElement("CronExpression")]
        public string CronExpression
        {
            get { return _cronExpression; }
            set
            {
                _cronExpression = value; 
                OnPropertyChanged();
                StopTimer();
                StartTimer();
            }
        }


        [XmlIgnore]
        public Guid InstanceId { get; private set; }

        [XmlIgnore]
        private Timer _timer;

        

        public void StartTimer() {
            try
            {
                var cronSchedule = CrontabSchedule.Parse(CronExpression);

                _timer = new Timer();

                var now = DateTime.Now;
                NextTime = cronSchedule.GetNextOccurrence(now);
                var ts = NextTime - now;
                if (ts.TotalMilliseconds < 0)
                {
                    return;
                }
                
                _timer.Interval = ts.TotalMilliseconds;
                _timer.Elapsed += (sender, args) =>
                {
                    LastTime = NextTime;
                    App.WriteKey("LastDateItemSet", LastTime);
                    SetWallpaper();
                    StopTimer();
                    StartTimer();
                };
                _timer.AutoReset = false;
                _timer.Start();
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void StopTimer() {
            try
            {
                if (_timer == null) {
                    return;
                }
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SetWallpaper() {
            new Thread(() =>
            {
                if (_fileInfo == null || !_fileInfo.Exists) return;
                Wallpaper.SetWithFade(_fileInfo, Style);
            }).Start();
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
            try
            {
                var serialiser = new XmlSerializer(typeof(WallpaperTrigger));
                var writer = new StringWriter();
                serialiser.Serialize(writer, this);
                var reader = new StringReader(writer.ToString());
                var obj = serialiser.Deserialize(reader);
                return obj as WallpaperTrigger;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}