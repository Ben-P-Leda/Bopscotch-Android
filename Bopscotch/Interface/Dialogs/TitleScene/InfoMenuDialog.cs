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
    public class InfoMenuDialog : ButtonDialog
    {
        public InfoMenuDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Top_Y_When_Active;

            AddButton("Rankings", new Vector2(Definitions.Left_Button_Column_X, 140), Button.ButtonIcon.Adventure, Color.DodgerBlue, 0.7f);
            AddButton("More Games", new Vector2(Definitions.Left_Button_Column_X, 260), Button.ButtonIcon.Website, Color.DodgerBlue, 0.7f);
            AddButton("About", new Vector2(Definitions.Right_Button_Column_X, 140), Button.ButtonIcon.Help, Color.DodgerBlue, 0.7f);
            AddButton("Rate Game", new Vector2(Definitions.Right_Button_Column_X, 260), Button.ButtonIcon.Rate, Color.DodgerBlue, 0.7f);

            AddButton("Back", new Vector2(Definitions.Back_Buffer_Center.X, 400.0f), Button.ButtonIcon.Back, Color.Red, 0.7f);

            _defaultButtonCaption = "Rankings";
            _cancelButtonCaption = "Back";

            _boxCaption = "Info";
        }

        private const int Dialog_Height = 480;
        private const float Top_Y_When_Active = 350.0f;
    }
}
