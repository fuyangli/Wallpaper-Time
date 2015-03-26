using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;

namespace WallpaperTime_.Utils
{
    public static class Wallpaper
    {
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int
        {
            Tiled,
            Centered,
            Stretched
        }

        public static void Set(Uri uri, Style style)
        {
            var s = new WebClient().OpenRead(uri.ToString());

            var img = Image.FromStream(s);
            var tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
            img.Save(tempPath, ImageFormat.Bmp);

            SetReg(style);

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                tempPath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

        public static void SetWithFade(Uri uri, Style style) {

            WinAPI.EnableActiveDesktop();
            ThreadStart threadStarter = () =>
            {
                var activeDesktop = WinAPI.ActiveDesktopWrapper.GetActiveDesktop();
                SetReg(style);
                activeDesktop.SetWallpaper(uri.AbsolutePath, 0);
                activeDesktop.ApplyChanges(WinAPI.AD_Apply.ALL | WinAPI.AD_Apply.FORCE);
                
                Marshal.ReleaseComObject(activeDesktop);
            };
            var thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA (REQUIRED!!!!)
            thread.Start();
            thread.Join(2000);
        }

        private static void SetReg(Style style) {
            var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

            if (style == Style.Stretched)
            {
                key?.SetValue(@"WallpaperStyle", 2.ToString());
                key?.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Centered)
            {
                key?.SetValue(@"WallpaperStyle", 1.ToString());
                key?.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Tiled)
            {
                key?.SetValue(@"WallpaperStyle", 1.ToString());
                key?.SetValue(@"TileWallpaper", 1.ToString());
            }
        }
    }
}
