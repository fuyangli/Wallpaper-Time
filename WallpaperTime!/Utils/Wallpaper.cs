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

    public enum Style 
    {
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
        public static Uri CreateWallpaperTemp(Uri uri) {
            //var s = new WebClient().OpenRead();
            var lockObject = new object();
            Image img = null;
            try {
                lock (lockObject) {
                    img = new Bitmap(uri.AbsolutePath);
                    //var hash = GetMd5Hash(ImageToByteArray(img));
                    //var tempPath = Path.Combine(Path.GetTempPath(), String.Format("wallpaper{0}.bmp", hash));
                    var tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
                    //if (!File.Exists(tempPath)) {
                        img.Save(tempPath, ImageFormat.Bmp);
                    //}
                    img.Dispose();
                    return new Uri(tempPath);
                }
            }
            catch (Exception) {
                if (img != null) {
                    img.Dispose();
                }
                return null;
            }
        }

        static byte[] ImageToByteArray(Image image)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Bmp);
            return ms.ToArray();
        }

        static string GetMd5Hash(byte[] buffer)
        {
            var md5Hasher = MD5.Create();

            byte[] data = md5Hasher.ComputeHash(buffer);

            var sBuilder = new StringBuilder();
            foreach (byte t in data) {
                sBuilder.Append(t.ToString("x2"));
            }
            return sBuilder.ToString();
        }


        public static void Set(Uri uri, Style style) {
            var tempPath = CreateWallpaperTemp(uri);
            if (tempPath == null) {
                return;
            }
            SetReg(style);

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                tempPath.AbsolutePath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

        public static void SetWithFade(Uri uri, Style style) {
            WinAPI.EnableActiveDesktop();
            ThreadStart threadStarter = () => {
                var activeDesktop = WinAPI.ActiveDesktopWrapper.GetActiveDesktop();
                var s = new WinAPI.WALLPAPEROPT() {dwStyle = (WinAPI.WallPaperStyle) ((int) style), Size = Marshal.SizeOf(typeof(WinAPI.WALLPAPEROPT))};
                var res = activeDesktop.SetWallpaperOptions(s, 0);
                res = activeDesktop.ApplyChanges(WinAPI.AD_Apply.ALL | WinAPI.AD_Apply.FORCE);
                res = activeDesktop.SetWallpaper(CreateWallpaperTemp(uri).AbsolutePath, 0);
                res = activeDesktop.ApplyChanges(WinAPI.AD_Apply.ALL | WinAPI.AD_Apply.FORCE);
                //SetReg(style);
                Marshal.ReleaseComObject(activeDesktop);
            };
            var thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA (REQUIRED!!!!)
            thread.Start();
            thread.Join(2000);
        }

        private static void SetReg(Style style) {
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
    }
}