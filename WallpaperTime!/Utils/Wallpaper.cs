using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace WallpaperTime_.Utils {
    public enum Style {
        Tiled = 0x1,
        Centered = 0x0,
        Stretched = 0x2,
        KeepAspect = 0x3,
        CropToFit = 0x4,
        Span = 0x5
    }

    public static class Wallpaper {
        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);


        //todo Creat hash for temp file and verify it
        public static Uri CreateWallpaperTemp(string path) {
            return CreateWallpaperTemp(new FileInfo(path));
        }

        public static Uri CreateWallpaperTemp(FileInfo path) {
            //var s = new WebClient().OpenRead();
            var lockObject = new object();
            var tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
            try {
                using (Image img = new Bitmap(path.FullName)) {
                    lock (lockObject) {
                        //var tempPath = Path.Combine(Path.GetTempPath(), String.Format("wallpaper{0}.bmp", Path.GetFileNameWithoutExtension(path.Name)));
                        
                        //if (!File.Exists(tempPath)) {
                        img.Save(tempPath, ImageFormat.Bmp);
                        //}
                        //var tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
                        //if (!File.Exists(tempPath)) {
                        //}
                        img.Dispose();
                        return new Uri(tempPath);
                    }
                }
            }
            catch (Exception e) {
                return new Uri(tempPath);
            }
        }

        private static byte[] ImageToByteArray(Image image) {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Bmp);
            return ms.ToArray();
        }

        private static string GetMd5Hash(byte[] buffer) {
            var md5Hasher = MD5.Create();

            byte[] data = md5Hasher.ComputeHash(buffer);

            var sBuilder = new StringBuilder();
            foreach (byte t in data) {
                sBuilder.Append(t.ToString("x2"));
            }
            return sBuilder.ToString();
        }


        public static void Set(String path, Style style) {
            var tempPath = CreateWallpaperTemp(path);
            if (tempPath == null) {
                return;
            }
            SetReg(style);

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                tempPath.AbsolutePath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

        public static void SetWithFade(FileInfo path, Style style) {
            try {
                WinAPI.EnableActiveDesktop();
                ThreadStart threadStarter = () => {
                    var activeDesktop = WinAPI.ActiveDesktopWrapper.GetActiveDesktop();
                    var s = new WinAPI.WALLPAPEROPT() {
                        dwStyle = (WinAPI.WallPaperStyle) ((int) style),
                        Size = Marshal.SizeOf(typeof (WinAPI.WALLPAPEROPT))
                    };
                    var res = activeDesktop.SetWallpaperOptions(s, 0);
                    res = activeDesktop.ApplyChanges(WinAPI.AD_Apply.ALL | WinAPI.AD_Apply.FORCE);
                    res = activeDesktop.SetWallpaper(CreateWallpaperTemp(path).AbsolutePath, 0);
                    res = activeDesktop.ApplyChanges(WinAPI.AD_Apply.ALL | WinAPI.AD_Apply.FORCE);
                    //SetReg(style);
                    Marshal.ReleaseComObject(activeDesktop);
                };
                var thread = new Thread(threadStarter);
                thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA (REQUIRED!!!!)
                thread.Start();
                thread.Join(2000);
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        private static void SetReg(Style style) {
            try {
                var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
                if (key == null) {
                    return;
                }
                if (style == Style.Stretched) {
                    key.SetValue(@"WallpaperStyle", 2.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }

                if (style == Style.Centered) {
                    key.SetValue(@"WallpaperStyle", 1.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }

                if (style == Style.Tiled) {
                    key.SetValue(@"WallpaperStyle", 1.ToString());
                    key.SetValue(@"TileWallpaper", 1.ToString());
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }
    }
}