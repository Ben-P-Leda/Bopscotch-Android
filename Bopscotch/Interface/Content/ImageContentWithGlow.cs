using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Bopscotch.Effects;
using Bopscotch.Gameplay.Objects.Behaviours;

namespace Bopscotch.Interface.Content
{
    public class ImageContentWithGlow : ImageContent, ICanHaveGlowEffect
    {
        private GlowBurst _glow;

        public ImageContentWithGlow(string textureName, Vector2 position, bool hideGlow)
            : this(textureName, position, hideGlow, 1.0f, Glow_Scale) { }

        public ImageContentWithGlow(string textureName, Vector2 position, bool hideGlow, float itemScale, float glowScale)
            : base(textureName, position)
        {
            base.Scale = itemScale;

            InitializeGlow(Glow_Layers, glowScale, Glow_Scale_Step, Glow_Render_Depth_Offset);
            if (hideGlow) { _glow.Visible = false; }
        }

        public void InitializeGlow(int layers, float scale, float scaleStep, float renderDepthOffset)
        {
            _glow = new GlowBurst(layers, scale, scaleStep);
            _glow.RenderDepth = RenderDepth + renderDepthOffset;
            _glow.Position = _position;
            _glow.Visible = base.Visible;
        }

        public void UpdateGlow(int millisecondsSinceLastUpdate)
        {
            _glow.Update(millisecondsSinceLastUpdate);
            _glow.Tint = FadedTint;
            _glow.Position = _position + Offset;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _glow.Draw(spriteBatch);
        }

        private const int Glow_Layers = 3;
        private const float Glow_Scale = 0.75f;
        private const float Glow_Scale_Step = 0.1f;
        private const float Glow_Render_Depth_Offset = 0.0001f;
    }
}
