using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if WINDOWS_PHONE
using Microsoft.Phone.Net.NetworkInformation;
#endif
#if __ANDROID__
using Android.Net;
#endif

using Leda.Core;

using Bopscotch.Data;

namespace Bopscotch.Interface.Dialogs.TitleScene
{
    public class StartMenuDialog : ButtonDialog
    {
        private bool _networkIsAvailable;
        private bool _displayingStoreOption;
        private string _statusMessage;

        public StartMenuDialog()
            : base()
        {
            _defaultButtonCaption = "Adventure";
            _cancelButtonCaption = "Back";

            _boxCaption = "Select Game Mode";
            _networkIsAvailable = true;
            _displayingStoreOption = false;
        }

        public override void Activate()
        {
            _statusMessage = "";
#if WINDOWS_PHONE
            _networkIsAvailable = DeviceNetworkInformation.IsWiFiEnabled && DeviceNetworkInformation.IsNetworkAvailable;
#endif
#if __ANDROID__
			_networkIsAvailable = (MainActivity.NetManager.GetNetworkInfo(ConnectivityType.Wifi).GetState() == NetworkInfo.State.Connected);
#endif

            Profile.SyncPlayerLives();
            _displayingStoreOption = (Data.Profile.Lives < 1);

            Height = Dialog_Height;
            TopYWhenActive = Top_Y_When_Active;

            SetUpButtons();

            if (!_networkIsAvailable) { _statusMessage = Translator.Translation("no-wifi"); }

            base.Activate();
        }

        private void SetUpButtons()
        {
            ClearButtons();

            if (_displayingStoreOption)
            {
                AddButton("Add Lives", new Vector2(Definitions.Left_Button_Column_X, 200), Button.ButtonIcon.Store, Color.Orange);
            }
            else
            {
                AddButton("Adventure", new Vector2(Definitions.Left_Button_Column_X, 200), Button.ButtonIcon.Adventure, Color.LawnGreen);
            }

            AddButton("Race", new Vector2(Definitions.Right_Button_Column_X, 200), Button.ButtonIcon.Race, Color.LawnGreen);
            AddButton("Back", new Vector2(Definitions.Back_Buffer_Center.X, 320.0f), Button.ButtonIcon.Back, Color.Red, 0.7f);

            if (!_networkIsAvailable)
            {
                DisableButton("Race");
                _buttons["Race"].IconBackgroundTint = Color.Gray;
            }
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            _statusMessage = Translator.Translation("lives-count").Replace("[COUNT]", Profile.Lives.ToString());

            if (Data.Profile.NotAtFullLives)
            {
                CheckForLifeRestoration();

                TimeSpan remaining = Profile.NextLifeRestoreTime - DateTime.Now;

                _statusMessage += Translator.Translation("next-life-time")
                    .Replace("[MIN]", remaining.Minutes.ToString())
                    .Replace("[SEC]", (remaining.Seconds < 10 ? "0" : "") + remaining.Seconds.ToString());
            }

            if (!_networkIsAvailable) { _statusMessage += " " + Translator.Translation("no-wifi"); }

            base.Update(millisecondsSinceLastUpdate);
        }

        private void CheckForLifeRestoration()
        {
            if (Data.Profile.NextLifeRestoreTime < DateTime.Now)
            {
                Data.Profile.SyncPlayerLives();
                _displayingStoreOption = false;
                SetUpButtons();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!string.IsNullOrEmpty(_statusMessage))
            {
                TextWriter.Write(
                    Translator.Translation(_statusMessage), spriteBatch, new Vector2(Definitions.Back_Buffer_Center.X, 380.0f + WorldPosition.Y), 
                    Color.White, Color.Black, 3.0f, 0.7f, 0.1f, TextWriter.Alignment.Center);
            }

            base.Draw(spriteBatch);
        }

        private const int Dialog_Height = 480;
        private const float Top_Y_When_Active = 350.0f;
    }
}
