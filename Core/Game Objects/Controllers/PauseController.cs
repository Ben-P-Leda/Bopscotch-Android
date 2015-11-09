using System;
using System.Collections.Generic;

using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Game_Objects.Controllers
{
    public class PauseController
    {
        private bool _paused;
        private List<IPausable> _objectsAffectedByPause;

        public bool Paused { get { return _paused; } set { _paused = value; SetPausedState(); } }

        public PauseController()
        {
            _paused = false;
            _objectsAffectedByPause = new List<IPausable>();
        }

        public void AddPausableObject(IPausable toAdd)
        {
            toAdd.Paused = _paused;

            if (!_objectsAffectedByPause.Contains(toAdd)) { _objectsAffectedByPause.Add(toAdd); }
        }

        public void RemovePausableObject(IPausable toRemove)
        {
            if (_objectsAffectedByPause.Contains(toRemove)) { _objectsAffectedByPause.Remove(toRemove); }
        }

        private void SetPausedState()
        {
            for (int i = 0; i < _objectsAffectedByPause.Count; i++) { _objectsAffectedByPause[i].Paused = _paused; }
        }
    }
}
