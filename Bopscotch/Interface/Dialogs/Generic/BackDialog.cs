using Microsoft.Xna.Framework;

using Android.Content;

namespace Bopscotch.Interface.Dialogs.Generic
{
    public class BackDialog : ButtonDialog
    {
        public BackDialog()
            : base()
        {
            Height = Constants.Dialog_Height;
            TopYWhenActive = Definitions.Back_Buffer_Height - (Constants.Dialog_Height + Constants.Bottom_Margin);
                
            _cancelButtonCaption = "Back";
        }

        public override void Activate()
        {
            ClearButtons();

            AddButton("Back", new Vector2(Definitions.Left_Button_Column_X, Constants.Button_Y), Button.ButtonIcon.Back, Color.Red, 0.6f);

            AddIconButton("Facebook", new Vector2(Definitions.Right_Button_Column_X + Social_Button_Spacing, Constants.Button_Y), Button.ButtonIcon.Facebook, Color.DodgerBlue, 0.6f);
            AddIconButton("Twitter", new Vector2(Definitions.Right_Button_Column_X, Constants.Button_Y), Button.ButtonIcon.Twitter, Color.DodgerBlue, 0.6f);
            AddIconButton("Leda", new Vector2(Definitions.Right_Button_Column_X - Social_Button_Spacing, Constants.Button_Y), Button.ButtonIcon.Website, Color.DodgerBlue, 0.6f);
            AddIconButton("Rate", new Vector2(Definitions.Right_Button_Column_X - (Social_Button_Spacing * 2), Constants.Button_Y), Button.ButtonIcon.Rate, Color.Orange, 0.6f);

            base.Activate();
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(0.0f, Definitions.Back_Buffer_Height);
        }

        protected override bool HandleButtonTouch(string buttonCaption)
        {
            string webUrl = "";

            switch (buttonCaption)
            {
                case "Facebook": webUrl = "http://www.facebook.com/ledaentertainment"; break;
                case "Twitter": webUrl = "http://www.twitter.com/ledaentertain"; break;
                case "Leda": webUrl = "http://www.ledaentertainment.com/games"; break;
                case "Rate": return RateGame();
                case "Back": return true;
            }

            if (!string.IsNullOrEmpty(webUrl))
            {
                MainActivity.SwitchToBrowser = true;
                Intent browser = new Intent(Intent.ActionView, Android.Net.Uri.Parse(webUrl));
                Bopscotch.Game1.Activity.StartActivity(browser);
            }

            return false;
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

        private const float Social_Button_Spacing = 125.0f;
    }
}
