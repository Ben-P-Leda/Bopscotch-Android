using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;
using Leda.Core.Asset_Management;

using Bopscotch.Data;
using Bopscotch.Gameplay.Objects.Environment.Blocks;
using Bopscotch.Gameplay.Objects.Environment.Collectables;

namespace Bopscotch.Data
{
    public class SurvivalLevelData : LevelData, ISerializable
    {
        public string ID { get { return Level_Data_ID; } set { } }
        public int PointsScoredThisLevel { get; set; }
        public int TotalCandiesOnLevel { private get; set; }
        public int AttemptsAtLevel { get; set; }

        public float CandyCollectionFraction { get { return _candiesCollected / TotalCandiesOnLevel; } }

        private float _candiesCollected;

        public SurvivalLevelData()
            : base()
        {
            _candiesCollected = 0;

            PointsScoredThisLevel = 0;
        }

        public XElement Serialize()
        {
            Serializer serializer = new Serializer(this);

            serializer.AddDataItem("level-index", Profile.CurrentAreaData.LastSelectedLevel);
            serializer.AddDataItem("play-state", CurrentPlayState);
            serializer.AddDataItem("accrued-score", PointsScoredThisLevel);
            serializer.AddDataItem("total-candies", TotalCandiesOnLevel);
            serializer.AddDataItem("candies-collected", _candiesCollected);
            serializer.AddDataItem("attempts-count", AttemptsAtLevel);

            return serializer.SerializedData;
        }

        public void Deserialize(XElement serializedData)
        {
            Serializer serializer = new Serializer(serializedData);

            Profile.CurrentAreaData.LastSelectedLevel = serializer.GetDataItem<int>("level-index");
            CurrentPlayState = serializer.GetDataItem<PlayState>("play-state");
            PointsScoredThisLevel = serializer.GetDataItem<int>("accrued-score");
            TotalCandiesOnLevel = serializer.GetDataItem<int>("total-candies");
            _candiesCollected = serializer.GetDataItem<int>("candies-collected");
            AttemptsAtLevel = serializer.GetDataItem<int>("attempts-count");
        }

        public void UpdateFromItemCollection(Collectable collectedItem)
        {
            if (collectedItem is ScoringCandy) 
            { 
                AddScoreForUnits(((ScoringCandy)collectedItem).ScoreValue, 1);
                _candiesCollected++;
                SoundEffectManager.PlayEffect("collect-candy");
            }
            else if (collectedItem is GoldenTicket) 
            { 
                HandleGoldenTicketCollection(collectedItem); 
            }
        }

        private void HandleGoldenTicketCollection(IWorldObject ticketSource)
        {
            Profile.GoldenTickets++;
            if (ticketSource is GoldenTicket) 
            { 
                Profile.CurrentAreaData.CollectGoldenTicketFromOpenLevel(ticketSource.WorldPosition); 
            }
            else if (ticketSource is SmashBlock) 
            { 
                Profile.CurrentAreaData.CollectGoldenTicketFromSmashCrate(ticketSource.WorldPosition); 
            }

            SoundEffectManager.PlayEffect("generic-fanfare");
        }

        private void AddScoreForUnits(float perUnitScore, float numberOfUnits)
        {
            PointsScoredThisLevel += (int)(perUnitScore * numberOfUnits);
        }

        public void UpdateScoreForMovement(int millisecondsSinceLastUpdate, float playerSpeed)
        {
            AddScoreForUnits(playerSpeed * playerSpeed * playerSpeed * Score_Speed_Scaler, millisecondsSinceLastUpdate);
        }

        public void UpdateFromSmashBlockContents(SmashBlock smashedBlock)
        {
            for (int i = 0; i < smashedBlock.Contents.Count; i++)
            {
                switch (smashedBlock.Contents[i].AffectsItem)
                {
                    case SmashBlockItemData.AffectedItem.Score: 
                        AddScoreForUnits(smashedBlock.Contents[i].Value, smashedBlock.Contents[i].Count);
                        _candiesCollected += smashedBlock.Contents[i].Count;
                        break;
                    case SmashBlockItemData.AffectedItem.GoldenTicket: 
                        HandleGoldenTicketCollection(smashedBlock); 
                        break;
                }
            }
        }

        public const string Level_Data_ID = "survival-level-data";

        private const float Score_Speed_Scaler = 0.625f;
    }
}
