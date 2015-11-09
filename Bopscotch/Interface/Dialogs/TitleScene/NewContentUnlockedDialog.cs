using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;

namespace Bopscotch.Interface.Dialogs.TitleScene
{
    public class NewContentUnlockedDialog : ButtonDialog
    {
        private List<string> _newContent;

        public bool HasContent { get { return (_newContent.Count > 0); } }

        public NewContentUnlockedDialog()
        {
            _newContent = new List<string>();

            Height = Dialog_Height;
            TopYWhenActive = Top_Y_When_Active;

            AddButton("OK", new Vector2(Definitions.Back_Buffer_Center.X, 320), Button.ButtonIcon.Tick, Color.LawnGreen);
            AddButton("Back", new Vector2(Definitions.Back_Buffer_Center.X, 1500), Button.ButtonIcon.None, Color.Transparent);

            _defaultButtonCaption = "OK";
            _cancelButtonCaption = "Back";

            _boxCaption = "New Content Unlocked!";
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _newContent.Count; i++)
            {
                TextWriter.Write(_newContent[i], spriteBatch,
                    new Vector2(Definitions.Back_Buffer_Center.X, Top_Line + (i * Line_Height) + WorldPosition.Y), 
                    Color.White, 
                    Color.Black, 
                    3.0f, 
                    0.7f, 
                    0.1f, 
                    TextWriter.Alignment.Center);
            }
            base.Draw(spriteBatch);
        }

        public void PrepareForActivation()
        {
            _newContent.Clear();
        }

        public void AddItem(string text)
        {
            _newContent.Add(text);
        }

        private const int Dialog_Height = 420;
        private const float Top_Y_When_Active = 400.0f;
        private const float Top_Line = 100.0f;
        private const float Line_Height = 55.0f;
    }
}
