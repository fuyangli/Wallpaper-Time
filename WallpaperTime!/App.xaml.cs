using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Win32;

namespace WallpaperTime_ {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App  {
        public App() {
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof (FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

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

        public static bool IsStartingWithWindows() {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (registryKey != null) {
                return registryKey.GetValue("ApplicationName", null) != null;
            }
            return false;
        }
    }
}