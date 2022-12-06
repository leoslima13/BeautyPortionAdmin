using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Helpers
{
    public static class Colors
    {
        private static readonly Lazy<ResourceDictionary> _colors = new Lazy<ResourceDictionary>(() =>
        {
            return Application.Current.Resources.MergedDictionaries.Single(md => md.Source.OriginalString.Equals("Styles/Colors.xaml"));
        });

        public static Color PrimaryColor => (Color)_colors.Value["Primary-Color"];
        public static Color PrimaryDarkColor => (Color)_colors.Value["Primary-Dark-Color"];
        public static Color PrimaryLightColor => (Color)_colors.Value["Primary-Light-Color"];
        public static Color PrimaryTextColor => (Color)_colors.Value["Primary-Text-Color"];
        public static Color SecondaryTextColor => (Color)_colors.Value["Secondary-Text-Color"];
        public static Color DividerColor => (Color)_colors.Value["Divider-Color"];
        public static Color TextColor => (Color)_colors.Value["Text-Color"];
        public static Color AccentColor => (Color)_colors.Value["Accent-Color"];
        public static Color ErrorColor => (Color)_colors.Value["Error-Color"];
    }
}
