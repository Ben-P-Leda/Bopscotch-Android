using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Gamestate_Management;
using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Game_Objects.Controllers.Collisions;
using Leda.Core.Timing;
using Leda.Core.Asset_Management;
using Leda.Core.Serialization;

using Bopscotch.Data;
using Bopscotch.Effects;
using Bopscotch.Effects.Particles;
using Bopscotch.Effects.SmashBlockItems;
using Bopscotch.Gameplay;
using Bopscotch.Gameplay.Controllers;
using Bopscotch.Gameplay.Objects.Behaviours;
using Bopscotch.Gameplay.Objects.Display;
using Bopscotch.Gameplay.Objects.Environment.Blocks;
using Bopscotch.Gameplay.Objects.Characters.Player;
using Bopscotch.Scenes.NonGame;

namespace Bopscotch.Scenes.Gameplay
{
    public class GameplaySceneBase : StorableScene
    {
        protected Input.TouchControls _inputProcessor;

        private MotionController _motionController;
        private AnimationController _animationController;
        private OneToManyCollisionController _playerCollisionController;
        private PauseController _pauseController;

        private List<ICanHaveGlowEffect> _objectWithGlowEffect;

        private LevelFactory _levelFactory;
        private SmashBlockItemFactory _smashBlockItemFactory;

        protected TimerController _timerController;
        protected AdditiveLayerParticleEffectManager _additiveParticleEffectManager;
        protected OpaqueLayerParticleEffectManager _opaqueParticleEffectManager; 
        protected PlayerTrackingCameraController _cameraController;
        protected Speedometer _speedometer;
        protected Player _player;
        protected LevelData _levelData;
        protected StatusDisplay _statusDisplay;
        protected PlayerEventPopup _playerEventPopup;

        protected string RaceAreaName { set { _levelFactory.RaceAreaName = value; } }

        protected bool _paused { get { return _pauseController.Paused; } set { _pauseController.Paused = value; } }

        public GameplaySceneBase(string sceneID)
            : base(sceneID)
        {
            _inputProcessor = Input.TouchControls.CreateController();

            _motionController = new MotionController();
            _animationController = new AnimationController();
            _timerController = new TimerController();

            _pauseController = new PauseController();
            _pauseController.AddPausableObject(_timerController);
            _pauseController.AddPausableObject(_animationController);

            _cameraController = new Bopscotch.Gameplay.Controllers.PlayerTrackingCameraController();
            _cameraController.Viewport = new Rectangle(0, 0, Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height);
            _cameraController.ScrollBoundaryViewportFractions = new Vector2(Definitions.Horizontal_Scroll_Boundary_Fraction, Definitions.Vertical_Scroll_Boundary_Fraction);

            Renderer.ClipOffCameraRendering(_cameraController, Camera_Clipping_Margin);

            _playerCollisionController = new OneToManyCollisionController();

            _opaqueParticleEffectManager = new OpaqueLayerParticleEffectManager(_cameraController);
            _additiveParticleEffectManager = new AdditiveLayerParticleEffectManager(_cameraController);

            _levelFactory = new Bopscotch.Gameplay.LevelFactory(RegisterGameObject, _timerController.RegisterUpdateCallback);

            _smashBlockItemFactory = new Effects.SmashBlockItems.SmashBlockItemFactory(RegisterGameObject, _timerController.RegisterUpdateCallback);

            _speedometer = new Bopscotch.Gameplay.Objects.Display.Speedometer();
            _playerEventPopup = new Bopscotch.Gameplay.Objects.Display.PlayerEventPopup();

            _objectWithGlowEffect = new List<ICanHaveGlowEffect>();
        }

        protected override void RegisterGameObject(IGameObject toRegister)
        {
            if (toRegister is IMobile) { _motionController.AddMobileObject((IMobile)toRegister); }
            if (toRegister is IAnimated) { _animationController.AddAnimatedObject((IAnimated)toRegister); }
            if (toRegister is ICameraLinked) { _cameraController.AddCameraLinkedObject((ICameraLinked)toRegister); }
            if (toRegister is ICollidable) { _playerCollisionController.AddCollidableObject((ICollidable)toRegister); }
            if (toRegister is IPausable) { _pauseController.AddPausableObject((IPausable)toRegister); }

            if (toRegister is SmashBlock) { ((SmashBlock)toRegister).SmashCallback = HandleSmashBlockSmash; }
            if (toRegister is ICanHaveGlowEffect) { _objectWithGlowEffect.Add((ICanHaveGlowEffect)toRegister); }

            base.RegisterGameObject(toRegister);
        }

        protected virtual void HandleSmashBlockSmash(SmashBlock smashedBlock)
        {
            _opaqueParticleEffectManager.LaunchCrateSmash(smashedBlock);
            _smashBlockItemFactory.CreateItemsForSmashBlock(smashedBlock);
        }

        protected override void UnregisterGameObject(IGameObject toUnregister)
        {
            if (toUnregister is IMobile) { _motionController.RemoveMobileObject((IMobile)toUnregister); }
            if (toUnregister is IAnimated) { _animationController.RemoveAnimatedObject((IAnimated)toUnregister); }
            if (toUnregister is ICameraLinked) { _cameraController.RemoveCameraLinkedObject((ICameraLinked)toUnregister); }
            if (toUnregister is ICollidable) { _playerCollisionController.RemoveCollidableObject((ICollidable)toUnregister); }
            if (toUnregister is IPausable) { _pauseController.RemovePausableObject((IPausable)toUnregister); }

            if (toUnregister is ICanHaveGlowEffect) { _objectWithGlowEffect.Remove((ICanHaveGlowEffect)toUnregister); }

            base.UnregisterGameObject(toUnregister);
        }

        public override void Initialize()
        {
            base.Initialize();
            _cameraController.Overspill = GameBase.WorldSpaceClipping;
        }

        public override void HandleAssetLoadCompletion(Type loaderSceneType)
        {
            base.HandleAssetLoadCompletion(loaderSceneType);

            if (loaderSceneType == typeof(StartupLoadingScene)) { CompletePostStartupLoadInitialization(); }

            _speedometer.CenterPosition = new Vector2(GameBase.SafeDisplayArea.X + GameBase.SafeDisplayArea.Width - _speedometer.Origin.X, _speedometer.Origin.Y);
            _statusDisplay.Position = new Vector2(GameBase.SafeDisplayArea.X, 0.0f);
        }

        protected virtual void CompletePostStartupLoadInitialization()
        {
            _speedometer.Initialize();
        }

        public override void Activate()
        {
            FlushGameObjects();
            _animationController.FlushAnimatedObjects();
            GC.Collect();

            RegisterStaticGameObjects();

            _pauseController.Paused = false;
            _levelFactory.AnimationController = _animationController;
            _levelFactory.SmashBlockCallback = HandleSmashBlockSmash;
            _levelFactory.SmashBlockRegenerationCallback = _additiveParticleEffectManager.LaunchCloudBurst;
            _levelFactory.BombBlockDetonationCallback = _additiveParticleEffectManager.LaunchFireball;

            base.Activate();

            if (!RecoveredFromTombstone) { SetForNewLevelStart(); }
            else { HandleTombstoneRecoveryCompletion(); }

            _player = _levelFactory.Player;
            _player.PlayerEventCallback = HandlePlayerEvent;
            _player.InputProcessor = _inputProcessor;
            _player.CollisionController = _playerCollisionController;
            _player.DeathByFallingThreshold = _levelFactory.Map.MapWorldDimensions.Y;

            _cameraController.WorldDimensions = _levelFactory.Map.MapWorldDimensions;
            _cameraController.PlayerToTrack = _player;
            _cameraController.PositionForPlayStart();

            _playerCollisionController.ObjectToTest = _player;

            SetInterfaceDisplayObjectsForGame();
        }

        protected virtual void RegisterStaticGameObjects()
        {
            RegisterGameObject(_levelFactory);
            RegisterGameObject(_opaqueParticleEffectManager);
            RegisterGameObject(_additiveParticleEffectManager);

            RegisterGameObject(_speedometer);
            RegisterGameObject(_playerEventPopup);
            RegisterGameObject(_statusDisplay);
        }

        private void SetForNewLevelStart()
        {
            _levelFactory.LoadAndInitializeLevel();

            if (Data.Profile.PlayingRaceMode) { ((Data.RaceLevelData)_levelData).LapsToComplete = _levelFactory.RaceLapCount; }
        }

        protected virtual void HandlePlayerEvent()
        {
            switch (_player.LastEvent)
            {
                case Player.PlayerEvent.Died:
                    _additiveParticleEffectManager.LaunchCloudBurst(_player);
                    _playerEventPopup.StartPopupForEvent(_player.LastEvent);
                    break;
            }
        }

        protected virtual void SetInterfaceDisplayObjectsForGame()
        {
            _speedometer.PlayerMotionEngine = (PlayerMotionEngine)_player.MotionEngine;
            _speedometer.Reset();
        }

        public override void HandleTombstone()
        {
            Data.Profile.Save();
            base.HandleTombstone();
        }

        protected virtual void HandleTombstoneRecoveryCompletion()
        {
        }

        protected override void HandlePostDeserializationResaturation()
        {
            TextureManager.ResaturateObjectTextures();
        }

        public override void Update(GameTime gameTime)
        {
            if (!string.IsNullOrEmpty(_inputProcessor.LastInGameButtonPressed)) { HandleInGameButtonPress(); }

            base.Update(gameTime);

            _inputProcessor.Update(MillisecondsSinceLastUpdate);

            _motionController.Update(MillisecondsSinceLastUpdate);
            _cameraController.Update(MillisecondsSinceLastUpdate);
            _timerController.Update(MillisecondsSinceLastUpdate);

            for (int i = 0; i < _objectWithGlowEffect.Count; i++) { _objectWithGlowEffect[i].UpdateGlow(MillisecondsSinceLastUpdate); }

            if (!_pauseController.Paused)
            {
                _opaqueParticleEffectManager.Update(MillisecondsSinceLastUpdate);
                _additiveParticleEffectManager.Update(MillisecondsSinceLastUpdate);
                _animationController.Update(MillisecondsSinceLastUpdate);
                _playerCollisionController.CheckForCollisions();
            }
        }

        protected virtual void HandleInGameButtonPress()
        {
        }

        protected override void CompleteDeactivation()
        {
            _levelFactory.Map.ClearDownBombBlocks();
            base.CompleteDeactivation();
        }

        private const int Camera_Clipping_Margin = 160;
    }
}
