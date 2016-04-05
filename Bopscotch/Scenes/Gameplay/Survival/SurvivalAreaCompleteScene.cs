using System;
using System.Collections.Generic;
using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Gamestate_Management;
using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Timing;
using Leda.Core.Asset_Management;

using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Scenes.NonGame;
using Bopscotch.Data;
using Bopscotch.Interface;
using Bopscotch.Interface.Content;
using Bopscotch.Interface.Dialogs.Generic;
using Bopscotch.Gameplay.Objects.Environment;

namespace Bopscotch.Scenes.Gameplay.Survival
{
    public class SurvivalAreaCompleteScene : ContentSceneWithControlDialog
    {
        private AnimationController _animationController;
        private Effects.Popups.PopupRequiringDismissal _congratulationsPopup;
        private Timer _textFadeTimer;

        private SurvivalAreaCompleteContentFactory _contentFactory;

        private ShareExitDialog _shareExitDialog;

        private bool _displayReminderOnExit;

        public SurvivalAreaCompleteScene()
            : base()
        {
            _animationController = new AnimationController();

            _textFadeTimer = new Timer("");
            _textFadeTimer.ActionCompletionHandler = HandleTimerActionComplete;
            GlobalTimerController.GlobalTimer.RegisterUpdateCallback(_textFadeTimer.Tick);

            _congratulationsPopup = new Effects.Popups.PopupRequiringDismissal();
            _congratulationsPopup.DisplayPosition = new Vector2(Definitions.Back_Buffer_Center.X, 135.0f);
            _congratulationsPopup.AnimationCompletionHandler = HandlePopupAnimationComplete;

            _contentFactory = new SurvivalAreaCompleteContentFactory(RegisterGameObject);

            _shareExitDialog = new ShareExitDialog();
            _shareExitDialog.SelectionCallback = HandleDialogDismiss;
            Dialog = _shareExitDialog;

            Overlay.TintFraction = 0.75f;
        }

        private void HandleTimerActionComplete()
        {
            _contentFactory.ActivatePopups();
        }

        private void HandlePopupAnimationComplete()
        {
            if (_congratulationsPopup.AwaitingDismissal)
            {
                _textFadeTimer.NextActionDuration = Text_Fade_Up_Duration_In_Milliseconds;
                ContentVisible = true;
                SoundEffectManager.PlayEffect("generic-fanfare");
            }
        }

        protected override void RegisterGameObject(IGameObject toRegister)
        {
            if (toRegister is IAnimated) { _animationController.AddAnimatedObject((IAnimated)toRegister); }
            if (toRegister is ContentBase) { ((ContentBase)toRegister).FadeFraction = 0.0f; }
            base.RegisterGameObject(toRegister);
        }

        protected override void UnregisterGameObject(IGameObject toUnregister)
        {
            if (toUnregister is IAnimated) { _animationController.RemoveAnimatedObject((IAnimated)toUnregister); }
            base.UnregisterGameObject(toUnregister);
        }

        private void HandleDialogDismiss(string buttonCaption)
        {
            if (buttonCaption == "Exit")
            {
                ExitScene();
            }
        }

        protected override void CompletePostStartupLoadInitialization()
        {
            _congratulationsPopup.MappingName = Popup_Texture_Name;
        }

        public override void Activate()
        {
            _displayReminderOnExit = (Profile.FullVersionOnlyUnlocks.Count > 0);

            _congratulationsPopup.Reset();

            FlushGameObjects();
            CreateBackgroundForScene(Profile.CurrentAreaData.SelectionTexture, new int[] { 0, 1, 2 });
            RegisterGameObject(Overlay);
            RegisterGameObject(_congratulationsPopup);
            RegisterGameObject(_shareExitDialog);

            if ((ContentHasBeenUnlocked) && (Data.Profile.CurrentAreaData.Name != "Tutorial"))
            {
                _shareExitDialog.ShareOptionEnabled = true;
                _shareExitDialog.ShareDialogText = "Share your achievement for 10 extra lives!";
                _shareExitDialog.ShareDialogDefaultValue = "I completed the " + Data.Profile.CurrentAreaData.Name + " area in Bopscotch!";
                _shareExitDialog.SharePostText = "I just completed the " + Data.Profile.CurrentAreaData.Name + " area in Bopscotch! Ddownload Bopscotch now for FREE on Android, iPhone, iPad and Windows Phone and see if you can do it too - or challenge me to a race!";
                _shareExitDialog.AwardShareReward = AwardLivesForSharing;
            }
            else
            {
                _shareExitDialog.ShareOptionEnabled = false;
            }

            _contentFactory.CreateContentForHeaderMessage();

            if (CurrentAreaHasUnlockableContent)
            {
                _contentFactory.CreateContentForUnlockableItems();
                if (ContentHasBeenUnlocked) { SwitchAreaToFirstNewUnlocked(); }
            }
            else
            {
                _contentFactory.CreateMessageContentForAreaWithNoUnlockables();
            }

            ContentFadeFraction = 0.0f;
            ContentVisible = false;

            SoundEffectManager.PlayEffect("race-winner");

            base.Activate();
        }

        private void AwardLivesForSharing()
        {
            Data.Profile.Lives += 10;
            Data.Profile.Save();
        }

        private bool CurrentAreaHasUnlockableContent
        {
            get { return (Profile.LockedContent.Count + Profile.UnlockedContent.Count + Profile.NewlyUnlockedContent.Count) > 0; }
        }

        private bool ContentHasBeenUnlocked
        {
            get { return (Profile.NewlyUnlockedContent.Count > 0); }
        }

        private void SwitchAreaToFirstNewUnlocked()
        {
            foreach (XElement el in Profile.NewlyUnlockedContent)
            {
                if (el.Attribute("type").Value == "area") { Profile.SelectedAreaName = el.Attribute("name").Value; break; }
            }
        }

        protected override void CompleteActivation()
        {
            base.CompleteActivation();

            _congratulationsPopup.Activate();
        }

        public override void Update(GameTime gameTime)
        {
            if (_congratulationsPopup.AwaitingDismissal) { ContentFadeFraction = _textFadeTimer.CurrentActionProgress; }

            _animationController.Update(MillisecondsSinceLastUpdate);
            base.Update(gameTime);
        }

        private void CheckAndHandleClear()
        {
            if ((CurrentState == Status.Active) && (_congratulationsPopup.AwaitingDismissal) && (_textFadeTimer.CurrentActionProgress == 1.0f))
            {
                for (int i = 0; i < _inputProcessors.Count; i++)
                {
                    if (_inputProcessors[i].SelectionTriggered) { ExitScene(); break; }
                }
            }
        }

        private void ExitScene()
        {
            if (_displayReminderOnExit) { NextSceneParameters.Set(TitleScene.First_Dialog_Parameter_Name, "reminder"); }
            else { NextSceneParameters.Set(TitleScene.First_Dialog_Parameter_Name, "survival-levels"); }

            NextSceneType = typeof(TitleScene);
            Deactivate();
        }

        private const string Popup_Texture_Name = "popup-area-complete";
        private const int Text_Fade_Up_Duration_In_Milliseconds = 250;
    }
}
