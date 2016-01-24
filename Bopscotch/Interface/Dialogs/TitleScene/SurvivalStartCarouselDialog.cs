using System.Collections.Generic;
using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Gamestate_Management;
using Leda.Core.Timing;
using Leda.Core.Asset_Management;

using Bopscotch.Input;
using Bopscotch.Interface.Dialogs.Carousel;

namespace Bopscotch.Interface.Dialogs.TitleScene
{
    public class SurvivalStartCarouselDialog : AreaSelectionCarouselDialog
    {
        public SurvivalStartCarouselDialog(Scene.ObjectRegistrationHandler registrationHandler, Scene.ObjectUnregistrationHandler unregistrationHandler)
            : base(registrationHandler, unregistrationHandler)
        {
            _boxCaption = Translator.Translation("Select Area and Level");

            Height = Dialog_Height;
            TopYWhenActive = 300;
            TopYWhenInactive = Definitions.Back_Buffer_Height;
            CarouselCenter = new Vector2(Definitions.Back_Buffer_Center.X, Carousel_Center_Y);
            CarouselRadii = new Vector2(Carousel_Horizontal_Radius, Carousel_Vertical_Radius);

            _captionsForButtonsNotActivatedByGamepadStartButton.Add("Back");
        }

        protected override void CreateButtons()
        {
            AddIconButton("previous", new Vector2(Definitions.Back_Buffer_Center.X - 470, 200), Button.ButtonIcon.Previous, Color.DodgerBlue);
            AddIconButton("next", new Vector2(Definitions.Back_Buffer_Center.X + 470, 200), Button.ButtonIcon.Next, Color.DodgerBlue);

            AddIconButton("previous-level", new Vector2(Definitions.Back_Buffer_Center.X - 470, 360), Button.ButtonIcon.Previous, Color.DodgerBlue);
            AddIconButton("next-level", new Vector2(Definitions.Back_Buffer_Center.X + 470, 360), Button.ButtonIcon.Next, Color.DodgerBlue);

			AddButton("Back", new Vector2(Definitions.Left_Button_Column_X, 500), Button.ButtonIcon.Back, Color.Red, 0.7f);
            AddButton("Start!", new Vector2(Definitions.Right_Button_Column_X, 500), Button.ButtonIcon.Play, Color.LawnGreen, 0.7f);

            _nonSpinButtonCaptions.Add("Start!");
            _nonSpinButtonCaptions.Add("previous-level");
            _nonSpinButtonCaptions.Add("next-level");
            _defaultButtonCaption = "Start!";

            _buttonSoundEffectOverrides.Add("previous-level", "menu-move");
            _buttonSoundEffectOverrides.Add("next-level", "menu-move");

            base.CreateButtons();
        }

        protected override void HandleNonSpinButtonAction(string buttonCaption)
        {
            switch (buttonCaption)
            {
                case "previous-level": AttemptToStepSelectedLevel(-1, buttonCaption, Data.Profile.CurrentAreaData.FirstLevelSelected); break;
                case "next-level": AttemptToStepSelectedLevel(1, buttonCaption, Data.Profile.CurrentAreaData.FurthestLevelSelected); break;
                default: DismissWithReturnValue(buttonCaption); break;
            }
        }

        private void AttemptToStepSelectedLevel(int stepDirection, string stepButtonCaption, bool stepNotPossible)
        {
            if (!stepNotPossible)
            {
                Data.Profile.CurrentAreaData.UpdateLevelSelection(stepDirection);
                SetupButtonsForSelectedAreaAndLevel();

                if (_buttons[stepButtonCaption].Disabled) { ActivateButton("Start!"); }
            }
        }

        protected void SetupButtonsForSelectedAreaAndLevel()
        {
            if ((bool)_dataSource[SelectedItem].Attribute("locked")) { SetupButtonsForLockedArea(); }
            else { SetupButtonsForAvailableArea(Data.Profile.CurrentAreaData.FirstLevelSelected, Data.Profile.CurrentAreaData.FurthestLevelSelected); }
        }

        private void SetupButtonsForLockedArea()
        {
            SetMovementLinksForButton("previous", "", "Back", "", "next");
            SetMovementLinksForButton("next", "", "Back", "previous", "");
            SetMovementLinksForButton("Back", "previous", "", "", "next");

            _buttons["Start!"].Disabled = true;
            _buttons["previous-level"].Disabled = true;
            _buttons["next-level"].Disabled = true;
        }

        private void SetupButtonsForAvailableArea(bool selectedIsFirst, bool selectedIsFurthest)
        {
            if (selectedIsFirst)
            {
                SetMovementLinksForButton("previous", "", "Back", "", "next");
                SetMovementLinksForButton("next-level", "next", "Start!", "", "");
                SetMovementLinksForButton("Back", "previous", "", "", "Start!");
                _buttons["previous-level"].Disabled = true;
            }
            else
            {
                SetMovementLinksForButton("previous", "", "previous-level", "", "next");
                SetMovementLinksForButton("next-level", "next", "Start!", "previous-level", "");
                SetMovementLinksForButton("Back", "previous-level", "", "", "Start!");
                _buttons["previous-level"].Disabled = false;
            }

            if (selectedIsFurthest)
            {
                SetMovementLinksForButton("next", "", "Start!", "previous", "");
                SetMovementLinksForButton("previous-level", "previous", "Back", "", "");
                SetMovementLinksForButton("Start!", "next", "", "Back", "");
                _buttons["next-level"].Disabled = true;
            }
            else
            {
                SetMovementLinksForButton("next", "", "next-level", "previous", "");
                SetMovementLinksForButton("previous-level", "previous", "Back", "", "next-level");
                SetMovementLinksForButton("Start!", "next-level", "", "Back", "");
                _buttons["next-level"].Disabled = false;
            }

            _buttons["Start!"].Disabled = false;
        }

        public override void Activate()
        {
            base.Activate();

            SetInitialSelection(Data.Profile.CurrentAreaData.Name);
            SetupButtonsForSelectedAreaAndLevel();

            if ((bool)_dataSource[SelectedItem].Attribute("locked")) { ActivateButton("previous"); }
            else { ActivateButton("Start!"); }
        }

        protected override void HandleDialogExitCompletion()
        {
            Data.Profile.Save();
            
            base.HandleDialogExitCompletion();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (!(bool)_dataSource[SelectedItem].Attribute("locked")) { DrawCurrentLevelSelection(spriteBatch); }
        }

        protected override void DrawAreaDetails(SpriteBatch spriteBatch)
        {
            string areaText = _dataSource[SelectedItem].Attribute("name").Value;

            if ((bool)_dataSource[SelectedItem].Attribute("locked"))
            {
                areaText = string.Concat(areaText, " (", Translator.Translation("locked"), ")");
            }
            else if (_dataSource[SelectedItem].Attribute("difficulty").Value != "n/a")
            {
                areaText = string.Concat(areaText, " (", Translator.Translation(_dataSource[SelectedItem].Attribute("difficulty").Value), ")");
            }

            TextWriter.Write(areaText, spriteBatch, new Vector2(Definitions.Back_Buffer_Center.X, WorldPosition.Y + 290.0f),
                TransitionTint(_textTint), TransitionTint(Color.Black), 3.0f, 0.75f, 0.1f, TextWriter.Alignment.Center);
        }

        private void DrawCurrentLevelSelection(SpriteBatch spriteBatch)
        {
            int rankValue = 0;
            float rankingSpace = Star_Dest_Frame_Side_Length;

            if ((Data.Profile.CurrentAreaData.Name != "Tutorial")
                && (Data.Profile.CurrentAreaData.LevelRanks != null)
                && (Data.Profile.CurrentAreaData.LevelRanks.Count > Data.Profile.CurrentAreaData.LastSelectedLevel))
            {
                Definitions.SurvivalRank rank = Data.Profile.CurrentAreaData.LevelRanks[Data.Profile.CurrentAreaData.LastSelectedLevel];
                rankValue = rank < 0 ? 0 : 3 - (int)rank;
                rankingSpace = rank < 0 ? Star_Dest_Frame_Side_Length : ((rankValue + 0.5f) * Star_Dest_Frame_Side_Length * 2.0f);
            }

            string currentLevelText = Translator.Translation("Level [N]").Replace("[N]", (Data.Profile.CurrentAreaData.LastSelectedLevel + 1).ToString());
            float currentLevelTextWidth = TextWriter.CalculateTextDimensions(currentLevelText, 0.75f).X;

            string availableLevelsText = Translator.Translation("([N] unlocked)").Replace("[N]", Data.Profile.CurrentAreaData.UnlockedLevelCount.ToString());

            float totalWidth = currentLevelTextWidth + TextWriter.CalculateTextDimensions(availableLevelsText, 0.75f).X + rankingSpace;

            float leftSideX = Definitions.Back_Buffer_Center.X - (totalWidth / 2.0f);
            float starsX = leftSideX + currentLevelTextWidth + (Star_Dest_Frame_Side_Length / 2.0f);

            TextWriter.Write(currentLevelText, spriteBatch, new Vector2(leftSideX, WorldPosition.Y + 350.0f),
                TransitionTint(_textTint), TransitionTint(Color.Black), 3.0f, 0.75f, 0.1f, TextWriter.Alignment.Left);

            TextWriter.Write(availableLevelsText, spriteBatch, new Vector2(Definitions.Back_Buffer_Center.X + (totalWidth / 2.0f), WorldPosition.Y + 350.0f),
                TransitionTint(_textTint), TransitionTint(Color.Black), 3.0f, 0.75f, 0.1f, TextWriter.Alignment.Right);

            Vector2 topLeft = GameBase.ScreenPosition(starsX, WorldPosition.Y + 365.0f);

            Rectangle destination = new Rectangle((int)(topLeft.X), (int)(topLeft.Y), Star_Dest_Frame_Side_Length * rankValue, Star_Dest_Frame_Side_Length);
            Rectangle source = new Rectangle(0, 0, Star_Source_Frame_Side_Length * rankValue, Star_Source_Frame_Side_Length);

            spriteBatch.Draw(TextureManager.Textures["ranking-stars"], destination, source, TransitionTint(_textTint), 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
        }

        protected override void HandleStepSelection(string buttonCaption)
        {
            base.HandleStepSelection(buttonCaption);

            _textTransitionTimer.NextActionDuration = Text_Fade_Duration_In_Milliseconds;
        }

        protected override void HandleRotationComplete()
        {
            base.HandleRotationComplete();

            Data.Profile.CurrentAreaName = Selection;
            SetupButtonsForSelectedAreaAndLevel();

            _textTransitionTimer.NextActionDuration = Text_Fade_Duration_In_Milliseconds;
        }

        private const float Carousel_Center_Y = 190.0f;
        private const float Carousel_Horizontal_Radius = 270.0f;
        private const float Carousel_Vertical_Radius = 35.0f;
        private const int Dialog_Height = 580;
        private const int Text_Fade_Duration_In_Milliseconds = 150;
        private const int Star_Source_Frame_Side_Length = 120;
        private const int Star_Dest_Frame_Side_Length = 30;
    }
}
