using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;
using Leda.Core.Asset_Management;

namespace Bopscotch.Gameplay.Objects.Environment
{
    public class Background : ISimpleRenderable
    {
        protected Color _tint;
        protected float _renderDepth;
        private Rectangle _targetArea;

        public string TextureReference { get; set; }
        public bool Visible { get; set; }
        public int RenderLayer { get; set; }

        public Background()
        {
            _tint = Color.White;
            _renderDepth = Render_Depth;

            CalculateBackgroundTargetArea();
            RenderLayer = Render_Layer;
            Visible = true;
        }

        private void CalculateBackgroundTargetArea()
        {
            float heightRatio = (float)Definitions.Back_Buffer_Height / (float)GameBase.Instance.GraphicsDevice.Viewport.Height;
            float targetWidth = (float)Definitions.Back_Buffer_Width / heightRatio;
            float xOffset = ((float)GameBase.Instance.GraphicsDevice.Viewport.Width - targetWidth) / 2.0f;

            _targetArea = new Rectangle((int)xOffset, 0, (int)targetWidth, GameBase.Instance.GraphicsDevice.Viewport.Height);
        }

        public void Initialize()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureManager.Textures[TextureReference], _targetArea, null, _tint, 0.0f, Vector2.Zero, SpriteEffects.None, _renderDepth);
        }

        private const int Render_Layer = 0;
        private const float Render_Depth = 0.9f;
    }
}
