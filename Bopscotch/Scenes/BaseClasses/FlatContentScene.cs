using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Behaviours;

using Bopscotch.Interface.Content;
using Bopscotch.Gameplay.Objects.Behaviours;

namespace Bopscotch.Scenes.BaseClasses
{
    public class FlatContentScene : StaticSceneBase
    {
        private List<ContentBase> _contentElements;
        private List<ICanHaveGlowEffect> _objectsWithGlowEffect;

        protected float ContentFadeFraction
        {
            set
            {
                for (int i = 0; i < _contentElements.Count; i++) { _contentElements[i].FadeFraction = value; }
            }
        }

        protected bool ContentVisible
        {
            set
            {
                for (int i = 0; i < _contentElements.Count; i++) { _contentElements[i].Visible = value; }
            }
        }

        public FlatContentScene()
            : base()
        {
            _contentElements = new List<ContentBase>();
            _objectsWithGlowEffect = new List<ICanHaveGlowEffect>();
        }

        protected override void RegisterGameObject(IGameObject toRegister)
        {
            if ((toRegister is ContentBase) && (!_contentElements.Contains((ContentBase)toRegister))) { _contentElements.Add((ContentBase)toRegister); }
            if (toRegister is ICanHaveGlowEffect) { _objectsWithGlowEffect.Add((ICanHaveGlowEffect)toRegister); }
            base.RegisterGameObject(toRegister);
        }

        protected override void UnregisterGameObject(IGameObject toUnregister)
        {
            if ((toUnregister is ContentBase) && (_contentElements.Contains((ContentBase)toUnregister))) { _contentElements.Remove((ContentBase)toUnregister); }
            if (toUnregister is ICanHaveGlowEffect) { _objectsWithGlowEffect.Remove((ICanHaveGlowEffect)toUnregister); }
            base.UnregisterGameObject(toUnregister);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for (int i = 0; i < _objectsWithGlowEffect.Count; i++) { _objectsWithGlowEffect[i].UpdateGlow(MillisecondsSinceLastUpdate); }
        }
    }
}
