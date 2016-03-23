using Microsoft.Xna.Framework;

using Android.Content;

using Leda.Core;

using Bopscotch.Facebook;

namespace Bopscotch.Interface.Dialogs.TitleScene
{
    public class MainMenuDialog : ButtonDialog
    {
        public FacebookLoginManager FacebookLoginManager { private get; set; }

        public MainMenuDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Top_Y_When_Active;

            AddButton("Info", new Vector2(Definitions.Left_Button_Column_X, 85), Button.ButtonIcon.Help, Color.DodgerBlue, 0.7f);
            AddButton("Options", new Vector2(Definitions.Left_Button_Column_X, 215), Button.ButtonIcon.Options, Color.DodgerBlue, 0.7f);
            AddButton("Character", new Vector2(Definitions.Right_Button_Column_X, 215), Button.ButtonIcon.Character, Color.DodgerBlue, 0.7f);
            AddButton("Start!", new Vector2(Definitions.Back_Buffer_Center.X, 360), Button.ButtonIcon.Play, Color.LawnGreen);
            AddButton("Quit", new Vector2(Definitions.Back_Buffer_Center.X, 1500), Button.ButtonIcon.None, Color.Transparent);
            AddButton("Store", new Vector2(Definitions.Right_Button_Column_X, 85), Button.ButtonIcon.Store, Color.Orange, 0.7f);

            AddIconButton("Facebook", new Vector2(Social_Button_Spacing * 2.0f, Social_Button_Y), Button.ButtonIcon.Facebook, Color.DodgerBlue, 0.6f);
            AddIconButton("Rate", new Vector2(Definitions.Back_Buffer_Width - (Social_Button_Spacing * 2.0f), Social_Button_Y), Button.ButtonIcon.Rate, Color.Orange, 0.6f);

            _defaultButtonCaption = "Start!";
            _cancelButtonCaption = "Quit";
        }

        public override void Activate()
        {
            _buttons["Facebook"].IconBackgroundTint = (Game1.FacebookAdapter.IsLoggedIn ? Color.LawnGreen : Color.Red);
            FacebookLoginManager.ConnectToDialog(this, _buttons["Facebook"]);

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

            switch (buttonCaption)
            {
                case "Facebook": FacebookLoginManager.SetFacebookConnectionState(); break;
                case "Rate": _activeButtonCaption = "Rate"; buttonShouldDismissDialog = true; break;
                default: buttonShouldDismissDialog = base.HandleButtonTouch(buttonCaption); break;
            }

            return buttonShouldDismissDialog;
        }

        private const int Dialog_Height = 480;
        private const float Top_Y_When_Active = 350.0f;

        private const float Social_Button_Y = 405.0f;
        private const float Social_Button_Spacing = 125.0f;
    }
}
