using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Leda.Core.Timing
{
    public class TimerController : ITimerController
    {
        public delegate void UpdateCallback(int elapsedTime);
        public delegate void TickCallbackRegistrationHandler(UpdateCallback toRegister);

        private List<UpdateCallback> _timerCallbacks;

        public bool Paused { set; private get; }

        public TimerController()
            : base()
        {
            Paused = false;

            _timerCallbacks = new List<UpdateCallback>();
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            if (!Paused)
            {
                for (int i = 0; i < _timerCallbacks.Count; i++) { _timerCallbacks[i](millisecondsSinceLastUpdate); }
            }
        }

        public void RegisterUpdateCallback(UpdateCallback toRegister)
        {
            if (!_timerCallbacks.Contains(toRegister)) { _timerCallbacks.Add(toRegister); }
        }
    }
}
