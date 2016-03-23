using Microsoft.Xna.Framework;

using Leda.FacebookAdapter;

using Bopscotch.Interface.Dialogs;

namespace Bopscotch.Facebook
{
    public class FacebookLoginManager
    {
        private ButtonDialog _callingDialog;
        private Button _loginButton;

        public void ConnectToDialog(ButtonDialog callingDialog, Button loginButton)
        {
            _callingDialog = callingDialog;
            _loginButton = loginButton;
        }

        public void SetFacebookConnectionState()
        {
            if (!Game1.FacebookAdapter.ActionInProgress)
            {
                if (Game1.FacebookAdapter.IsLoggedIn)
                {
                    Game1.FacebookAdapter.AttemptLogout();
                    Data.Profile.FacebookToken = "";
                    Data.Profile.Save();
                    _loginButton.IconBackgroundTint = Color.Red;
                }
                else
                {
                    _loginButton.IconBackgroundTint = Color.Gray;
                    Game1.FacebookAdapter.ActionCallback = CompleteFacebookConnectionAttempt;
                    Game1.FacebookAdapter.AttemptLogin();
                }
            }
        }

        private void CompleteFacebookConnectionAttempt(ActionResult actionResult)
        {
            Game1.FacebookAdapter.ActionCallback = null;
            if ((actionResult == ActionResult.LoginSuccessful) || (actionResult == ActionResult.LoginAlreadyLoggedIn))
            {
                _loginButton.IconBackgroundTint = Color.LawnGreen;

                Data.Profile.FacebookToken = Game1.FacebookAdapter.AccessToken;
                Data.Profile.Save();

                HandleFirstLogin();
            }
            else
            {
                _loginButton.IconBackgroundTint = Color.Red;
            }
        }

        private void HandleFirstLogin()
        {
            if (!Data.Profile.AvatarCostumeUnlocked("Wizard"))
            {
                Data.Profile.UnlockCostume("Wizard");
                _callingDialog.DismissWithReturnValue("Facebook");
            }
        }
    }
}