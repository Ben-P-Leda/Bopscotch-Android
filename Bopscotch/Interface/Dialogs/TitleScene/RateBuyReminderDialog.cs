using System.Collections.Generic;
using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Asset_Management;

namespace Bopscotch.Interface.Dialogs.TitleScene
{
    public class RateBuyReminderDialog : ButtonDialog
    {
        private List<string> _textLines;

        public RateBuyReminderDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Top_Y_When_Active;

            _boxCaption = Translator.Translation("Hi There!");
            _cancelButtonCaption = "Back";

            _textLines = new List<string>();
        }

        public override void Activate(bool skipEntrySequence)
        {
            base.Activate(skipEntrySequence);

			ClearButtons();
            _textLines.Clear();

            AddButton("Back", new Vector2(Definitions.Left_Button_Column_X, 360), Button.ButtonIcon.Back, Color.DodgerBlue, 0.7f);

			SetUpForRatingReminder();
        }

        private void SetUpForBuyFullVersionReminder()
        {
            SetUpReminderButton("Buy Full", Button.ButtonIcon.Store);
            CreateContent("buyfull");
        }

        private void SetUpForRatingReminder()
        {
            SetUpReminderButton("Rate Game", Button.ButtonIcon.Rate);
            CreateContent("review");
        }

        private void SetUpReminderButton(string buttonCaption, Button.ButtonIcon buttonIcon)
        {
            AddButton(buttonCaption, new Vector2(Definitions.Right_Button_Column_X, 360), buttonIcon, Color.Orange, 0.7f);
            ActivateButton(buttonCaption);
        }

        protected void CreateContent(string contentFile)
        {
            XDocument data = FileManager.LoadXMLContentFile(string.Format(Content_Elements_File, Translator.CultureCode, contentFile));

            foreach (XElement el in data.Element("elements").Elements("element")) { _textLines.Add(el.Value); }
        }

        public override void Activate()
        {
            base.Activate();
            Data.Profile.UpdateReminderDetails();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            for (int i = 0; i < _textLines.Count; i++)
            {
                TextWriter.Write(_textLines[i], spriteBatch, new Vector2(Definitions.Back_Buffer_Center.X, WorldPosition.Y + Text_Top + (i * Line_Height)),
                    Color.White, Color.Black, 3.0f, 0.65f, Text_Render_Depth, TextWriter.Alignment.Center);
            }
        }

        private const int Dialog_Height = 480;
        private const float Top_Y_When_Active = 400.0f;
        private const string Content_Elements_File = "Content/Files/Content/{0}/{1}.xml";
        private const float Text_Render_Depth = 0.01f;
        private const float Text_Top = 80.0f;
        private const float Line_Height = 45.0f;
    }
}
