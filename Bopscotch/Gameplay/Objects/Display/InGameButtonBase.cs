using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Shapes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;

namespace Bopscotch.Gameplay.Objects.Display
{
    public abstract class InGameButtonBase : Circle, ISimpleRenderable
    {
        private Texture2D _containerTexture;
        private Vector2 _containerOrigin;

        protected Color _tint;
        protected float _scale;

        public int RenderLayer { get { return Render_Layer; } set { } }
        public bool Visible { get; set; }

        protected virtual Vector2 CenterPosition { get { return base.Center; } set { base.Center = value; } }
        protected virtual float Scale { get { return _scale; } set { _scale = value; Radius = _containerOrigin.X * value; } }

        public InGameButtonBase()
            : base(Vector2.Zero, 0.0f)
        {
            _scale = Default_Scale;
            _tint = Color.DodgerBlue;

            Visible = true;
        }

        public virtual void Initialize()
        {
            _containerTexture = TextureManager.Textures[Container_Texture_Name];
            _containerOrigin = new Vector2(_containerTexture.Width, _containerTexture.Height) / 2.0f;

            Radius = _containerOrigin.X;
        }

        public virtual void Reset()
        {
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_containerTexture, GameBase.ScreenPosition(Center), null, _tint, 0.0f, _containerOrigin, GameBase.ScreenScale(_scale),
                SpriteEffects.None, Container_Render_Depth);
        }

        private const int Render_Layer = 4;
        private const string Container_Texture_Name = "button-icon-container";
        private const float Container_Render_Depth = 0.51f;
        private const float Default_Scale = 0.6f;

        protected const float Icon_Render_Depth = 0.5f;
    }
}
