using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Asset_Management;
using Leda.Core.Serialization;
using Leda.Core.Game_Objects.Behaviours;

using Bopscotch.Scenes.NonGame;
using Bopscotch.Data;
using Bopscotch.Gameplay.Objects.Display.Survival;
using Bopscotch.Gameplay.Objects.Characters.Player;
using Bopscotch.Gameplay.Objects.Environment.Blocks;
using Bopscotch.Gameplay.Objects.Environment.Collectables;
using Bopscotch.Effects.Popups;
using Bopscotch.Interface.Dialogs.SurvivalGameplayScene;

namespace Bopscotch.Scenes.Gameplay.Survival
{
    public class SurvivalGameplayScene : GameplaySceneBase
    {
        private PopupRequiringDismissal _readyPopup;
        private PauseButton _pauseButton;
        private PauseDialog _pauseDialog;
        private NoLivesDialog _noLivesDialog;
        private TutorialRunner _tutorialRunner;
        private bool _levelComplete;

        private SurvivalLevelData LevelData { get { return (SurvivalLevelData)_levelData; } }
        private SurvivalDataDisplay StatusDisplay { get { return (SurvivalDataDisplay)_statusDisplay; } set { _statusDisplay = value; } }

        public SurvivalGameplayScene()
            : base("survival-gameplay")
        {
            StatusDisplay = new SurvivalDataDisplay();

            _readyPopup = new PopupRequiringDismissal() { ID = "get-ready-popup" };
            _pauseButton = new PauseButton();
            _pauseDialog = new PauseDialog();
            _noLivesDialog = new NoLivesDialog();
            _tutorialRunner = new TutorialRunner();

            _pauseDialog.InputSources.Add(_inputProcessor);
            _pauseDialog.ExitCallback = HandleDialogClose;

            _noLivesDialog.InputSources.Add(_inputProcessor);
            _noLivesDialog.ExitCallback = HandleDialogClose;

            _playerEventPopup.AnimationCompletionHandler = HandlePlayerEventAnimationComplete;
        }

        private void HandleDialogClose(string selectedOption)
        {
            switch (selectedOption)
            {
                case "Continue":
                    if (!_tutorialRunner.DisplayingHelp) { _paused = false; }
                    break;
                case "Skip Level":
                    HandleLevelSkip();
                    break;
                case "Quit":
                    NextSceneParameters.Set(TitleScene.First_Dialog_Parameter_Name, "start");
                    NextSceneType = typeof(TitleScene); 
                    Profile.PauseOnSceneActivation = false; 
                    Deactivate(); 
                    break;
                case "Add Lives":
                    NextSceneParameters.Set("return-to-game", true);
                    NextSceneType = typeof(StoreScene);
                    Deactivate();
                    break;
            }
        }

        private void HandleLevelSkip()
        {
            if ((_readyPopup.Visible) && (!_readyPopup.BeingDismissed)) { _readyPopup.Dismiss(); }
            Profile.GoldenTickets--;
            _player.TriggerLevelSkip();
            _paused = false;
        }

        private void HandlePlayerEventAnimationComplete()
        {
            switch (_player.LastEvent)
            {
                case Player.PlayerEvent.Died: HandlePlayerDeath(); break;
                case Player.PlayerEvent.Goal_Passed: HandleLevelCleared(); break;
            }
        }

        private void HandlePlayerDeath()
        {
            Data.Profile.Save();

            if (Data.Profile.Lives < 1) { _noLivesDialog.Activate(); }
            else { RefreshScene(); }
        }

        private void RefreshScene()
        {
            NextSceneType = typeof(SurvivalGameplayScene); 
            Deactivate();
        }

        private void HandleLevelCleared()
        {
            StatusDisplay.FreezeDisplayedScore = true;
            Profile.CurrentAreaData.UpdateCurrentLevelScore(LevelData.PointsScoredThisLevel);
            Profile.CurrentAreaData.StepToNextLevel();
            Profile.Save();

            if (Profile.CurrentAreaData.Completed) { CompleteArea(); }
            else { RefreshScene(); }
        }

        private void CompleteArea()
        {
            Profile.UnlockCurrentAreaContent();
            NextSceneType = typeof(SurvivalAreaCompleteScene); 
            Deactivate();
        }

        protected override void RegisterGameObject(IGameObject toRegister)
        {
            base.RegisterGameObject(toRegister);

            if (toRegister is Collectable) { ((Collectable)toRegister).CollectionCallback = HandleCollectableCollection; }
        }

        private void HandleCollectableCollection(Collectable collectedItem)
        {
            LevelData.UpdateFromItemCollection(collectedItem);
        }

        public override void HandleAssetLoadCompletion(Type loaderSceneType)
        {
            base.HandleAssetLoadCompletion(loaderSceneType);
        }

        protected override void CompletePostStartupLoadInitialization()
        {
            base.CompletePostStartupLoadInitialization();

            _pauseButton.Initialize();
            _pauseButton.DisplayEdgePositions = new Vector2(GameBase.SafeDisplayArea.X + GameBase.SafeDisplayArea.Width, 0.0f);
            _inputProcessor.AddButtonArea(PauseButton.In_Game_Button_Name, _pauseButton.Center, _pauseButton.Radius, true);
        }

        public override void Activate()
        {
            Profile.SyncPlayerLives();

            _levelComplete = false;
            _levelData = new SurvivalLevelData();
            ObjectsToSerialize.Add(LevelData);
            StatusDisplay.CurrentLevelData = LevelData;
            StatusDisplay.FreezeDisplayedScore = false;
            RaceAreaName = "";

            base.Activate();

            if (Profile.PauseOnSceneActivation)
            {
                EnablePause();
                Profile.PauseOnSceneActivation = false;
            }

            if (!RecoveredFromTombstone) 
            {
                ((PlayerMotionEngine)_player.MotionEngine).DifficultySpeedBoosterUnit = Profile.CurrentAreaData.SpeedStep;
                _readyPopup.Activate(); 
            }
        }

        protected override void RegisterStaticGameObjects()
        {
            base.RegisterStaticGameObjects();

            RegisterGameObject(_pauseButton);
            RegisterGameObject(_readyPopup);
            RegisterGameObject(_pauseDialog);

            if (Profile.CurrentAreaData.Name == "Tutorial") { RegisterGameObject(_tutorialRunner); }
            else { RegisterGameObject(_noLivesDialog); }
        }

        protected override void HandlePlayerEvent()
        {
            switch (_player.LastEvent)
            {
                case Player.PlayerEvent.Goal_Passed:
                    _levelComplete = true;
                    _playerEventPopup.StartPopupForEvent(Player.PlayerEvent.Goal_Passed);
                    SoundEffectManager.PlayEffect("level-clear");
                    break;
            }

            base.HandlePlayerEvent();
        }

        protected override void SetInterfaceDisplayObjectsForGame()
        {
            base.SetInterfaceDisplayObjectsForGame();

            _pauseButton.Initialize();
            _readyPopup.MappingName = Ready_Popup_Texture;

            SetStatusAndTutorialDisplays();
        }

        public override void HandleFastResume()
        {
            if (!_paused) { AttemptToPauseGame(); }
            base.HandleFastResume();
        }

        public override void HandleGameObscured()
        {
            if (!_paused) { AttemptToPauseGame(); }
            base.HandleGameObscured();
        }

        protected override void HandleTombstoneRecoveryCompletion()
        {
            TextureManager.ResaturateObjectTextures();
            Profile.PauseOnSceneActivation = true;
        }

        private void SetStatusAndTutorialDisplays()
        {
            if (Profile.CurrentAreaData.Name == "Tutorial")
            {
                if (!RecoveredFromTombstone) { _tutorialRunner.SetForTombstoneRecovery(); }
                _tutorialRunner.Initialize();
                _tutorialRunner.DisplayArea = GameBase.SafeDisplayArea;
                _tutorialRunner.PauseTrigger = HoldForTutorialStep;
                _tutorialRunner.Visible = true;

                _player.TutorialStepTrigger = _tutorialRunner.CheckForStepTrigger;
                _statusDisplay.Visible = false;
            }
            else
            {
                _statusDisplay.Visible = true;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_inputProcessor.ActionTriggered) { HandleActionTrigger(); }

            UpdateScore(MillisecondsSinceLastUpdate);
        }

        protected override void HandleInGameButtonPress()
        {
            if (_inputProcessor.LastInGameButtonPressed == PauseButton.In_Game_Button_Name) { AttemptToPauseGame(); }
        }

        protected bool AttemptToPauseGame()
        {
            if ((!_paused || (_tutorialRunner.DisplayingHelp && !_pauseDialog.Active)) && (!_noLivesDialog.Active))
            {
                if (CurrentState == Status.Deactivating) { Profile.PauseOnSceneActivation = true; }
                else { EnablePause(); }

                return true;
            }

            return false;
        }

        private void EnablePause()
        {
            _paused = true;
            _pauseDialog.SkipLevelButtonDisabled = ((Profile.GoldenTickets < 1) || (Data.Profile.CurrentAreaData.Name == "Tutorial") || (_levelComplete));
            _pauseDialog.Activate();
        }

        private void HandleActionTrigger()
        {
            if (!_paused && _readyPopup.AwaitingDismissal) { BeginPlay(); }

            if (_paused && !_pauseDialog.Visible && _tutorialRunner.DisplayingHelp && _tutorialRunner.StepCanBeDismissed) 
            { 
                _paused = false;
                _tutorialRunner.ClearCurrentStep(); 
            }
        }

        private void BeginPlay()
        {
            _readyPopup.Dismiss();
            _tutorialRunner.ClearCurrentStep();
            _player.SetForMovement();
            _levelData.CurrentPlayState = Data.LevelData.PlayState.InPlay;
        }

        private void UpdateScore(int millisecondsSinceLastUpdate)
        {
            if ((!_paused) && (_player.CanMoveHorizontally))
            {
                LevelData.UpdateScoreForMovement(millisecondsSinceLastUpdate, ((PlayerMotionEngine)_player.MotionEngine).Speed);
            }
        }

        protected override Serializer Deserialize(Serializer serializer)
        {
            serializer.KnownSerializedObjects.Add(LevelData);
            serializer.KnownSerializedObjects.Add(_readyPopup);
            serializer.KnownSerializedObjects.Add(_playerEventPopup);

            return base.Deserialize(serializer);
        }

        protected override void HandleSmashBlockSmash(SmashBlock smashedBlock)
        {
            // TODO: add extra score etc

            LevelData.UpdateFromSmashBlockContents(smashedBlock);
            base.HandleSmashBlockSmash(smashedBlock);
        }

        protected override void HandleBackButtonPress()
        {
            if ((!AttemptToPauseGame()) && (_pauseDialog.Active)) { _pauseDialog.Cancel(); }
        }

        private void HoldForTutorialStep()
        {
            _paused = true;
        }

        protected override void CompleteDeactivation()
        {
            if (_nextSceneType != typeof(SurvivalGameplayScene)) { MusicManager.StopMusic(); }
            base.CompleteDeactivation();
        }

        private const string Ready_Popup_Texture = "popup-get-ready";
    }
}