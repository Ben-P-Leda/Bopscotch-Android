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

using Xamarin.InAppBilling;

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
        Icon = "@drawable/Icon",
        LaunchMode = Android.Content.PM.LaunchMode.SingleTop)]
    public class MainActivity : AndroidGameActivity
    {
        public static bool SwitchToBrowser;

        public static ConnectivityManager NetManager;
        public static InAppBillingServiceConnection BillingServiceConnection;

        private Game1 _game;
        private Bundle _bundle;

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

            _game.Run();

            NetManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            BillingServiceConnection = null;
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

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            Android.Util.Log.Debug("Leda", "Purchase op. done, sending result to handler...");
            try
            {
                BillingServiceConnection.BillingHandler.HandleActivityResult(requestCode, resultCode, data);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Debug("Leda", "Failed! with exception message: " + ex.Message);
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        protected override void OnDestroy()
        {
            if (BillingServiceConnection != null)
            {
                BillingServiceConnection.Disconnect();
            }

            base.OnDestroy();
        }
    }
}


