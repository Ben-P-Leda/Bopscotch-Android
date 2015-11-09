using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;

using Android.Content;

using Leda.Core;

namespace Bopscotch.Interface.Dialogs.TitleScene
{
    public class MainMenuDialog : ButtonDialog
    {
        public MainMenuDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Top_Y_When_Active;

            _defaultButtonCaption = "Start!";
            _cancelButtonCaption = "Quit";
        }

        public override void Activate()
        {
            ClearButtons();

            AddButton("About", new Vector2(Definitions.Left_Button_Column_X, 85), Button.ButtonIcon.Help, Color.DodgerBlue, 0.7f);
            AddButton("Options", new Vector2(Definitions.Left_Button_Column_X, 215), Button.ButtonIcon.Options, Color.DodgerBlue, 0.7f);
            AddButton("Character", new Vector2(Definitions.Right_Button_Column_X, 215), Button.ButtonIcon.Character, Color.DodgerBlue, 0.7f);
            AddButton("Start!", new Vector2(Definitions.Back_Buffer_Center.X, 360), Button.ButtonIcon.Play, Color.LawnGreen);
            AddButton("Quit", new Vector2(Definitions.Back_Buffer_Center.X, 1500), Button.ButtonIcon.None, Color.Transparent);
            AddButton("Store", new Vector2(Definitions.Right_Button_Column_X, 85), Button.ButtonIcon.Store, Color.Orange, 0.7f);

            AddIconButton("Facebook", new Vector2(Social_Button_Spacing * 2.0f, Social_Button_Y), Button.ButtonIcon.Facebook, Color.DodgerBlue, 0.6f);
            AddIconButton("Twitter", new Vector2(Social_Button_Spacing * 3.0f, Social_Button_Y), Button.ButtonIcon.Twitter, Color.DodgerBlue, 0.6f);
            AddIconButton("Leda", new Vector2(Definitions.Back_Buffer_Width - (Social_Button_Spacing * 3.0f), Social_Button_Y), Button.ButtonIcon.Website, Color.DodgerBlue, 0.6f);
            AddIconButton("Rate", new Vector2(Definitions.Back_Buffer_Width - (Social_Button_Spacing * 2.0f), Social_Button_Y), Button.ButtonIcon.Rate, Color.Orange, 0.6f);

            base.Activate();
        }

        protected override void ActivateButton(string caption)
        {
            if ((caption == "Start!") && (_activeButtonCaption != "Start!") && (_activeButtonCaption != null))
            {
                SetMovementLinksForButton("Start!", _activeButtonCaption, "", "Options", "Character");
            }

            base.ActivateButton(caption);
        }

        protected override bool HandleButtonTouch(string buttonCaption)
        {
            bool buttonShouldDismissDialog = false;
            string webUrl = "";

            switch (buttonCaption)
            {
                case "Facebook": webUrl = "http://www.facebook.com/ledaentertainment"; break;
                case "Twitter": webUrl = "http://www.twitter.com/ledaentertain"; break;
                case "Leda": webUrl = "http://www.ledaentertainment.com/games"; break;
                case "Rate": buttonShouldDismissDialog = RateGame(); break;
                case "Full Game": _activeButtonCaption = buttonCaption; buttonShouldDismissDialog = true; break;
                default: buttonShouldDismissDialog = base.HandleButtonTouch(buttonCaption); break;
            }

            if (!string.IsNullOrEmpty(webUrl))
            {
                MainActivity.SwitchToBrowser = true;
                Intent browser = new Intent(Intent.ActionView, Android.Net.Uri.Parse(webUrl));
                Bopscotch.Game1.Activity.StartActivity(browser);
            }

            return buttonShouldDismissDialog;
        }

        private bool RateGame()
        {
            MainActivity.SwitchToBrowser = true;
            Intent market = new Intent(Intent.ActionView, UrlProvider.ReviewGameUrl);
            Bopscotch.Game1.Activity.StartActivity(market);

            Data.Profile.FlagAsRated();

            if (!Data.Profile.AvatarCostumeUnlocked("Angel")) 
            { 
                Data.Profile.UnlockCostume("Angel");
                _activeButtonCaption = "Rate"; 
                return true; 
            }

            return false;
        }

        private const int Dialog_Height = 480;
        private const float Top_Y_When_Active = 350.0f;

        private const float Social_Button_Y = 405.0f;
        private const float Social_Button_Spacing = 125.0f;
    }
}
