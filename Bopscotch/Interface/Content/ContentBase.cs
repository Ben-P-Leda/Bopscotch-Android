using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Game_Objects.Behaviours;

namespace Bopscotch.Interface.Content
{
    public abstract class ContentBase : IGameObject, ISimpleRenderable
    {
        protected Vector2 _position;

        public virtual Vector2 Offset { protected get; set; }
        public virtual Color Tint { protected get; set; }
        public virtual float FadeFraction { protected get; set; }
        public float FadeFractionModifier { protected get; set; }
        public virtual float Scale { protected get; set; }

        public virtual bool Visible { get; set; }
        public virtual int RenderLayer { get; set; }
        public virtual float RenderDepth { protected get; set; }

        public ContentBase(Vector2 position)
        {
            _position = position;

            Offset = Vector2.Zero;

            Tint = Color.White;
            FadeFraction = 1.0f;
            FadeFractionModifier = 1.0f;
            Scale = 1.0f;

            Visible = true;
            RenderLayer = Default_Render_Layer;
            RenderDepth = Default_Render_Depth;
        }

        public void Initialize()
        {
        }

        public void Reset()
        {
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
        }

        protected Color FadedTint { get { return Color.Lerp(Color.Transparent, Tint, FadeFraction * FadeFractionModifier); } }

        private const int Default_Render_Layer = 4;
        private const float Default_Render_Depth = 0.2f;

    }
}
