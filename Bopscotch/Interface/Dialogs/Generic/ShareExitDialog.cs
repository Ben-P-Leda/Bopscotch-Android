using System;

using Microsoft.Xna.Framework;

using Leda.Core.Gamestate_Management;

namespace Bopscotch.Interface.Dialogs.Generic
{
    public class ShareExitDialog : ButtonDialog
    {
        public delegate void ShareRewardCallback();

        private bool _shareOptionEnabled;

        public virtual bool ShareOptionEnabled 
        { 
            protected get { return ((_shareOptionEnabled) && (Game1.FacebookAdapter.IsLoggedIn)); }
            set { _shareOptionEnabled = value; }
        }

        public string ShareDialogText { private get; set; }
        public string ShareDialogDefaultValue { private get; set; }
        public string SharePostText { private get; set; }
        public ShareRewardCallback AwardShareReward { private get; set; }

        public ShareExitDialog()
            : this(Constants.Button_Y) { }

        public ShareExitDialog(float buttonY)
            : base()
        {
            Height = Constants.Dialog_Height;
            TopYWhenActive = Definitions.Back_Buffer_Height - (Constants.Dialog_Height + Constants.Bottom_Margin);

            AddButton("Share", new Vector2(Definitions.Left_Button_Column_X, buttonY), Button.ButtonIcon.Facebook, Color.Orange, 0.7f);
            AddButton("Exit", new Vector2(Definitions.Right_Button_Column_X, buttonY), Button.ButtonIcon.Play, Color.Red, 0.7f);

            _defaultButtonCaption = "Exit";
            _cancelButtonCaption = "Exit";

            _shareOptionEnabled = true;

            Height = Constants.Dialog_Height;
            TopYWhenActive = Definitions.Back_Buffer_Height - (Constants.Dialog_Height + Constants.Bottom_Margin);
        }

        public override void Activate()
        {
            if (ShareOptionEnabled)
            {
                _buttons["Share"].Disabled = false;
                _buttons["Share"].IconBackgroundTint = Color.Orange;
                ActivateButton("Share");
            }
            else
            {
                _buttons["Share"].Disabled = true;
                _buttons["Share"].IconBackgroundTint = Color.Gray;
                ActivateButton("Exit");
            }

            base.Activate();
        }

        protected override void Dismiss()
        {
            if (_activeButtonCaption == "Share")
            {
                Bopscotch.Interface.KeyboardHelper.BeginShowKeyboardInput(
                    Translator.Translation("Share on Facebook"),
                    Translator.Translation(ShareDialogText),
                    Translator.Translation(ShareDialogDefaultValue),
                    ShareResult);
            }
            else
            {
                base.Dismiss();
            }
        }

        private void ShareResult(IAsyncResult result)
        {
            string message = Bopscotch.Interface.KeyboardHelper.EndShowKeyboardInput(result);
            if (!string.IsNullOrWhiteSpace(message))
            {
                if (AwardShareReward != null)
                {
                    AwardShareReward();
                }

                Game1.FacebookAdapter.Caption = "www.ledaentertainment.com";
                Game1.FacebookAdapter.Description = SharePostText;
                Game1.FacebookAdapter.AttemptPost(message);

                DismissWithReturnValue("Exit");
            }

            AwardShareReward = null;
        }

        private const int Delay_Before_Result_Announcement_In_Milliseconds = 3000;
    }
}
