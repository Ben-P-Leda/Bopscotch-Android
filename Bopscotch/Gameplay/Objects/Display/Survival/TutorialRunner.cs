using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Asset_Management;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Timing;
using Leda.Core.Serialization;

using Bopscotch.Interface;

namespace Bopscotch.Gameplay.Objects.Display.Survival
{
    public class TutorialRunner : ISimpleRenderable, ISerializable
    {
        public delegate void TutorialRunnerStepTrigger(Vector2 playerWorldPosition);
        public delegate void TutorialRunnerPauseTrigger();

        private Vector2 _titlePosition;
        private Vector2 _textPosition;
        private List<XElement> _tutorialData;
        private XElement _activeNode;
        private bool _promptActive;
        private Rectangle _promptFrame;
        private Timer _promptTimer;

        public string ID { get { return "tutorial-runner"; } set { } }
        public bool Visible { get; set; }
        public int RenderLayer { get { return Render_Layer; } set { } }
        public TutorialRunnerPauseTrigger PauseTrigger { private get; set; }
        public bool DisplayingHelp { get { return (_activeNode != null); } }
        public bool StepCanBeDismissed { get; private set; }

        public Rectangle DisplayArea
        {
            set
            {
                _textPosition = new Vector2(
                    Math.Max(value.X + Text_Margin, Text_Minimum_Left),
                    Math.Max(value.Y + Text_Margin, Text_Minimum_Top));
                _titlePosition = new Vector2(
                    Math.Min(value.X + value.Width - Text_Margin, Title_Maximum_Right),
                    Math.Min(value.Y + value.Height - Text_Margin, Title_Maximum_Top));
            }
        }

        public TutorialRunner()
        {
            PauseTrigger = null;

            _promptTimer = new Timer("tutorial-timer");
            _promptTimer.ActionCompletionHandler = UpdateContinuePromptImage;
            GlobalTimerController.GlobalTimer.RegisterUpdateCallback(_promptTimer.Tick);
        }

        private void UpdateContinuePromptImage()
        {
            StepCanBeDismissed = true;

            if (_promptFrame.X != 0) { _promptFrame.X = 0; }
            else { _promptFrame.X = Prompt_Icon_Frame_Width; }

            _promptTimer.NextActionDuration = Prompt_Icon_Frame_Duration_In_Milliseconds;
        }

        public void Initialize()
        {
            XDocument tutorialXML = null;

            try
            {
                string tutorialFileName = string.Format(Tutorial_File_Template, Translator.CultureCode);
                tutorialXML = FileManager.LoadXMLContentFile(tutorialFileName);
            }
            catch
            {
                string tutorialFileName = string.Format(Tutorial_File_Template, Default_Tutorial_File);
                tutorialXML = FileManager.LoadXMLContentFile(tutorialFileName);
            }

            _tutorialData = tutorialXML.Element("tutorial-steps").Elements("step").ToList();
            _activeNode = null;

            _promptFrame = new Rectangle(0, 0, Prompt_Icon_Frame_Width, TextureManager.Textures[Prompt_Icon_Texture_Name].Height);
        }

        public void Reset()
        {
            StepCanBeDismissed = false;
        }

        public void SetForTombstoneRecovery()
        {
            _promptActive = true;
            UpdateContinuePromptImage();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            TextWriter.Write(Translator.Translation("Tutorial"), spriteBatch, _titlePosition, Color.White, 0.75f, 0.01f, TextWriter.Alignment.Right);

            if (_activeNode != null) { DrawStepText(spriteBatch); }
            if ((_promptActive) && (StepCanBeDismissed)) { DrawPointer(spriteBatch); }
        }

        private void DrawPointer(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureManager.Textures[Prompt_Icon_Texture_Name], GameBase.ScreenPosition(Text_Minimum_Left, Title_Maximum_Top), _promptFrame,
                Color.White, 0.0f, Vector2.Zero, GameBase.ScreenScale(1.75f), SpriteEffects.None, 0.01f);
        }

        private void DrawStepText(SpriteBatch spriteBatch)
        {
            TextWriter.Write(_activeNode.Value.Trim(), spriteBatch, _textPosition, Color.White, 0.75f, 0.01f, TextWriter.Alignment.Left);
        }

        public void CheckForStepTrigger(Vector2 playerPosition)
        {
            if (!DisplayingHelp)
            {
                for (int i = 0; i < _tutorialData.Count; i++)
                {
                    int zoneHeight = Definitions.Grid_Cell_Pixel_Size * (_tutorialData[i].Attribute("height") == null ? 3 : (int)_tutorialData[i].Attribute("height"));

                    Rectangle stepArea = new Rectangle(
                        (int)_tutorialData[i].Attribute("x"),
                        (int)_tutorialData[i].Attribute("y") - zoneHeight,
                        Definitions.Grid_Cell_Pixel_Size * (_tutorialData[i].Attribute("width") == null ? 1 : (int)_tutorialData[i].Attribute("width")),
                        zoneHeight);

                    if (stepArea.Contains((int)playerPosition.X, (int)playerPosition.Y))
                    {
                        _promptActive = true;
                        _promptTimer.NextActionDuration = Prompt_Icon_Display_Delay_In_Milliseconds;
                        StepCanBeDismissed = false;
                        _activeNode = _tutorialData[i];
                        PauseTrigger();
                        break;
                    }
                }
            }
        }

        public void ClearCurrentStep()
        {
            if (DisplayingHelp)
            {
                _tutorialData.Remove(_activeNode);
                _activeNode = null;
            }

            _promptActive = false;
        }

        public XElement Serialize()
        {
            Serializer serializer = new Serializer(this);

            serializer.AddDataItem("pointer-active", _promptActive);

            return serializer.SerializedData;
        }

        public void Deserialize(XElement serializedData)
        {
            Serializer serializer = new Serializer(serializedData);

            _promptActive = serializer.GetDataItem<bool>("pointer-active");
        }

        private const int Render_Layer = 0;
        private const float Text_Margin = 50.0f;
        private const float Title_Maximum_Right = 1250.0f;
        private const float Title_Maximum_Top = 725.0f;
        private const float Text_Minimum_Left = 320.0f;
        private const float Text_Minimum_Top = 100.0f;
        private const string Tutorial_File_Template = "Content/Files/Tutorial/{0}.xml";
        private const string Default_Tutorial_File = "en-GB";
        private const string Prompt_Icon_Texture_Name = "tap-icon";
        private const int Prompt_Icon_Frame_Width = 44;
        private const int Prompt_Icon_Display_Delay_In_Milliseconds = 500;
        private const int Prompt_Icon_Frame_Duration_In_Milliseconds = 500;
    }
}
