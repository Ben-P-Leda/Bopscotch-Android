using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;

namespace Bopscotch.Interface.Dialogs.RaceJoinScene
{
    public class JoinRaceOptionsDialog : ButtonDialog
    {
        private float _displayLeftLimit;

        public JoinRaceOptionsDialog(float displayLeftLimit, float totalDisplayWidth)
            : base()
        {
            _displayLeftLimit = displayLeftLimit;

            Height = Dialog_Height;
            TopYWhenActive = RaceOpponentListDialog.Top_Y_When_Active + RaceOpponentListDialog.Dialog_Height + Dialog_Margin;

            AddButton("Change", new Vector2(Definitions.Right_Button_Column_X, Button_Row_Y), Button.ButtonIcon.Options, Color.DodgerBlue, 0.6f);
            AddButton("Back", new Vector2(Definitions.Left_Button_Column_X, Button_Row_Y), Button.ButtonIcon.Back, Color.Red, 0.6f);

            SetMovementLinksForButton("Change", "", "Back", "", "");
            SetMovementLinksForButton("Back", "Change", "", "", "");

            _cancelButtonCaption = "Back";
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(0.0f, Definitions.Back_Buffer_Height + RaceOpponentListDialog.Dialog_Height + Dialog_Margin + Entry_Margin);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            TextWriter.Write(string.Concat(Translator.Translation("Name:")," ", Data.Profile.Settings.RaceName), spriteBatch, 
                new Vector2(_displayLeftLimit + Text_X_Margin, WorldPosition.Y + Text_Y_Margin), Color.White, Color.Black, 2.0f, Text_Render_Depth, 
                TextWriter.Alignment.Left);
        }

        private const int Dialog_Height = 250;
        private const float Button_X_Margin = 220.0f;
        private const float Button_Row_Y = 160.0f;
        private const float Dialog_Margin = 50.0f;
        private const float Text_X_Margin = 30.0f;
        private const float Text_Y_Margin = 15.0f;
        private const float Text_Render_Depth = 0.1f;
    }
}
