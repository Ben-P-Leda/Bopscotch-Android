using Android.Net;

namespace Bopscotch
{
    public class UrlProvider
    {
        public static string PackageName { private get; set; }
        public static Uri MoreGamesUrl { get { return Uri.Parse("https://play.google.com/store/apps/developer?id=Leda+Entertainment"); } }
        public static Uri ReviewGameUrl { get { return Android.Net.Uri.Parse(string.Concat("market://details?id=", PackageName)); } }
    }
}