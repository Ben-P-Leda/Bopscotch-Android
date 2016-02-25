using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Asset_Management;
using Leda.Core.Serialization;
using Leda.Core.Game_Objects.Behaviours;

using Bopscotch.Scenes.NonGame;
using Bopscotch.Data;
using Bopscotch.Gameplay;
using Bopscotch.Gameplay.Coordination;
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
        private SurvivalLevelData _levelData;

        private PopupRequiringDismissal _readyPopup;
        private PauseButton _pauseButton;
        private PauseDialog _pauseDialog;
        private NoLivesDialog _noLivesDialog;
        private TutorialRunner _tutorialRunner;
        private TutorialDialog _tutorialDialog;
        private SurvivalRankingCoordinator _rankingCoordinator;
        private bool _levelComplete;
        private int _attemptsAtCurrentLevel;

        private SurvivalDataDisplay StatusDisplay { get { return (SurvivalDataDisplay)_statusDisplay; } set { _statusDisplay = value; } }

        public SurvivalGameplayScene()
            : base("survival-gameplay")
        {
            StatusDisplay = new SurvivalDataDisplay();

            _readyPopup = new PopupRequiringDismissal() { ID = "get-ready-popup" };
            _pauseButton = new PauseButton();
            _pauseDialog = new PauseDialog();
            _noLivesDialog = new NoLivesDialog();
            _tutorialDialog = new TutorialDialog();
            _tutorialRunner = new TutorialRunner();
            _rankingCoordinator = new SurvivalRankingCoordinator(CloseCurrentLevel, RegisterGameObject);

            _pauseDialog.InputSources.Add(_inputProcessor);
            _pauseDialog.ExitCallback = HandleDialogClose;

            _noLivesDialog.InputSources.Add(_inputProcessor);
            _noLivesDialog.ExitCallback = HandleDialogClose;

            _tutorialDialog.InputSources.Add(_inputProcessor);
            _tutorialDialog.ExitCallback = HandleDialogClose;

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
                    NextSceneParameters.Set("music-already-running", false);
                    NextSceneType = typeof(TitleScene); 
                    Profile.PauseOnSceneActivation = false; 
                    Deactivate(); 
                    break;
                case "Add Lives":
                    NextSceneParameters.Set("return-to-game", true);
                    NextSceneType = typeof(StoreScene);
                    Deactivate();
                    break;
                case "OK":
                    _paused = false;
                    _tutorialRunner.ClearCurrentStep();

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

            if (Data.Profile.Lives < 1) 
            { 
                _noLivesDialog.Activate(); 
            }
            else 
            {
                _attemptsAtCurrentLevel++;
                RefreshScene(); 
            }
        }

        private void RefreshScene()
        {
            NextSceneParameters.Set("attempt-count", _attemptsAtCurrentLevel);
            NextSceneType = typeof(SurvivalGameplayScene); 
            Deactivate();
        }

        private void HandleLevelCleared()
        {
            StatusDisplay.FreezeDisplayedScore = true;
            _rankingCoordinator.LevelCompleted = true;

            Definitions.SurvivalRank rank = _rankingCoordinator.GetRankForLevel(_levelData);
            Profile.CurrentAreaData.UpdateCurrentLevelResults(_levelData.PointsScoredThisLevel, rank);
            Profile.Save();

            if (Profile.CurrentAreaData.Name == "Tutorial")
            {
                CloseCurrentLevel();
            }
            else
            {
                _rankingCoordinator.DisplayRanking(rank);
            }
        }

        private void CloseCurrentLevel()
        {
            Profile.CurrentAreaData.StepToNextLevel();
            Profile.Save();

            if (Profile.CurrentAreaData.Completed) 
            { 
                CompleteArea(); 
            }
            else 
            {
                _attemptsAtCurrentLevel = 0;
                RefreshScene(); 
            }
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
            _levelData.UpdateFromItemCollection(collectedItem);
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

            _rankingCoordinator.Initialize();
        }

        public override void Activate()
        {
            Profile.SyncPlayerLives();
            if (_levelData != null) { ObjectsToSerialize.Remove(_levelData); }

            _attemptsAtCurrentLevel = NextSceneParameters.Get<int>("attempt-count");

            _levelComplete = false;
            _rankingCoordinator.Reset();
            _levelData = new SurvivalLevelData();
            _levelData.AttemptsAtLevel = _attemptsAtCurrentLevel;

            ObjectsToSerialize.Add(_levelData);
            StatusDisplay.CurrentLevelData = _levelData;
            StatusDisplay.FreezeDisplayedScore = false;
            RaceAreaName = "";

            base.Activate();

            if (Profile.PauseOnSceneActivation)
            {
                if (!_rankingCoordinator.LevelCompleted)
                {
                    if (Profile.CurrentAreaData.Name == "Tutorial")
                    {
                        _tutorialRunner.CheckForStepTrigger(_player.WorldPosition);
                    }
                    if (!_paused)
                    {
                        EnablePause();
                    }
                }
                Profile.PauseOnSceneActivation = false;
            }

            if (!RecoveredFromTombstone) 
            {
                ((PlayerMotionEngine)_player.MotionEngine).DifficultySpeedBoosterUnit = Profile.CurrentAreaData.SpeedStep;
                _readyPopup.Activate(); 
            }
            else if (_rankingCoordinator.LevelCompleted)
            {
                HandleLevelCleared();
            }
        }

        protected override void SetLevelMetrics(LevelFactory levelFactory)
        {
            _levelData.TotalCandiesOnLevel = levelFactory.TotalCandiesOnLevel;
            _levelData.RankACandyFraction = levelFactory.RankACandyFraction;
            _levelData.RankBCandyFraction = levelFactory.RankBCandyFraction;
        }

        protected override void RegisterStaticGameObjects()
        {
            base.RegisterStaticGameObjects();

            RegisterGameObject(_pauseButton);
            RegisterGameObject(_readyPopup);
            RegisterGameObject(_pauseDialog);
            RegisterGameObject(_rankingCoordinator);

            if (Profile.CurrentAreaData.Name == "Tutorial") 
            { 
                RegisterGameObject(_tutorialDialog); 
            }
            else
            {
                RegisterGameObject(_noLivesDialog); 
            }
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
                _tutorialRunner.Initialize();
                _tutorialRunner.PauseTrigger = HoldForTutorialStep;

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
            if (_inputProcessor.LastInGameButtonPressed == PauseButton.In_Game_Button_Name)
            { 
                AttemptToPauseGame(); 
            }
        }

        protected bool AttemptToPauseGame()
        {
            if ((!_paused || (_tutorialRunner.DisplayingHelp && !_pauseDialog.Active)) && (!_noLivesDialog.Visible) && (!_tutorialDialog.Visible))
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
            if ((!_paused) && (_readyPopup.AwaitingDismissal)) { BeginPlay(); }
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
                _levelData.UpdateScoreForMovement(millisecondsSinceLastUpdate, ((PlayerMotionEngine)_player.MotionEngine).Speed);
            }
        }

        protected override Serializer Deserialize(Serializer serializer)
        {
            serializer.KnownSerializedObjects.Add(_levelData);
            serializer.KnownSerializedObjects.Add(_readyPopup);
            serializer.KnownSerializedObjects.Add(_playerEventPopup);

            return base.Deserialize(serializer);
        }

        protected override void HandleSmashBlockSmash(SmashBlock smashedBlock)
        {
            _levelData.UpdateFromSmashBlockContents(smashedBlock);
            base.HandleSmashBlockSmash(smashedBlock);
        }

        protected override void HandleBackButtonPress()
        {
            if ((!AttemptToPauseGame()) && (_pauseDialog.Active))
            {
                _pauseDialog.Cancel(); 
            }
            else if (_tutorialDialog.Active)
            {
                _tutorialDialog.Cancel();
            }
        }

        private void HoldForTutorialStep()
        {
            _tutorialDialog.StepText = _tutorialRunner.StepText;
            _tutorialDialog.Activate();
            _paused = true;
        }

        protected override void CompleteDeactivation()
        {
            if (_nextSceneType != typeof(SurvivalGameplayScene)) { MusicManager.StopMusic(); }
            base.CompleteDeactivation();
        }

        protected override void HandlePostDeserializationResaturation()
        {
            base.HandlePostDeserializationResaturation();

            _attemptsAtCurrentLevel = ((SurvivalLevelData)_levelData).AttemptsAtLevel;
        }

        private const string Ready_Popup_Texture = "popup-get-ready";
    }
}