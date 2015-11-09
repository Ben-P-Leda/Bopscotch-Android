using System;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Timing;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Game_Objects.Controllers.Rendering;
using Leda.Core.Asset_Management;

namespace Leda.Core.Gamestate_Management
{
    public abstract class Scene : DrawableGameComponent
    {
        public delegate void DeactivationHandlerFunction(Type nextSceneType);
        public delegate void ObjectRegistrationHandler(IGameObject toRegister);
        public delegate void ObjectUnregistrationHandler(IGameObject toUnRegister);

        private List<IGameObject> _gameObjects;
        private List<ITemporary> _temporaryObjects;

        private Timer _transitionTimer;
        private Type _nextSceneType;
        private TimeSpan _lastUpdateTime;

        protected SpriteBatch SpriteBatch { get; private set; }
        protected RenderController Renderer { get; private set; }
        protected virtual Type NextSceneType { set { _nextSceneType = value; } }
        protected SceneParameters NextSceneParameters { get { return SceneParameters.Instance; } }
        protected Color ClearColour { private get; set; }
        protected float CrossfadeDuration { private get; set; }

        public Status CurrentState { get; private set; }
        public int MillisecondsSinceLastUpdate { get; private set; }

        public string CrossFadeTextureName { private get; set; }
        public DeactivationHandlerFunction DeactivationHandler { get; set; }

        public Scene()
            : base(GameBase.Instance)
        {
            DeactivationHandler = null;

            SpriteBatch = null;
            Renderer = new RenderController();
            CurrentState = Status.Inactive;
            ClearColour = Color.Black;
            CrossfadeDuration = Default_Crossfade_Duration;
            CrossFadeTextureName = "";

            _transitionTimer = new Timer("scene.transitiontimer", HandleTransitionCompletion);
            _nextSceneType = null;

            _gameObjects = new List<IGameObject>();
            _temporaryObjects = new List<ITemporary>();

            GameBase.Instance.Components.Add(this);
            Enabled = false;
            Visible = false;

            GlobalTimerController.GlobalTimer.RegisterUpdateCallback(_transitionTimer.Tick);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            SpriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public override void Update(GameTime gameTime)
        {
            if (_lastUpdateTime != TimeSpan.Zero)
            {
                TimeSpan difference = (gameTime.TotalGameTime - _lastUpdateTime);
                MillisecondsSinceLastUpdate = difference.Milliseconds + (difference.Seconds / 1000);
            }

            if (GamePad.GetState(0).Buttons.Back == ButtonState.Pressed) { HandleBackButtonPress(); }

            RemoveDisposedObjects();

            base.Update(gameTime);

            _lastUpdateTime = gameTime.TotalGameTime;
        }

        protected virtual void Render()
        {
            Renderer.RenderObjects(SpriteBatch);
        }

        public override void Draw(GameTime gameTime)
        {
			if (GraphicsDevice != null)
			{
				GraphicsDevice.Clear (ClearColour);
				Render ();

				if (CurrentState == Status.Activating) {
					OverlayCrossfader (1.0f - _transitionTimer.CurrentActionProgress);
				} else if (CurrentState == Status.Deactivating) {
					OverlayCrossfader (_transitionTimer.CurrentActionProgress);
				}
			}

            base.Draw(gameTime);
        }

        private void OverlayCrossfader(float opacity)
        {
            if (!string.IsNullOrEmpty(CrossFadeTextureName))
            {
                SpriteBatch.Begin();
                SpriteBatch.Draw(TextureManager.Textures[CrossFadeTextureName], GraphicsDevice.Viewport.Bounds, Color.Lerp(Color.Transparent, Color.Black, opacity));
                SpriteBatch.End();
            }
        }

        private void HandleTransitionCompletion()
        {
            if (CurrentState == Status.Activating) { CompleteActivation(); }
            else if (CurrentState == Status.Deactivating) { CompleteDeactivation(); }
        }

        protected virtual void HandleBackButtonPress()
        {
        }

        public virtual void Activate()
        {
            Reset();

            if (CrossfadeDuration <= 0.0f)
            {
                CompleteActivation();
            }
            else
            {
                CurrentState = Status.Activating;
                _transitionTimer.NextActionDuration = CrossfadeDuration;
                _transitionTimer.ActionSpeed = 1.0f;
            }

            Enabled = true;
            Visible = true;
        }

        protected virtual void CompleteActivation()
        {
            NextSceneParameters.Clear();
            CurrentState = Status.Active;
        }

        protected void Deactivate()
        {
            if (CrossfadeDuration <= 0.0f)
            {
                CompleteDeactivation();
            }
            else
            {
                CurrentState = Status.Deactivating;
                _transitionTimer.NextActionDuration = CrossfadeDuration;
                _transitionTimer.ActionSpeed = 1.0f;
            }
        }

        protected virtual void CompleteDeactivation()
        {
            Enabled = false;
            Visible = false;

            CurrentState = Status.Inactive;

            for (int i = _temporaryObjects.Count - 1; i >= 0; i--) { _temporaryObjects[i].ReadyForDisposal = true; }
            RemoveDisposedObjects();

            if ((_nextSceneType != null) && (DeactivationHandler != null)) { DeactivationHandler(_nextSceneType); }
            _nextSceneType = null;
        }

        protected virtual void RegisterGameObject(IGameObject toRegister)
        {
            if (!_gameObjects.Contains(toRegister)) { _gameObjects.Add(toRegister); }
            if (toRegister is ITemporary) { _temporaryObjects.Add((ITemporary)toRegister); }
            if (toRegister is ISimpleRenderable) { Renderer.AddRenderableObject((ISimpleRenderable)toRegister); }
        }

        protected virtual void UnregisterGameObject(IGameObject toUnregister)
        {
            if (_gameObjects.Contains(toUnregister)) { _gameObjects.Remove(toUnregister); }
            if (toUnregister is ITemporary) { _temporaryObjects.Remove((ITemporary)toUnregister); }
            if (toUnregister is ISimpleRenderable) { Renderer.RemoveRenderableObject((ISimpleRenderable)toUnregister); }
        }

        protected void FlushGameObjects()
        {
            for (int i = _gameObjects.Count - 1; i >= 0; i--) { UnregisterGameObject(_gameObjects[i]); }
        }

        public List<IGameObject> GameObjects(Type toGet)
        {
            List<IGameObject> objects = new List<IGameObject>();
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                if (_gameObjects[i].GetType() == toGet) { objects.Add(_gameObjects[i]); }
            }
            return objects;
        }

        private void RemoveDisposedObjects()
        {
            for (int i = _temporaryObjects.Count - 1; i >= 0; i--)
            {
                if (_temporaryObjects[i].ReadyForDisposal)
                {
                    _temporaryObjects[i].PrepareForDisposal();
                    UnregisterGameObject((IGameObject)_temporaryObjects[i]); 
                }
            }
        }

        protected void InitializeGameObjects()
        {
            for (int i = 0; i < _gameObjects.Count; i++) { _gameObjects[i].Initialize(); }
        }

        protected virtual void Reset()
        {
            MillisecondsSinceLastUpdate = 0;
            _lastUpdateTime = TimeSpan.Zero;

            for (int i = 0; i < _gameObjects.Count; i++) { _gameObjects[i].Reset();  }
        }

        public virtual void HandleAssetLoadCompletion(Type loaderSceneType)
        {
        }

        public virtual void HandleTombstone()
        {
        }

		// Used by Windows Phone version to ensure that fast resume is handled correctly
        public virtual void HandleFastResume()
        {
        }

        public virtual void HandleGameObscured()
        {
        }

        public virtual void HandleGameUnobscured()
        {
        }

		// Used by iOS to implement any custom behaviour on home key etc
		public virtual void HandleGameActivated()
		{
		}

		public virtual void HandleGameResigned()
		{
		}

		public virtual void HandleGameReturnedToForeground()
		{
		}

		public virtual void HandleGameSentToBackground()
		{
		}

        public enum Status
        {
            Inactive,
            Activating,
            Active,
            Deactivating
        }

        protected const float Default_Crossfade_Duration = 350.0f;
    }
}
