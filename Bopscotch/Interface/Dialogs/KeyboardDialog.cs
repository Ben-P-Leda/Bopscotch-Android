using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Asset_Management;

namespace Bopscotch.Interface.Dialogs
{
    public class KeyboardDialog : ButtonDialog
    {
        private Dictionary<string, Rectangle> _keyAreas;

        public string Prompt { private get; set; }
        public string Entry { get; set; }
        public int MaximumEntryLength { private get; set; }

        public KeyboardDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Top_Y_When_Active;

            AddButton("Back", new Vector2(350.0f, 440.0f), Button.ButtonIcon.Back, Color.Red, 0.6f);
            AddButton("OK", new Vector2(1250.0f, 440.0f), Button.ButtonIcon.Tick, Color.LawnGreen, 0.6f);

            _defaultButtonCaption = "Start";
            _cancelButtonCaption = "Back";

            _keyAreas = new Dictionary<string, Rectangle>();

            Entry = "";
            MaximumEntryLength = Default_Maximum_Entry_Length;

            CreateKeys();
        }

        private void CreateKeys()
        {
            CreateStandardKeyRow(Row_1_Keys.Split(','), 1);
            CreateStandardKeyRow(Row_2_Keys.Split(','), 2);
            CreateStandardKeyRow(Row_3_Keys.Split(','), 3);
            CreateStandardKeyRow(Row_4_Keys.Split(','), 4);

            _keyAreas.Add(
                Translator.Translation("SPACE"), 
                new Rectangle(
                    (int)(Definitions.Back_Buffer_Center.X - (Space_Bar_Width / 2.0f)), 
                    (int)(Top_Row_Offset + (5 * Row_Height)), 
                    (int)Space_Bar_Width, 
                    (int)Row_Height));
        }

        private void CreateStandardKeyRow(string[] keys, int rowIndex)
        {
            Vector2 rowTopLeft = 
                new Vector2(Definitions.Back_Buffer_Center.X - ((keys.Length * Standard_Button_Width) / 2.0f), Top_Row_Offset + (rowIndex * Row_Height));

            for (int i = 0; i < keys.Length; i++)
            {
                _keyAreas.Add(
                    keys[i],
                    new Rectangle((int)(rowTopLeft.X + (i * Standard_Button_Width)), (int)(rowTopLeft.Y), (int)Standard_Button_Width, (int)Row_Height));
            }
        }

        public override void Activate()
        {
            if (Entry.Length < 1) { DisableButton("Start"); }
            else { EnableButton("Start"); }

            base.Activate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            foreach (KeyValuePair<string, Rectangle> kvp in _keyAreas) { DrawKey(spriteBatch, kvp.Key, kvp.Value); }

            TextWriter.Write(
                string.Concat(Translator.Translation("Name:"), " ", Entry, "_"),
                spriteBatch,
                new Vector2(160.0f, WorldPosition.Y + 10.0f),
                Color.White,
                Color.Black,
                3.0f,
                0.7f,
                Character_Render_Depth,
                TextWriter.Alignment.Left);
        }

        private void DrawKey(SpriteBatch spriteBatch, string character, Rectangle touchAreaBounds)
        {
            Vector2 topLeft = GameBase.ScreenPosition(touchAreaBounds.X + Key_Margin, touchAreaBounds.Y + Key_Margin + WorldPosition.Y);
            Rectangle targetArea = new Rectangle((int)topLeft.X, (int)topLeft.Y,
                (int)GameBase.ScreenScale(touchAreaBounds.Width - (Key_Margin * 2)), (int)GameBase.ScreenScale(touchAreaBounds.Height - (Key_Margin * 2)));

            spriteBatch.Draw(TextureManager.Textures[Key_Texture], targetArea, null, Color.Lerp(Color.Black, Color.Transparent, 0.5f), 0.0f, 
                Vector2.Zero, SpriteEffects.None, Key_Background_Render_Depth);

            TextWriter.Write(character, spriteBatch, new Vector2(touchAreaBounds.X + (touchAreaBounds.Width / 2.0f), touchAreaBounds.Y + WorldPosition.Y), 
                Color.White, Character_Scale, Character_Render_Depth, TextWriter.Alignment.Center);
        }

        protected override void CheckForAndHandleSelection(Input.InputProcessorBase inputSource)
        {
            base.CheckForAndHandleSelection(inputSource);

            if ((inputSource.SelectionTriggered) && (inputSource.SelectionLocation != Vector2.Zero))
            {
                foreach (KeyValuePair<string, Rectangle> kvp in _keyAreas)
                {
                    if (kvp.Value.Contains((int)inputSource.SelectionLocation.X, (int)(inputSource.SelectionLocation.Y - WorldPosition.Y)))
                    {
                        if (kvp.Key == "DEL") { DeleteLastCharacterFromEntry(); }
                        else { AddCharacterToEntry(kvp.Key); }
                    }
                }
            }

            if (Entry.Length < 1) { DisableButton("Start"); }
            else { EnableButton("Start"); }
        }

        private void AddCharacterToEntry(string toAdd)
        {
            if (toAdd == Translator.Translation("SPACE")) { toAdd = " "; }
            if (Entry.Length < MaximumEntryLength) { Entry = string.Concat(Entry, toAdd); }
        }

        private void DeleteLastCharacterFromEntry()
        {
            if (!string.IsNullOrEmpty(Entry)) { Entry = Entry.Substring(0, Entry.Length - 1); }
        }

        protected override void Dismiss()
        {
            if ((!string.IsNullOrEmpty(Entry.Trim())) || (_activeButtonCaption != "Start"))
            {
                Entry = Entry.Trim();
                base.Dismiss();
            }
        }

        private const int Dialog_Height = 520;
        private const float Top_Y_When_Active = 360.0f;
        private const int Default_Maximum_Entry_Length = 15;

        private const string Row_1_Keys = "1,2,3,4,5,6,7,8,9,0,DEL";
        private const string Row_2_Keys = "Q,W,E,R,T,Y,U,I,O,P";
        private const string Row_3_Keys = "A,S,D,F,G,H,J,K,L";
        private const string Row_4_Keys = "Z,X,C,V,B,N,M,.";

        private const float Space_Bar_Width = 700.0f;

        private const float Standard_Button_Width = 117.0f;
        private const float Row_Height = 65.0f;
        private const float Top_Row_Offset = 15.0f;
        private const float Key_Margin = 3.0f;
        private const string Key_Texture = "pixel";
        private const float Key_Background_Render_Depth = 0.12f;
        private const float Character_Render_Depth = 0.1f;
        private const float Character_Scale = 0.6f;

    }
}