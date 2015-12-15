using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Controllers;

using Bopscotch.Effects;
using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Interface;
using Bopscotch.Interface.Content;
using Bopscotch.Interface.Dialogs;

namespace Bopscotch.Scenes.NonGame
{
    public abstract class ContentSceneWithControlDialog : FlatContentScene
    {
        private MotionController _motionController;

        protected ButtonDialog Dialog { private get; set; }
        protected FullScreenColourOverlay Overlay { get; private set; }

        public ContentSceneWithControlDialog()
            : base()
        {
            _motionController = new MotionController();

            Overlay = new FullScreenColourOverlay();
        }

        public override void Initialize()
        {
            base.Initialize();

            if (Dialog != null)
            {
                Dialog.InputSources = _inputProcessors;
                _motionController.AddMobileObject(Dialog);
                RegisterGameObject(Dialog);
            }

            RegisterGameObject(Overlay);
        }

        protected override void Reset()
        {
            if (Dialog != null)
            {
                Dialog.Reset();
            }

            base.Reset();
        }

        public override void Activate()
        {
            Dialog.Activate();

            base.Activate();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _motionController.Update(MillisecondsSinceLastUpdate);
        }

        protected override void HandleBackButtonPress()
        {
            base.HandleBackButtonPress();

            if (CurrentState == Status.Active) { Dialog.Cancel(); }
        }
    }
}
