using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Gamestate_Management;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;

using Bopscotch.Interface;
using Bopscotch.Data;
using Bopscotch.Effects.Popups.Ranking;

namespace Bopscotch.Gameplay.Coordination
{
    public class SurvivalRankingCoordinator : ISimpleRenderable, ISerializable
    {
        public delegate void RankSequenceCallback();

        private Scene.ObjectRegistrationHandler _registerObject;
        private RankingStar[] _rankingStars;
        private RankingLetter _rankingLetter;

        public string ID { get { return "ranking-coordinator"; } set { } }
        public bool Visible { get; set; }
        public int RenderLayer { get { return Render_Layer; } set { } }
        public bool LevelCompleted { get; set; }

        public SurvivalRankingCoordinator(RankSequenceCallback completionCallback, Scene.ObjectRegistrationHandler registrationHandler)
        {
            _registerObject = registrationHandler;

            _rankingStars = new RankingStar[Ranking_Star_Count];
            for (int i = 0; i < Ranking_Star_Count; i++)
            {
                _rankingStars[i] = new RankingStar();
            }

            _rankingLetter = new RankingLetter();
            _rankingLetter.NextAction = completionCallback;
        }

        public void Initialize()
        {
            for (int i = 0; i < Ranking_Star_Count; i++)
            {
                _rankingStars[i].Initialize();
                _rankingStars[i].FrameOffset = i;
            }

            _rankingLetter.Initialize();
        }

        public void Reset()
        {
            Visible = false;
            LevelCompleted = false;

            for (int i = 0; i < Ranking_Star_Count; i++)
            {
                _rankingStars[i].Reset();
                _registerObject(_rankingStars[i]);
            }

            _rankingLetter.Reset();
            _registerObject(_rankingLetter);
        }

        public Definitions.SurvivalRank GetRankForLevel(SurvivalLevelData levelData)
        {
            Definitions.SurvivalRank rank = Definitions.SurvivalRank.C;

            if ((levelData.CandyCollectionFraction >= levelData.RankACandyFraction) && (levelData.AttemptsAtLevel < Rank_A_Lives_Used))
            {
                rank = Definitions.SurvivalRank.A;
            }
            else if ((levelData.CandyCollectionFraction >= levelData.RankBCandyFraction) || (levelData.AttemptsAtLevel < Rank_B_Lives_Used)) 
            { 
                rank = Definitions.SurvivalRank.B;
            }

            return rank;
        }

        public void DisplayRanking(Definitions.SurvivalRank rank)
        {
            _rankingLetter.FrameOffset = (int)rank;

            switch (rank)
            {
                case Definitions.SurvivalRank.A: DisplayForRankA(); break;
                case Definitions.SurvivalRank.B: DisplayForRankB(); break;
                default: DisplayForRankC(); break;
            }

            Visible = true;

            _rankingStars[0].Activate();
        }

        private void DisplayForRankA()
        {
            _rankingStars[2].DisplayPosition = Definitions.Back_Buffer_Center + new Vector2(0.0f, Display_Line - (Definitions.Grid_Cell_Pixel_Size * 0.5f));
            _rankingStars[2].NextAction = _rankingLetter.Activate;

            _rankingStars[1].DisplayPosition = Definitions.Back_Buffer_Center + new Vector2(Definitions.Grid_Cell_Pixel_Size * 2.0f, Display_Line);
            _rankingStars[1].NextAction = _rankingStars[2].Activate;

            _rankingStars[0].DisplayPosition = Definitions.Back_Buffer_Center + new Vector2(-Definitions.Grid_Cell_Pixel_Size * 2.0f, Display_Line);
            _rankingStars[0].NextAction = _rankingStars[1].Activate;
        }

        private void DisplayForRankB()
        {
            _rankingStars[1].DisplayPosition = Definitions.Back_Buffer_Center + new Vector2(Definitions.Grid_Cell_Pixel_Size, Display_Line);
            _rankingStars[1].NextAction = _rankingLetter.Activate;

            _rankingStars[0].DisplayPosition = Definitions.Back_Buffer_Center + new Vector2(-Definitions.Grid_Cell_Pixel_Size, Display_Line);
            _rankingStars[0].NextAction = _rankingStars[1].Activate;
        }

        private void DisplayForRankC()
        {
            _rankingStars[0].DisplayPosition = Definitions.Back_Buffer_Center + new Vector2(0.0f, Display_Line);
            _rankingStars[0].NextAction = _rankingLetter.Activate;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            TextWriter.Write(Translator.Translation("Your Ranking:"), spriteBatch, new Vector2(Definitions.Back_Buffer_Center.X, Prompt_Line),
                Color.White, Color.Black, 3.0f, 0.7f, 0.1f, TextWriter.Alignment.Center);
        }

        public XElement Serialize()
        {
            Serializer serializer = new Serializer(this);

            serializer.AddDataItem("level-complete", LevelCompleted);

            return serializer.SerializedData;
        }

        public void Deserialize(XElement serializedData)
        {
            Serializer serializer = new Serializer(serializedData);

            LevelCompleted = serializer.GetDataItem<bool>("level-complete");
        }


        private const int Render_Layer = 4;
        private const int Ranking_Star_Count = 3;
        private const float Rank_B_Lives_Used = 4;
        private const float Rank_A_Lives_Used = 1;
        private const float Display_Line = -100.0f;
        private const float Prompt_Line = 150.0f;
    }
}
