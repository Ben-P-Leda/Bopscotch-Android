using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;
using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Game_Objects.Behaviours;

using Bopscotch.Scenes.NonGame;

namespace Bopscotch.Scenes.BaseClasses
{
    public abstract class MenuDialogScene : StaticSceneBase
    {
        private MotionController _motionController;

        protected string _lastActiveDialogName;
        protected Dictionary<string, Interface.Dialogs.ButtonDialog> _dialogs;

        public MenuDialogScene()
            : base()
        {
            _motionController = new MotionController();
            _dialogs = new Dictionary<string, Interface.Dialogs.ButtonDialog>();
        }

        protected override void CompletePostStartupLoadInitialization()
        {
            base.CompletePostStartupLoadInitialization();

            RegisterDialogs();
        }

        protected override void RegisterGameObject(IGameObject toRegister)
        {
            if (toRegister is IMobile) { _motionController.AddMobileObject((IMobile)toRegister); }
            base.RegisterGameObject(toRegister);
        }

        protected void RegisterDialogs()
        {
            foreach (KeyValuePair<string, Interface.Dialogs.ButtonDialog> kvp in _dialogs)
            {
                kvp.Value.InputSources = _inputProcessors;
                RegisterGameObject(kvp.Value);
            }
        }

        public override void Activate()
        {
            _lastActiveDialogName = "";
            base.Activate();
        }

        protected void ActivateDialog(string dialogToActivate)
        {
            _dialogs[dialogToActivate].Activate();
            _lastActiveDialogName = dialogToActivate;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _motionController.Update(MillisecondsSinceLastUpdate);

        }

        protected override void HandleBackButtonPress()
        {
            base.HandleBackButtonPress();

            if ((CurrentState == Status.Active) && (!string.IsNullOrEmpty(_lastActiveDialogName))) { _dialogs[_lastActiveDialogName].Cancel(); }
        }
    }
}
