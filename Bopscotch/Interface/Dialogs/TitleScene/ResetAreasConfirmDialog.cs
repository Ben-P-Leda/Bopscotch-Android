using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;

namespace Bopscotch.Interface.Dialogs.TitleScene
{
    public class ResetAreasConfirmDialog : ButtonDialog
    {
        public ResetAreasConfirmDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Top_Y_When_Active;

            AddButton("Confirm", new Vector2(Definitions.Left_Button_Column_X, 325), Button.ButtonIcon.Tick, Color.Red, 0.7f);
            AddButton("Cancel", new Vector2(Definitions.Right_Button_Column_X, 325), Button.ButtonIcon.Play, Color.DodgerBlue, 0.7f);

            SetMovementLinksForButton("Confirm", "", "", "", "Cancel");
            SetMovementLinksForButton("Cancel", "", "", "Confirm", "");

            _defaultButtonCaption = "Cancel";
            _cancelButtonCaption = "Cancel";

            _boxCaption = "Reset Game?";
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            TextWriter.Write("This will clear all scores,", spriteBatch, 
                new Vector2(Definitions.Back_Buffer_Center.X, 80.0f + WorldPosition.Y), Color.White, Color.Black, 3.0f, 0.7f, 0.1f, TextWriter.Alignment.Center);
            TextWriter.Write("unlocked levels and gold tickets!", spriteBatch,
                new Vector2(Definitions.Back_Buffer_Center.X, 130.0f + WorldPosition.Y), Color.White, Color.Black, 3.0f, 0.7f, 0.1f, TextWriter.Alignment.Center);

            TextWriter.Write("(Areas and costumes will remain unlocked)", spriteBatch,
                new Vector2(Definitions.Back_Buffer_Center.X, 190.0f + WorldPosition.Y), Color.White, Color.Black, 3.0f, 0.6f, 0.1f, TextWriter.Alignment.Center);

            base.Draw(spriteBatch);
        }

        private const int Dialog_Height = 400;
        private const float Top_Y_When_Active = 350.0f;
    }
}

