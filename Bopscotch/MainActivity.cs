using System;
using System.Threading;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Provider;
using Android.Net;

using Microsoft.Xna.Framework;

namespace Bopscotch
{
    [Activity(
        Label = "Bopscotch",
#if __ANDROID_11__
        HardwareAccelerated = false,
#endif
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize,
        MainLauncher = true,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape,
        Icon = "@drawable/Icon")]
    public class MainActivity : AndroidGameActivity
    {
        public static bool SwitchToBrowser;
        public static bool AwaitingKeyRequest;
        public static string KeyRequestResult;

        public static ConnectivityManager NetManager;

        private Game1 _game;
        private Bundle _bundle;
        private Thread _keyCheckThread = null;

        protected override void OnCreate(Bundle bundle)
        {
            _bundle = bundle;
            base.OnCreate(bundle);

            FrameLayout Layout = new FrameLayout(this);

            _game = new Game1();
            SetContentView(_game.Services.GetService<View>());

            Data.Profile.Settings.Identity = (Settings.Secure.GetString(this.ContentResolver, Settings.Secure.AndroidId));

            UrlProvider.PackageName = ApplicationContext.PackageName;

            SwitchToBrowser = false;
            AwaitingKeyRequest = false;
            KeyRequestResult = "";

            _game.Run();

            NetManager = (ConnectivityManager)GetSystemService(ConnectivityService);
        }

        protected override void OnPause()
        {
            _game.HandleActivityPauseEvent();

            Leda.Core.TextWriter.CleanDownForActivityPause();
            Bopscotch.Data.Profile.PlayingRaceMode = false;
            base.OnPause();
        }

        protected override void OnResume()
        {
            base.OnResume();

            SwitchToBrowser = false;

            _game.HandleActivityResumeEvent();
        }
    }
}


