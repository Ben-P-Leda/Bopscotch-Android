using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;

namespace Bopscotch.Interface.Dialogs
{
    public class DisconnectedDialog : ButtonDialog
    {
        public DisconnectedDialog()
            : this(Default_Caption) { }

        public DisconnectedDialog(string caption)
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Definitions.Back_Buffer_Center.Y - (Dialog_Height / 2.0f);

            AddButton("OK", new Vector2(Definitions.Back_Buffer_Center.X, 200), Button.ButtonIcon.Back, Color.DodgerBlue);

            _cancelButtonCaption = "OK";

            _boxCaption = Translator.Translation(caption);
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(0.0f, -Height);
        }

        private const int Dialog_Height = 300;
        private const string Default_Caption = "Connection Lost!";
        private const float Top_Line_Y = 80.0f;
        private const float Line_Height = 50.0f;
    }
}
