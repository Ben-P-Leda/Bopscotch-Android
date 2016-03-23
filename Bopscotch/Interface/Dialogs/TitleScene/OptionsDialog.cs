using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Asset_Management;
using Leda.Core.Gamestate_Management;

using Leda.FacebookAdapter;

using Bopscotch.Facebook;

namespace Bopscotch.Interface.Dialogs.TitleScene
{
    public class OptionsDialog : ButtonDialog
    {
        public FacebookLoginManager FacebookLoginManager { private get; set; }

        public OptionsDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Top_Y_When_Active;

            AddButton("Back", new Vector2(Definitions.Left_Button_Column_X, 400.0f), Button.ButtonIcon.Back, Color.Red, 0.7f);
            AddButton("Reset Game", new Vector2(Definitions.Right_Button_Column_X, 400.0f), Button.ButtonIcon.Trash, Color.Yellow, 0.7f);

            AddIconButton("MusicSwitch", new Vector2(Definitions.Back_Buffer_Center.X - Margin, 145.0f), Button.ButtonIcon.Music, Color.LawnGreen, 0.6f);
            AddIconButton("SoundSwitch", new Vector2(Definitions.Back_Buffer_Width - Margin, 145.0f), Button.ButtonIcon.Sound, Color.LawnGreen, 0.6f);
            AddIconButton("HelperSwitch", new Vector2(Definitions.Back_Buffer_Center.X - Margin, 245.0f), Button.ButtonIcon.Help, Color.LawnGreen, 0.6f);
            AddIconButton("FacebookSwitch", new Vector2(Definitions.Back_Buffer_Width - Margin, 245.0f), Button.ButtonIcon.Facebook, Color.LawnGreen, 0.6f);

            _defaultButtonCaption = "Back";
            _cancelButtonCaption = "Back";
            _boxCaption = "Options";
        }

        public override void Activate()
        {
            FacebookLoginManager.ConnectToDialog(this, _buttons["FacebookSwitch"]);

            base.Activate();

            SetSwitchButtonStates();
            ActivateButton("Back");
        }

        private void SetSwitchButtonStates()
        {
            _buttons["MusicSwitch"].IconBackgroundTint = (MusicManager.Muted ? Color.Red : Color.LawnGreen);
            _buttons["SoundSwitch"].IconBackgroundTint = (SoundEffectManager.Muted ? Color.Red : Color.LawnGreen);
            _buttons["HelperSwitch"].IconBackgroundTint = (Data.Profile.Settings.ShowPowerUpHelpers ? Color.LawnGreen : Color.Red);
            _buttons["FacebookSwitch"].IconBackgroundTint = (Game1.FacebookAdapter.IsLoggedIn ? Color.LawnGreen : Color.Red);
        }

        protected override void CheckForAndHandleSelection(Input.InputProcessorBase inputSource)
        {
            if (inputSource.SelectionTriggered)
            {
                if (inputSource.SelectionLocation != Vector2.Zero) 
                { 
                    base.CheckForAndHandleSelection(inputSource); 
                }
                else
                {
                    if ((_activeButtonCaption == "Back") || (_activeButtonCaption == "Reset Game")) 
                    { 
                        Dismiss(); 
                    }
                    else 
                    { 
                        HandleToggle(_activeButtonCaption); 
                    }

                    SoundEffectManager.PlayEffect(ActivateSelectionSoundEffectName);
                }
            }
        }

        private void HandleToggle(string buttonCaption)
        {
            switch (buttonCaption)
            {
                case "MusicSwitch": MusicManager.Muted = !MusicManager.Muted; break;
                case "SoundSwitch": SoundEffectManager.Muted = !SoundEffectManager.Muted; break;
                case "HelperSwitch": Data.Profile.Settings.ShowPowerUpHelpers = !Data.Profile.Settings.ShowPowerUpHelpers; break;
                case "FacebookSwitch": FacebookLoginManager.SetFacebookConnectionState(); break;
            }

            Data.Profile.Save();
            SetSwitchButtonStates();
        }

        protected override bool HandleButtonTouch(string buttonCaption)
        {
            if ((buttonCaption == "Back") || (buttonCaption == "Reset Game"))
            {
                _activeButtonCaption = buttonCaption;
                Dismiss();
            }
            else
            {
                HandleToggle(buttonCaption);
            }

            return false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            TextWriter.Write("Music", spriteBatch, new Vector2(Margin, 95.0f + WorldPosition.Y), Color.White, Color.Black,
                3.0f, 0.7f, 0.1f, TextWriter.Alignment.Left);
            TextWriter.Write("Sound Effects", spriteBatch, new Vector2(Definitions.Back_Buffer_Center.X + Margin, 95.0f + WorldPosition.Y), Color.White, Color.Black,
                3.0f, 0.7f, 0.1f, TextWriter.Alignment.Left);

            TextWriter.Write("Race Power-up Help", spriteBatch, new Vector2(Margin, 205.0f + WorldPosition.Y), Color.White, Color.Black,
                3.0f, 0.7f, 0.1f, TextWriter.Alignment.Left);
            TextWriter.Write("Facebook Sharing", spriteBatch, new Vector2(Definitions.Back_Buffer_Center.X + Margin, 205.0f + WorldPosition.Y), Color.White, Color.Black,
                3.0f, 0.7f, 0.1f, TextWriter.Alignment.Left);

            base.Draw(spriteBatch);
        }

        private const int Dialog_Height = 480;
        private const float Top_Y_When_Active = 350.0f;
        private const float Margin = 115.0f;
    }
}
