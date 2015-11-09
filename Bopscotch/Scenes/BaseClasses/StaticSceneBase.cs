using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;
using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Game_Objects.Behaviours;

using Bopscotch.Scenes.NonGame;
using Bopscotch.Gameplay.Objects.Environment;

namespace Bopscotch.Scenes.BaseClasses
{
    public abstract class StaticSceneBase : Scene
    {
        private string _backgroundTextureName;

        protected List<Input.InputProcessorBase> _inputProcessors;
        protected Background _background;

        protected string BackgroundTextureName { set { _backgroundTextureName = value; SetBackgroundTexture(); } }

        public StaticSceneBase()
            : base()
        {
            _inputProcessors = Data.Profile.Settings.AllControllers;
            _backgroundTextureName = "";
        }

        public override void HandleAssetLoadCompletion(Type loaderSceneType)
        {
            if (loaderSceneType == typeof(StartupLoadingScene)) { CompletePostStartupLoadInitialization(); }
        }

        protected virtual void CompletePostStartupLoadInitialization()
        {
            _background = new Bopscotch.Gameplay.Objects.Environment.Background();
            SetBackgroundTexture();
            RegisterGameObject(_background);
        }

        private void SetBackgroundTexture()
        {
            if ((_background != null) && (!string.IsNullOrEmpty(_backgroundTextureName)))
            {
                _background.TextureReference = _backgroundTextureName;
            }
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < _inputProcessors.Count; i++) { _inputProcessors[i].Update(MillisecondsSinceLastUpdate); }

            base.Update(gameTime);
        }
    }
}
