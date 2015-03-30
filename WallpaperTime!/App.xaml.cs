using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace WallpaperTime_ {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public static void RegisterInStartup(bool isChecked) {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (registryKey != null) {
                if (isChecked) {
                    registryKey.SetValue("ApplicationName", System.Reflection.Assembly.GetExecutingAssembly().Location);
                }
                else {
                    registryKey.DeleteValue("ApplicationName");
                }
            }
        }

        public static bool IsStartingWithWindows()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (registryKey != null)
            {
                return registryKey.GetValue("ApplicationName", null) != null;
            }
            return false;
        }
    }
}