using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Timing;
using Leda.Core.Asset_Management;

using Bopscotch.Gameplay.Coordination;
using Bopscotch.Gameplay.Objects.Characters.Player;
using Bopscotch.Gameplay.Objects.Environment;
using Bopscotch.Gameplay.Objects.Environment.Blocks;
using Bopscotch.Gameplay.Objects.Display.Race;
using Bopscotch.Interface.Dialogs;
using Bopscotch.Interface.Dialogs.RaceGameplayScene;
using Bopscotch.Scenes.NonGame;

namespace Bopscotch.Scenes.Gameplay.Race
{
    public class RaceGameplayScene : GameplaySceneBase
    {
        private Communication.InterDeviceCommunicator _communicator;
        private CountdownPopup _countdownPopup;
        private RaceInfoPopup _positionStatusPopup;
        private RaceInfoPopup _raceEventPopup;
        private WaitingMessage _waitingMessage;
        private bool _raceStarted;
        private PowerUpButton _powerUpButton;
        private PowerUpTimer _powerUpDisplayTimer;
        private PowerUpHelper _powerUpHelper;
        private Blackout _blackout;
        private Timer _exitTimer;
        private QuitRaceButton _quitRaceButton;
        private QuitRaceDialog _quitRaceDialog;
        private Timer _quitTimer;
        private DisconnectedDialog _disconnectedDialog;

        private RaceProgressCoordinator _progressCoordinator;
        private RacePowerUpCoordinator _powerUpCoordinator;

        private int _startCoundown;
        private Timer _startSequenceTimer;

        public Communication.ICommunicator Communicator 
        { 
            set { if (value is Communication.InterDeviceCommunicator) { _communicator = (Communication.InterDeviceCommunicator)value; } } 
        }

        public bool AllLapsCompleted { get { return (_progressCoordinator.LapsCompleted >= LevelData.LapsToComplete); } }

        private Data.RaceLevelData LevelData { get { return (Data.RaceLevelData)_levelData; } }
        private RaceDataDisplay StatusDisplay { get { return (RaceDataDisplay)_statusDisplay; } set { _statusDisplay = value; } }

        public RaceGameplayScene()
            : base("race-gameplay")
        {
            StatusDisplay = new RaceDataDisplay();

            _countdownPopup = new CountdownPopup();
            _quitRaceButton = new QuitRaceButton();
            _powerUpButton = new PowerUpButton();
            _playerEventPopup.AnimationCompletionHandler = HandlePlayerEventAnimationComplete;

            _powerUpDisplayTimer = new PowerUpTimer();
            _powerUpDisplayTimer.TickCallback = _timerController.RegisterUpdateCallback;
            _powerUpHelper = new PowerUpHelper();
            _blackout = new Blackout();
            _blackout.TickCallback = _timerController.RegisterUpdateCallback;

            _positionStatusPopup = new RaceInfoPopup();
            _raceEventPopup = new RaceInfoPopup();
            _waitingMessage = new WaitingMessage();

            _exitTimer = new Timer("", HandleExitTimerActionComplete);
            GlobalTimerController.GlobalTimer.RegisterUpdateCallback(_exitTimer.Tick);

            _startSequenceTimer = new Timer("", HandleStartCountdownStep);
            GlobalTimerController.GlobalTimer.RegisterUpdateCallback(_startSequenceTimer.Tick);

            _quitTimer = new Timer("", HandleQuitTimerActionComplete);
            GlobalTimerController.GlobalTimer.RegisterUpdateCallback(_quitTimer.Tick);

            _quitRaceDialog = new QuitRaceDialog();
            _quitRaceDialog.CancellationTimer = _quitTimer;
            _quitRaceDialog.InputSources.Add(_inputProcessor);

            _disconnectedDialog = new DisconnectedDialog();
            _disconnectedDialog.SelectionCallback = HandleDisconnectAcknowledge;
            _disconnectedDialog.InputSources.Add(_inputProcessor);
        }

        private void HandlePlayerEventAnimationComplete()
        {
            switch (_player.LastEvent)
            {
                case Player.PlayerEvent.Died:
                    System.GC.Collect();
                    ResurrectPlayer(); 
                    break;
            }
        }

        private void HandleExitTimerActionComplete()
        {
            if (CurrentState == Status.Active)
            {
                if (_progressCoordinator.Result == Definitions.RaceOutcome.OwnPlayerWin)
                {
                    AwardLivesForWin();
                }

                NextSceneParameters.Set(RaceFinishScene.Outcome_Parameter_Name, _progressCoordinator.Result);
                SwitchToResultsScene();
            }
        }

        private void AwardLivesForWin()
        {
            if (Data.Profile.Lives < Data.Profile.Race_Win_Lives_Max)
            {
                Data.Profile.Lives += Data.Profile.Race_Win_Lives_Reward;
                Data.Profile.Save();
                NextSceneParameters.Set("race-lives-awarded", true);
            }
        }

        private void SwitchToResultsScene()
        {
            _communicator.Active = false;

            NextSceneType = typeof(RaceFinishScene);
            Deactivate();
        }

        private void HandleStartCountdownStep()
        {
            if (_startCoundown-- > 0) 
            {
                _startSequenceTimer.NextActionDuration = 1001; 

                SoundEffectManager.PlayEffect("race-countdown"); 
            }
            else 
            {
                _raceStarted = true;

                SoundEffectManager.PlayEffect("race-start"); 
                MusicManager.PlayLoopedMusic("race-gameplay"); 
            }
        }

        private void HandleQuitTimerActionComplete()
        {
            if (_quitRaceDialog.Active)
            {
                _quitRaceDialog.DismissWithReturnValue("");
                _communicator.Message = "";

                NextSceneParameters.Set(RaceFinishScene.Outcome_Parameter_Name, Definitions.RaceOutcome.Incomplete);
                SwitchToResultsScene();
            }
        }

        private void HandleDisconnectAcknowledge(string buttonCaption)
        {
            Definitions.RaceOutcome outcome = AllLapsCompleted ? Definitions.RaceOutcome.OwnPlayerWin : Definitions.RaceOutcome.Incomplete;

            if (outcome == Definitions.RaceOutcome.OwnPlayerWin)
            {
                AwardLivesForWin();
            }

            NextSceneParameters.Set(RaceFinishScene.Outcome_Parameter_Name, outcome);
            SwitchToResultsScene();
        }

        public override void HandleAssetLoadCompletion(System.Type loaderSceneType)
        {
            base.HandleAssetLoadCompletion(loaderSceneType);

            _waitingMessage.Position = new Vector2(GameBase.SafeDisplayArea.X, GameBase.SafeDisplayArea.Y + GameBase.SafeDisplayArea.Height);

            _powerUpButton.Center = new Vector2(
                GameBase.SafeDisplayArea.X + GameBase.SafeDisplayArea.Width - _powerUpButton.Radius,
                GameBase.SafeDisplayArea.Height - _powerUpButton.Radius);

            _powerUpDisplayTimer.DisplayTopRight = new Vector2(GameBase.SafeDisplayArea.X + GameBase.SafeDisplayArea.Width, GameBase.SafeDisplayArea.Y);

            _raceEventPopup.DisplayPosition = new Vector2(
                Definitions.Back_Buffer_Center.X,
                GameBase.SafeDisplayArea.Y + (GameBase.SafeDisplayArea.Height * 0.25f));
            _positionStatusPopup.DisplayPosition = new Vector2(
                Definitions.Back_Buffer_Center.X,
                GameBase.SafeDisplayArea.Y + GameBase.SafeDisplayArea.Height - Position_Status_Popup_Bottom_Margin);
        }

        protected override void CompletePostStartupLoadInitialization()
        {
            base.CompletePostStartupLoadInitialization();

            _quitRaceButton.Initialize();
            _quitRaceButton.DisplayEdgePositions = new Vector2(GameBase.SafeDisplayArea.X + GameBase.SafeDisplayArea.Width, 0.0f);
            _inputProcessor.AddButtonArea(QuitRaceButton.In_Game_Button_Name, _quitRaceButton.Center, _quitRaceButton.Radius, true);

            _powerUpButton.Initialize();
            _powerUpButton.Center = new Vector2(
                GameBase.SafeDisplayArea.X + GameBase.SafeDisplayArea.Width - _powerUpButton.Radius,
                GameBase.SafeDisplayArea.Y + GameBase.SafeDisplayArea.Height - _powerUpButton.Radius);

            _powerUpDisplayTimer.Initialize();

            _inputProcessor.AddButtonArea(PowerUpButton.In_Game_Button_Name, _powerUpButton.Center, _powerUpButton.Radius, false);
        }

        public override void Activate()
        {
            _raceStarted = false;
            _levelData = new Data.RaceLevelData();

            RaceAreaName = NextSceneParameters.Get<string>(Course_Area_Parameter);

            base.Activate();

            if (RecoveredFromTombstone)
            {
                ExitFollowingFocusLoss();
            }
            else
            {
                _player.PlayerEventCallback = HandlePlayerEvent;
                ((PlayerMotionEngine)_player.MotionEngine).DifficultySpeedBoosterUnit = NextSceneParameters.Get<int>(Course_Speed_Parameter);

                SetCoordinatorsForRace(NextSceneParameters.Get<string>(Course_Area_Parameter));
                SetUpOpponentAttackEffects();
                _waitingMessage.Activate();
            }
        }

        protected override void RegisterStaticGameObjects()
        {
            base.RegisterStaticGameObjects();

            RegisterGameObject(_quitRaceButton);
        }

        protected override void SetInterfaceDisplayObjectsForGame()
        {
            base.SetInterfaceDisplayObjectsForGame();

            _quitRaceButton.Initialize();

            _powerUpButton.Reset();
            RegisterGameObject(_powerUpButton);

            _powerUpDisplayTimer.Reset();
            RegisterGameObject(_powerUpDisplayTimer);

            _powerUpHelper.Reset();
            _powerUpHelper.MotionLine = Definitions.Back_Buffer_Center.X;
            RegisterGameObject(_powerUpHelper);

            _countdownPopup.Reset();
            RegisterGameObject(_countdownPopup);

            _raceEventPopup.Reset();
            RegisterGameObject(_raceEventPopup);

            _positionStatusPopup.Reset();
            RegisterGameObject(_positionStatusPopup);

            _waitingMessage.Reset();
            RegisterGameObject(_waitingMessage);

            _quitRaceDialog.Reset();
            RegisterGameObject(_quitRaceDialog);

            _disconnectedDialog.Reset();
            RegisterGameObject(_disconnectedDialog);
        }

        private void SetCoordinatorsForRace(string selectedCourse)
        {
            _powerUpCoordinator = new RacePowerUpCoordinator();
            _powerUpCoordinator.Player = _player;
            _powerUpCoordinator.DisplayTimer = _powerUpDisplayTimer;

            _progressCoordinator = new RaceProgressCoordinator();
            _progressCoordinator.Communicator = _communicator;
            _progressCoordinator.Player = _player;
            _progressCoordinator.StatusPopup = _positionStatusPopup;
            _progressCoordinator.StatusDisplay = StatusDisplay;
            _progressCoordinator.LapsToComplete = LevelData.LapsToComplete;
            _progressCoordinator.SetRestartPoint();

            _timerController.RegisterUpdateCallback(_progressCoordinator.SequenceTimerTick);
            RegisterGameObject(_progressCoordinator);

            _communicator.OwnPlayerData = _progressCoordinator;
            _communicator.SetForRace(selectedCourse);
        }

        private void SetUpOpponentAttackEffects()
        {
            _blackout.Reset();
            RegisterGameObject(_blackout);
        }

        protected override void HandlePlayerEvent()
        {
            switch (_player.LastEvent)
            {
                case Player.PlayerEvent.Died:
                    ResetPowerUpDisplayFollowingPlayerDeath();
                    break;
                case Player.PlayerEvent.Restart_Point_Touched:
                    SoundEffectManager.PlayEffect("race-checkpoint");
                    _opaqueParticleEffectManager.LaunchFlagStars(_player.LastRaceRestartPointTouched);
                    _progressCoordinator.CheckAndUpdateRestartPoint();
                    break;
                case Player.PlayerEvent.Restart_Point_Changed_Direction:
                    _raceEventPopup.StartPopupForRaceInfo("popup-race-wrong-way");
                    break;
                case Player.PlayerEvent.Goal_Passed:
                    _progressCoordinator.CheckAndUpdateRestartPoint();
                    HandleLapCompleted();
                    break;
            }

            base.HandlePlayerEvent();
        }

        private void ResetPowerUpDisplayFollowingPlayerDeath()
        {
            if (_powerUpButton.Visible) { _powerUpButton.Deactivate(); }
            if (_powerUpDisplayTimer.Visible) { _powerUpDisplayTimer.Deactivate(); }
            if (_powerUpHelper.Visible) { _powerUpHelper.Dismiss(); }
        }

        private void HandleLapCompleted()
        {
            if (_progressCoordinator.LapCompleted())
            {
                SoundEffectManager.PlayEffect("generic-fanfare");
                if (AllLapsCompleted) { HandleRaceGoalAchieved(); }

                _opaqueParticleEffectManager.LaunchFlagStars(_player.LastRaceRestartPointTouched);
                if (_progressCoordinator.LapsCompleted == LevelData.LapsToComplete) { _raceEventPopup.StartPopupForRaceInfo("popup-race-goal"); }
                else if (_progressCoordinator.LapsCompleted + 1 == LevelData.LapsToComplete) { _raceEventPopup.StartPopupForRaceInfo("popup-race-last-lap"); }
            }
        }


        private void HandleRaceGoalAchieved()
        {
            if (_exitTimer.CurrentActionProgress == 1.0f) { _exitTimer.NextActionDuration = Exit_Sequence_Duration_In_Milliseconds; }
            _player.CanMoveHorizontally = false;
            _player.IsExitingLevel = true;
            _progressCoordinator.CompleteRace();
        }

        private void ResurrectPlayer()
        {
            _progressCoordinator.ResurrectPlayerAtLastRestartPoint();
            _cameraController.PositionForPlayStart();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!_raceStarted) { HandlePreRaceEvents(); }

            _progressCoordinator.Update(MillisecondsSinceLastUpdate);
            if (_progressCoordinator.OpponentAttackPending) { SetUpOpponentAttack(); }
            HandleCommunications();
        }

        private void HandlePreRaceEvents()
        {
            if ((_cameraController.LockedOntoStartPoint) && ((_communicator.OtherPlayerData.ReadyToRace) || (Data.Profile.Settings.TestingRaceMode)))
            {
                if (_waitingMessage.Visible) { _waitingMessage.HideAfterCurrentAnimationCycleComplete(); }
                if (!_countdownPopup.Running) { StartCountdown(); }
            }
        }

        private void SetUpOpponentAttack()
        {
            SoundEffectManager.PlayEffect("opponent-attack");
            if (!_player.IsDead) { _additiveParticleEffectManager.LaunchEnemyAttack(_player); }

            switch (_progressCoordinator.NextOpponentAttackPowerUp)
            {
                case Definitions.PowerUp.Shades: _blackout.Activate(); break;
                case Definitions.PowerUp.Shell: _player.SetActivePowerUp(Definitions.PowerUp.Shell); break;
                case Definitions.PowerUp.Horn: _player.SetActivePowerUp(Definitions.PowerUp.Horn); break;
            }

            _progressCoordinator.SetLastOpponentAttackTime();
        }

        private void HandleCommunications()
        {
            _communicator.Update();

            if (!_communicator.ConnectionLost) { CheckAndHandleOpponentUpdates(); }
            else { HandleCommunicationLoss(); }
        }

        private void CheckAndHandleOpponentUpdates()
        {
            if ((_exitTimer.CurrentActionProgress == 1.0f) && (_communicator.OtherPlayerData.LapsCompleted >= LevelData.LapsToComplete))
            {
                _exitTimer.NextActionDuration = Exit_Sequence_Duration_In_Milliseconds -
                    (_communicator.OtherPlayerData.TotalRaceTimeElapsedInMilliseconds - _communicator.OtherPlayerData.LastCheckpointTimeInMilliseconds);
            }
        }

        private void HandleCommunicationLoss()
        {
            if ((CurrentState == Status.Active) && (!_paused) && (!Data.Profile.Settings.TestingRaceMode))
            {
                _paused = true;
                _disconnectedDialog.Activate();
            }
        }

        protected override void HandleInGameButtonPress()
        {
            if ((_inputProcessor.LastInGameButtonPressed == PowerUpButton.In_Game_Button_Name) && (_powerUpButton.Visible))
            {
                SoundEffectManager.PlayEffect("power-up");
                _opaqueParticleEffectManager.LaunchPowerUpStars(_player, _powerUpCoordinator.CurrentPowerUpAttacksOpponent);
                if (_powerUpCoordinator.CurrentPowerUpAttacksOpponent) { _progressCoordinator.StartAttackSequence(_powerUpCoordinator.AvailablePowerUp); }
                _powerUpCoordinator.ActivateAvailablePowerUp();
                _powerUpButton.Deactivate();
                _powerUpHelper.Dismiss();
            }

            if (_inputProcessor.LastInGameButtonPressed == QuitRaceButton.In_Game_Button_Name) { StartQuitRaceSequence(); }
        }

        private void StartQuitRaceSequence()
        {
            if ((CurrentState == Status.Active) && (!_quitRaceDialog.Active))
            {
                _quitRaceDialog.Activate();
                _quitTimer.NextActionDuration = QuitRaceDialog.Cancellation_Duration_In_Milliseconds;
            }
        }

        private void StartCountdown()
        {
            _progressCoordinator.StartTimerToBeginningOfRace();

            _startCoundown = 3;
            _startSequenceTimer.NextActionDuration = 501;

            _countdownPopup.Activate();
        }

        protected override void HandleSmashBlockSmash(SmashBlock smashedBlock)
        {
            if (smashedBlock.Contents.Count > 0)
            {
                _powerUpButton.IconTexture = smashedBlock.Contents[0].TextureName;
                _powerUpCoordinator.SetAvailablePowerUpFromTexture(smashedBlock.Contents[0].TextureName);

                if (Data.Profile.Settings.ShowPowerUpHelpers)
                {
                    _powerUpHelper.SetHelpText(smashedBlock.Contents[0].TextureName);
                    _powerUpHelper.Activate();
                }

                _inputProcessor.ActivateButton(PowerUpButton.In_Game_Button_Name);
            }
            base.HandleSmashBlockSmash(smashedBlock);
        }

        protected override void HandleBackButtonPress()
        {
            if ((CurrentState == Status.Active) && (_quitRaceDialog.Active)) { _quitRaceDialog.DismissWithReturnValue("cancel"); }
            else { StartQuitRaceSequence(); }
        }

        protected override void CompleteDeactivation()
        {
            MusicManager.StopMusic();
            base.CompleteDeactivation();
        }

        public override void HandleGameObscured()
        {
            base.HandleGameObscured();

            ExitFollowingFocusLoss();
        }

        public override void HandleFastResume()
        {
            ExitFollowingFocusLoss();
        }

#if IOS
		public override void HandleGameResigned()
		{
			ExitFollowingFocusLoss();
		}
#endif

        private void ExitFollowingFocusLoss()
        {
            _communicator.Active = false;

            NextSceneParameters.Set(TitleScene.First_Dialog_Parameter_Name, TitleScene.Race_Aborted_Dialog);
            NextSceneType = typeof(TitleScene);
            CrossfadeDuration = 0;
            Deactivate();
            CrossfadeDuration = Default_Crossfade_Duration;
        }

        private const int Exit_Sequence_Duration_In_Milliseconds = 3500;
        private const float Position_Status_Popup_Bottom_Margin = 100.0f;

        public const string Course_Area_Parameter = "course-area-name";
        public const string Course_Speed_Parameter = "course-speed";
    }
}