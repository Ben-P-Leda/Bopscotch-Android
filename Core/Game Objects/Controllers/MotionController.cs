using System.Collections.Generic;

using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Game_Objects.Controllers
{
    public class MotionController
    {
        private List<IMobile> _objectsToMove;

        public MotionController()
        {
            _objectsToMove = new List<IMobile>();
        }

        public void AddMobileObject(IMobile toAdd)
        {
            if (!_objectsToMove.Contains(toAdd)) { _objectsToMove.Add(toAdd); }
        }

        public void RemoveMobileObject(IMobile toRemove)
        {
            if (_objectsToMove.Contains(toRemove)) { _objectsToMove.Remove(toRemove); }
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            for (int i = 0; i < _objectsToMove.Count; i++) { _objectsToMove[i].Update(millisecondsSinceLastUpdate); }
        }
    }
}
