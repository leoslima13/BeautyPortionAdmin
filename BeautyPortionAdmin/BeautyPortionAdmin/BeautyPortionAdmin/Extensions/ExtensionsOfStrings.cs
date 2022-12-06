using System;
using System.Collections.Generic;
using System.Text;

namespace BeautyPortionAdmin.Extensions
{
    public static class ExtensionsOfStrings
    {
        public static string AsNavigation(this string pageName)
        {
            return $"NavigationPage/{pageName}";
        }

        public static Uri AsNavigationAbsolute(this string pageName)
        {
            return new Uri($"http://beautyportionadmin.com/NavigationPage/{pageName}", UriKind.Absolute);
        }
    }
}
