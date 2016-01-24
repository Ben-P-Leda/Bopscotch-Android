using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Asset_Management;
using Leda.Core.Timing;

using Bopscotch.Data;
using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Interface;
using Bopscotch.Interface.Dialogs.RankingScene;
using Bopscotch.Interface.Content;

namespace Bopscotch.Scenes.NonGame
{
    public class RankingScene : ContentSceneWithControlDialog
    {
        private string _selectedArea;
        private NavigationDialog _navigationDialog;
        private Timer _faderTimer;
        private bool _fadingOut;
        private bool _fadeInProgress;
        private int _lastSwipeDirection;

        public RankingScene()
            : base()
        {
            _selectedArea = "Hilltops";
            _faderTimer = new Timer("", CompleteFadeTransition);

            Overlay.Tint = Color.Black;
            Overlay.TintFraction = 0.0f;
            Overlay.RenderLayer = 4;
            Overlay.RenderDepth = 0.99999f;

            GlobalTimerController.GlobalTimer.RegisterUpdateCallback(_faderTimer.Tick);
        }

        private void CompleteFadeTransition()
        {
            if (_fadingOut)
            {
                CreateContentForSelectedArea();
                _fadingOut = false;
                _faderTimer.NextActionDuration = Page_Fade_Duration;
            }
            else
            {
                Overlay.TintFraction = 0.0f;
                _fadeInProgress = false;
                _navigationDialog.PagingComplete = true;
            }
        }

        public override void Initialize()
        {
            _navigationDialog = new NavigationDialog();
            _navigationDialog.SelectionCallback = HandleNavigationDialogDismiss;
            _navigationDialog.PageChangeCallback = HandlePageChange;

            Dialog = _navigationDialog;

            base.Initialize();
        }

        private void HandleNavigationDialogDismiss(string buttonCaption)
        {
            NextSceneType = typeof(TitleScene);
            NextSceneParameters.Set("music-already-running", true);

            Deactivate();
        }

        private void HandlePageChange(string areaName)
        {
            _selectedArea = areaName;

            _fadeInProgress = true;
            _fadingOut = true;
            _faderTimer.NextActionDuration = Page_Fade_Duration;
        }

        public override void Activate()
        {
            base.Activate();

            _faderTimer.Reset();
            _fadingOut = false;
            _fadeInProgress = false;
            _lastSwipeDirection = 0;

            Overlay.TintFraction = 0.0f;

            CreateContentForSelectedArea();
        }

        private void CreateContentForSelectedArea()
        {
            FlushContent();

            CreateTextElement(_selectedArea, new Vector2(Definitions.Back_Buffer_Center.X, Title_Y_Position), TextWriter.Alignment.Center, 1.0f);

            AreaDataContainer areaData = Profile.GetDataForNamedArea(_selectedArea);
            if (areaData != null)
            {
                _background.TextureReference = areaData.SelectionTexture;

                if (areaData.Locked)
                {
                    CreateContentForLockedArea();
                }
                else
                {
                    CreateRankingContent(areaData.LevelScores, areaData.LevelRanks);
                }
            }
        }

        private void CreateContentForLockedArea()
        {
            Rectangle iconFrame = TextureManager.Textures["icon-locked"].Bounds;
            Vector2 iconOrigin = new Vector2(iconFrame.Width, iconFrame.Height) / 2.0f;
            ImageContent icon = CreateImageElement("icon-locked", Definitions.Back_Buffer_Center, iconFrame, iconOrigin, 5.0f);
            icon.RenderDepth = 0.6f;

            CreateTextElement(Translator.Translation("Locked"), Definitions.Back_Buffer_Center, TextWriter.Alignment.Center, 1.0f);
        }

        private void CreateRankingContent(List<int> scores, List<Definitions.SurvivalRank> ranks)
        {
            float leftSide = Definitions.Back_Buffer_Center.X - (Definitions.Back_Buffer_Center.X * 0.8f);
            float rightSide = Definitions.Back_Buffer_Center.X + (Definitions.Back_Buffer_Center.X * 0.2f);
            int cumulativeScore = 0;

            for (int i=0; i < ranks.Count; i++)
            {
                if (((scores.Count > i) && (scores[i] > 0)) || (ranks[i] != Definitions.SurvivalRank.NotSet))
                {
                    float x = i % 2 == 1 ? rightSide : leftSide;
                    float y = ((i / 2) * Table_Line_Height) + Table_Top_Y;

                    CreateTextElement((i + 1).ToString() + ":", new Vector2(x, y), TextWriter.Alignment.Right, 0.6f);

                    if (scores.Count > i)
                    {
                        cumulativeScore += scores[i];
                        CreateTextElement(cumulativeScore.ToString(), new Vector2(x, y), TextWriter.Alignment.Left, 0.6f);

                        if (ranks[i] != Definitions.SurvivalRank.NotSet)
                        {
                            Rectangle rankFrame = new Rectangle(160 * (int)ranks[i], 0, 160, 200);
                            CreateImageElement("ranking-letters", new Vector2(x + Rank_Offset, y + Vertical_Offset), rankFrame, Vector2.Zero, 0.2f);

                            Rectangle starFrame = new Rectangle(0, 0, 120 * (3 - (int)ranks[i]), 120);
                            CreateImageElement("ranking-stars", new Vector2(x + Stars_Offset, y + Vertical_Offset), starFrame, Vector2.Zero, 0.3f);
                        }
                        else
                        {
                            CreateTextElement(Translator.Translation("(unranked)"), new Vector2(x + Rank_Offset, y), TextWriter.Alignment.Left, 0.6f);
                        }
                    }
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (_fadeInProgress)
            {
                Overlay.TintFraction = _fadingOut ? _faderTimer.CurrentActionProgress : 1.0f - _faderTimer.CurrentActionProgress;
            }
            else
            {
                if (_inputProcessors[0].MoveLeft)
                {
                    if (_lastSwipeDirection > -1)
                    {
                        _navigationDialog.HandleAreaStep(1);
                        _lastSwipeDirection = -1;
                    }
                }
                else if (_inputProcessors[0].MoveRight)
                {
                    if (_lastSwipeDirection < 1)
                    {
                        _navigationDialog.HandleAreaStep(-1);
                        _lastSwipeDirection = 1;
                    }
                }
                else
                {
                    _lastSwipeDirection = 0;
                }
            }

            base.Update(gameTime);
        }

        private const float Title_Y_Position = 10.0f;
        private const float Table_Top_Y = 90.0f;
        private const float Table_Line_Height = 40.0f;
        private const float Rank_Offset = 300.0f;
        private const float Stars_Offset = 400.0f;
        private const float Vertical_Offset = 12.5f;
        private const float Page_Fade_Duration = 200.0f;
    }
}
