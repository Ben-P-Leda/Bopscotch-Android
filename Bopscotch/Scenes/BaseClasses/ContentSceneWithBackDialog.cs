using System.Xml.Linq;

using Microsoft.Xna.Framework;

#if WINDOWS_PHONE
using Microsoft.Xna.Framework.GamerServices;
#endif

using Leda.Core;
using Leda.Core.Asset_Management;
using Leda.Core.Game_Objects.Controllers;

using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Interface;
using Bopscotch.Interface.Content;
using Bopscotch.Interface.Dialogs;

namespace Bopscotch.Scenes.NonGame
{
    public abstract class ContentSceneWithBackDialog : FlatContentScene
    {
        private BackDialog _backDialog;
        private MotionController _motionController;

        protected bool _maintainsTitleSceneMusic;
        protected string _contentFileName;

        public ContentSceneWithBackDialog()
            : base()
        {
            _motionController = new MotionController();

            RegisterGameObject(new Effects.FullScreenColourOverlay() { Tint = Color.Black, TintFraction = 0.5f });

            _maintainsTitleSceneMusic = true;            
        }

        public override void Initialize()
        {
            base.Initialize();

            _backDialog = new BackDialog();
            _backDialog.InputSources = _inputProcessors;
            _backDialog.SelectionCallback = HandleBackDialogDismiss;

            _motionController.AddMobileObject(_backDialog);

            RegisterGameObject(_backDialog);

            CreateContent(_contentFileName);
        }

        protected override void Reset()
        {
            _backDialog.Reset();

            base.Reset();
        }

        private void HandleBackDialogDismiss(string buttonCaption)
        {
            NextSceneType = typeof(TitleScene);

			if (buttonCaption == "Rate") { NextSceneParameters.Set(TitleScene.First_Dialog_Parameter_Name, TitleScene.Rate_Game_Dialog); }
			if (buttonCaption == "Full Game") { NextSceneParameters.Set(TitleScene.First_Dialog_Parameter_Name, "purchase"); }
            if (_maintainsTitleSceneMusic) { NextSceneParameters.Set("music-already-running", true); }

            Deactivate();
        }

        protected void CreateContent(string fileName)
        {
            XDocument content = FileManager.LoadXMLContentFile(string.Format(fileName, Translator.CultureCode));

            foreach (XElement element in content.Element("elements").Elements("element"))
            {
                switch (element.Attribute("type").Value)
                {
                    case "text": CreateTextElementFromXml(element); break;
                    case "image": CreateImageElementFromXml(element); break;
                }
            }
        }

        private TextContent CreateTextElementFromXml(XElement source)
        {
            TextContent element = new TextContent(
                Translator.Translation(source.Value.Trim()),
                new Vector2((float)source.Attribute("x-position"), (float)source.Attribute("y-position")));

            element.RenderDepth = Element_Render_Depth;

            if (source.Attribute("scale") != null) { element.Scale = (float)source.Attribute("scale"); }
            if (source.Attribute("alignment") != null)
            {
                switch (source.Attribute("alignment").Value)
                {
                    case "left": element.Alignment = TextWriter.Alignment.Left; break;
                    case "right": element.Alignment = TextWriter.Alignment.Right; break;
                }
            }

            RegisterGameObject(element);

            return element;
        }

        private ImageContent CreateImageElementFromXml(XElement source)
        {
            ImageContent element = new ImageContent(source.Value.Trim(), new Vector2((float)source.Attribute("x-position"), (float)source.Attribute("y-position")));

            if (source.Attribute("scale") != null) { element.Scale = (float)source.Attribute("scale"); }

            if (source.Element("frame") != null)
            {
                element.Frame = new Rectangle((int)source.Element("frame").Attribute("x"), (int)source.Element("frame").Attribute("y"),
                    (int)source.Element("frame").Attribute("width"), (int)source.Element("frame").Attribute("height"));
            }

            if (source.Element("origin") != null)
            {
                element.Origin = new Vector2((float)source.Element("origin").Attribute("x"), (float)source.Element("origin").Attribute("y"));
            }

            element.RenderDepth = Element_Render_Depth;

            RegisterGameObject(element);

            return element;
        }

        public override void Activate()
        {
            _backDialog.Activate();

            base.Activate();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _motionController.Update(MillisecondsSinceLastUpdate);
        }

        protected override void HandleBackButtonPress()
        {
            base.HandleBackButtonPress();

            if (CurrentState == Status.Active) { _backDialog.Cancel(); }
        }

        private const float Element_Render_Depth = 0.5f;
    }
}
