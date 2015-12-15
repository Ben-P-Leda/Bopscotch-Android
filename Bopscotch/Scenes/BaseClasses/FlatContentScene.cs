using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
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

        protected void FlushContent()
        {
            for (int i=_contentElements.Count - 1; i>= 0; i--)
            {
                UnregisterGameObject(_contentElements[i]);
            }
        }

        protected TextContent CreateTextElement(string text, Vector2 position, TextWriter.Alignment alignment, float scale)
        {
            TextContent element = new TextContent(text, position)
            {
                RenderDepth = Default_Render_Depth,
                RenderLayer = Default_Render_Layer,
                Scale = scale,
                Alignment = alignment
            };

            RegisterGameObject(element);

            return element;
        }

        protected ImageContent CreateImageElement(string textureName, Vector2 position, Rectangle frame, Vector2 origin, float scale)
        {
            ImageContent element = new ImageContent(textureName, position)
            {
                RenderDepth = Default_Render_Depth,
                RenderLayer = Default_Render_Layer,
                Frame = frame,
                Origin = origin,
                Scale = scale
            };

            RegisterGameObject(element);

            return element;
        }

        private const int Default_Render_Layer = 2;
        protected const float Default_Render_Depth = 0.5f;
    }
}
