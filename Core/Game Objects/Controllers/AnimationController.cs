using System;
using System.Collections.Generic;

using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Game_Objects.Controllers
{
    public class AnimationController : IPausable
    {
        private List<IAnimated> _simpleObjectsToAnimate;

        public bool Paused { private get; set; }

        public AnimationController()
        {
            _simpleObjectsToAnimate = new List<IAnimated>();
        }

        public void AddAnimatedObject(IAnimated toAdd)
        {
            if (!_simpleObjectsToAnimate.Contains(toAdd)) { _simpleObjectsToAnimate.Add(toAdd); }
        }

        public void RemoveAnimatedObject(IAnimated toRemove)
        {
            if (_simpleObjectsToAnimate.Contains(toRemove)) { _simpleObjectsToAnimate.Remove(toRemove); }
        }

		public void FlushAnimatedObjects()
		{
			_simpleObjectsToAnimate.Clear();
		}

        public void Update(int millisecondsSinceLastUpdate)
        {
            if (!Paused)
            {
                for (int i = 0; i < _simpleObjectsToAnimate.Count; i++)
                {
                    if (_simpleObjectsToAnimate[i].AnimationEngine != null)
                    {
                        _simpleObjectsToAnimate[i].AnimationEngine.UpdateAnimation(millisecondsSinceLastUpdate);
                    }
                }
            }
        }
    }
}
