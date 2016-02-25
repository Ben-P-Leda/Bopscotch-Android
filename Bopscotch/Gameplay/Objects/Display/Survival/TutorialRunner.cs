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
    public class TutorialRunner
    {
        public delegate void TutorialRunnerStepTrigger(Vector2 playerWorldPosition);
        public delegate void TutorialRunnerPauseTrigger();

        private List<XElement> _tutorialData;
        private XElement _activeNode;

        public TutorialRunnerPauseTrigger PauseTrigger { private get; set; }
        public bool DisplayingHelp { get { return (_activeNode != null); } }
        public bool StepCanBeDismissed { get; private set; }

        public string StepText { get { return _activeNode.Value.Trim(); } }

        public TutorialRunner()
        {
            PauseTrigger = null;
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
        }

        public void Reset()
        {
            StepCanBeDismissed = false;
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
        }

        private const string Tutorial_File_Template = "Content/Files/Tutorial/{0}.xml";
        private const string Default_Tutorial_File = "en-GB";
    }
}
