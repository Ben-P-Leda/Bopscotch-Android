using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Threading;

using Microsoft.Xna.Framework;

namespace Bopscotch.Data
{
    public class AreaDataContainer
    {
        private int _levelCount;
        private List<Vector2> _worldPositionsOfGoldenTicketsCollectedFromOpenLevel;
        private List<Vector2> _worldPositionsOfSmashedCratesContainingGoldenTickets;

        public string Name { get; private set; }
        public string SelectionTexture { get; private set; }
        public string DifficultyTag { get; private set; }
        public int SpeedStep { get; private set; }
        public bool Locked { get; set; }
        public bool SelectedForSurvival { get; set; }
        public bool DoesNotHaveRaceCourse { get; set; }
        public int LastSelectedLevel { get; set; }
        public List<int> LevelScores { get; private set; }
        public XElement ContentToUnlockOnCompletion { get; private set; }

        public bool Completed { get { return (LevelScores.Count >= _levelCount); } }
        public bool FirstLevelSelected { get { return (LastSelectedLevel < 1); } }
        public bool FurthestLevelSelected { get { return ((LastSelectedLevel >= LevelScores.Count) || (LastSelectedLevel >= _levelCount - 1)); } }
        public int UnlockedLevelCount { get { return Math.Min(LevelScores.Count + 1, _levelCount); } }

        public AreaDataContainer(string name, string selectionTexture, string difficultyTag, int speedStep, int levelCount)
        {
            Name = name;
            SelectionTexture = selectionTexture;
            DifficultyTag = difficultyTag;
            SpeedStep = speedStep;
            SelectedForSurvival = false;
            LastSelectedLevel = 0;
            Locked = false;
            DoesNotHaveRaceCourse = false;

            LevelScores = new List<int>();

            _levelCount = levelCount;
            _worldPositionsOfGoldenTicketsCollectedFromOpenLevel = new List<Vector2>();
            _worldPositionsOfSmashedCratesContainingGoldenTickets = new List<Vector2>();
        }

        public long ScoreAtCurrentLevelStart
        {
            get
            {
                long score = 0;
                for (int i = 0; i < LastSelectedLevel; i++) { score += LevelScores[i]; }
                return score;
            }
        }

        public XElement Xml
        {
            get
            {
                XElement el = new XElement("area");

                el.Add(new XAttribute("name", Name));
                el.Add(new XAttribute("selection-texture", SelectionTexture));
                el.Add(new XAttribute("difficulty-tag", DifficultyTag));
                el.Add(new XAttribute("speed-step", SpeedStep));
                el.Add(new XAttribute("locked", Locked));
                el.Add(new XAttribute("no-race", DoesNotHaveRaceCourse));
                el.Add(new XAttribute("level-count", _levelCount));
                el.Add(new XAttribute("last-level", LastSelectedLevel));

                XElement scoreElement = new XElement("scores");
                foreach (int i in LevelScores) { scoreElement.Add(new XElement("score", i)); }
                el.Add(scoreElement);

                if (ContentToUnlockOnCompletion != null) { el.Add(ContentToUnlockOnCompletion); }

                if (_worldPositionsOfGoldenTicketsCollectedFromOpenLevel.Count > 0) 
                { 
                    el.Add(CollectedGoldenTicketsData(_worldPositionsOfGoldenTicketsCollectedFromOpenLevel, "tickets-from-level"));
                }
                if (_worldPositionsOfSmashedCratesContainingGoldenTickets.Count > 0)
                {
                    el.Add(CollectedGoldenTicketsData(_worldPositionsOfSmashedCratesContainingGoldenTickets, "tickets-from-crates"));
                }

                return el;
            }
        }

        private XElement CollectedGoldenTicketsData(List<Vector2> worldPositions, string elementName)
        {
            XElement ticketData = new XElement(elementName);
            foreach (Vector2 v in worldPositions)
            {
                XElement el = new XElement("position");
                el.Add(new XAttribute("x", v.X));
                el.Add(new XAttribute("y", v.Y));
                ticketData.Add(el);
            }

            return ticketData;
        }

        public void UpdateCurrentLevelScore(int score)
        {
            while (LevelScores.Count <= LastSelectedLevel) { LevelScores.Add(0); }
            LevelScores[LastSelectedLevel] = score;
        }

        public void UpdateLevelSelection(int stepDirection)
        {
            LastSelectedLevel = (int)MathHelper.Clamp(LastSelectedLevel + stepDirection, 0, Math.Min(LevelScores.Count, _levelCount - 1));
        }

        public void StepToNextLevel()
        {
            if (!Completed) { LastSelectedLevel++; }
        }

        private void SetAreaCompletionUnlockables(XElement unlockablesData)
        {
            ContentToUnlockOnCompletion = unlockablesData;
        }

        private void SetCollectedTicketsFromLevel(XElement dataSource)
        {
            SetCollectedTicketData(_worldPositionsOfGoldenTicketsCollectedFromOpenLevel, dataSource);
        }

        private void SetCollectedTicketData(List<Vector2> worldPositions, XElement dataSource)
        {
            worldPositions.Clear();
            foreach (XElement el in dataSource.Elements("position"))
            {
                worldPositions.Add(new Vector2((float)el.Attribute("x"), (float)el.Attribute("y")));
            }
        }

        private void SetCollectedTicketsFromCrates(XElement dataSource)
        {
            SetCollectedTicketData(_worldPositionsOfSmashedCratesContainingGoldenTickets, dataSource);
        }

        public void CollectGoldenTicketFromOpenLevel(Vector2 ticketWorldPosition)
        {
            _worldPositionsOfGoldenTicketsCollectedFromOpenLevel.Add(ticketWorldPosition);
        }

        public void CollectGoldenTicketFromSmashCrate(Vector2 crateWorldPosition)
        {
            _worldPositionsOfSmashedCratesContainingGoldenTickets.Add(crateWorldPosition);
        }

        public bool GoldenTicketHasBeenCollectedFromLevel(Vector2 ticketWorldPosition)
        {
            return _worldPositionsOfGoldenTicketsCollectedFromOpenLevel.Contains(ticketWorldPosition);
        }

        public bool GoldenTicketHasBeenCollectedFromCrate(Vector2 crateWorldPosition)
        {
            return _worldPositionsOfSmashedCratesContainingGoldenTickets.Contains(crateWorldPosition);
        }

        public static AreaDataContainer CreateFromXml(XElement dataSource)
        {
            AreaDataContainer container = new AreaDataContainer(
                dataSource.Attribute("name").Value, 
                dataSource.Attribute("selection-texture").Value,
                dataSource.Attribute("difficulty-tag").Value,
                (int)dataSource.Attribute("speed-step"),
                (int)dataSource.Attribute("level-count"));

            if (dataSource.Attribute("last-level") != null) { container.LastSelectedLevel = (int)dataSource.Attribute("last-level"); }
            if (dataSource.Attribute("locked") != null) { container.Locked = (bool)dataSource.Attribute("locked"); }
            if (dataSource.Attribute("no-race") != null) { container.DoesNotHaveRaceCourse = (bool)dataSource.Attribute("no-race"); }

            if (dataSource.Element("completion-unlockables") != null) { container.SetAreaCompletionUnlockables(dataSource.Element("completion-unlockables")); }

            if ((dataSource.Element("scores") != null) && (dataSource.Element("scores").Elements("score") != null))
            {
                foreach (XElement s in dataSource.Element("scores").Elements("score")) { container.LevelScores.Add((int)s); }
            }

            if (dataSource.Element("tickets-from-level") != null) { container.SetCollectedTicketsFromLevel(dataSource.Element("tickets-from-level")); }
            if (dataSource.Element("tickets-from-crates") != null) { container.SetCollectedTicketsFromCrates(dataSource.Element("tickets-from-crates")); }

            return container;
        }

        public void Reset()
        {
            if (ContentToUnlockOnCompletion != null)
            {
                foreach (XElement el in ContentToUnlockOnCompletion.Elements("unlockable"))
                {
                    if (el.Attribute("type").Value == "golden-ticket") { el.SetAttributeValue("unlocked", false); }
                }
            }

            LevelScores.Clear();
            _worldPositionsOfGoldenTicketsCollectedFromOpenLevel.Clear();
            _worldPositionsOfSmashedCratesContainingGoldenTickets.Clear();

            LastSelectedLevel = 0;
        }

        public void SetCompletionUnlockables(XElement unlockables)
        {
            foreach (XElement el in unlockables.Elements("unlockable"))
            {
                el.SetAttributeValue("unlocked", Completed);

                bool found = false;
                foreach (XElement x in ContentToUnlockOnCompletion.Elements())
                {
                    if ((x.Attribute("type").Value == el.Attribute("type").Value) && (x.Attribute("name").Value == el.Attribute("name").Value)) { found = true; }
                }

                if (!found) { ContentToUnlockOnCompletion.Add(el); }
            }
        }

        private const string Default_Language_Node = "en-GB";
    }
}
