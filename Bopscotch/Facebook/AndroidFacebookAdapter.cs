using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Webkit;

using Facebook;

using Microsoft.Xna.Framework;

namespace Leda.FacebookAdapter
{
    public class AndroidFacebookAdapter : FacebookAdapterBase, IFacebookAdapter
    {
		public AndroidGameActivity Activity { private get; set; }

        public AndroidFacebookAdapter()
			: base()
        {
        }

		public void AttemptLogin()
		{
			var webAuth = new Intent(Activity, typeof(FBWebViewAuthActivity));
			webAuth.PutExtra("AppId", ApplicationId);
			webAuth.PutExtra("ExtendedPermissions", "user_about_me,publish_actions");
			Activity.StartActivityForResult(webAuth, 0);
		}

		public void HandleAuthorizationActivityClose(Result activityResult, Intent data)
		{
			if (activityResult == Result.Ok) { FinishLogin(data.GetStringExtra("AccessToken"), data.GetBooleanExtra("ManualLoginRequired", true)); }
			else { CompleteAction(ActionResult.LoginCancelled); }

			GC.Collect();
		}

        public override void AttemptLogout()
        {
			CookieSyncManager.CreateInstance(Activity);
			CookieManager.Instance.RemoveAllCookie();

			base.AttemptLogout();
        }
    }
}

