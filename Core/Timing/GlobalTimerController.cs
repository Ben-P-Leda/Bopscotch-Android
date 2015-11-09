using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Leda.Core.Timing
{
    public sealed class GlobalTimerController : GameComponent, ITimerController
    {
        private static GlobalTimerController _instance = null;

        public static GlobalTimerController GlobalTimer
        {
            get
            {
                if (_instance == null) { _instance = new GlobalTimerController(); }
                return _instance;
            }
        }

        public static void ClearInstance() { _instance = null; }

        private TimerController _timerController;

        public bool Paused { set { _timerController.Paused = value; } }

        public GlobalTimerController()
            : base(GameBase.Instance)
        {
            _timerController = new TimerController();

            GameBase.Instance.Components.Add(this);
        }

        public override void Update(GameTime gameTime)
        {
            _timerController.Update(gameTime.ElapsedGameTime.Milliseconds);

            base.Update(gameTime);
        }

        public void RegisterUpdateCallback(TimerController.UpdateCallback toRegister)
        {
            _timerController.RegisterUpdateCallback(toRegister);
        }
    }
}
