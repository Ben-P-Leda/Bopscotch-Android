using System.Xml;
using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Asset_Management;

using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Interface;
using Bopscotch.Interface.Content;
using Bopscotch.Interface.Dialogs;

namespace Bopscotch.Scenes.NonGame
{
    public class CreditsScene : ContentSceneWithControlDialog
    {
        public CreditsScene()
            : base()
        {
            Overlay.Tint = Color.Black;
            Overlay.TintFraction = 0.5f;
        }

        public override void Initialize()
        {
            BackDialog backDialog = new BackDialog();
            backDialog.SelectionCallback = HandleBackDialogDismiss;

            Dialog = backDialog;

            base.Initialize();

            CreateContent(Credits_Content_Elements_File);
        }

        protected void CreateContent(string fileName)
        {
            XDocument content = FileManager.LoadXMLContentFile(string.Format(fileName, Translator.CultureCode));

            foreach (XElement element in content.Element("elements").Elements("element"))
            {
                CreateTextElementFromXml(element);
            }
        }

        protected override void CompletePostStartupLoadInitialization()
        {
            CreateBackgroundForScene(Background_Texture_Name, new int[] { 0, 1, 2, 3 });
        }

        private TextContent CreateTextElementFromXml(XElement source)
        {

            return CreateTextElement(
                source.Value.Trim(),
                new Vector2((float)source.Attribute("x-position"), (float)source.Attribute("y-position")),
                TextWriter.Alignment.Center,
                (float)source.Attribute("scale"));
        }

        private void HandleBackDialogDismiss(string buttonCaption)
        {
            NextSceneType = typeof(TitleScene);

            if (buttonCaption == "Rate") { NextSceneParameters.Set(TitleScene.First_Dialog_Parameter_Name, TitleScene.Rate_Game_Dialog); }
            
            NextSceneParameters.Set("music-already-running", true);

            Deactivate();
        }

        private const string Background_Texture_Name = "background-4";
        private const string Credits_Content_Elements_File = "Content/Files/Content/{0}/credits.xml";
    }
}
