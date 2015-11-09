using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Bopscotch.Gameplay.Objects.Behaviours;

namespace Bopscotch.Effects.SmashBlockItems
{
    public class SmashBlockGlowingItem : SmashBlockItem, ICanHaveGlowEffect
    {
        private GlowBurst _glow;

        public SmashBlockGlowingItem()
            : base()
        {
            InitializeGlow(Glow_Layers, Glow_Scale, Glow_Scale_Step, Glow_Render_Depth_Offset);
        }

        public void InitializeGlow(int layers, float scale, float scaleStep, float renderDepthOffset)
        {
            _glow = new GlowBurst(layers, scale, scaleStep);
            _glow.RenderDepth = RenderDepth + renderDepthOffset;
            _glow.Position = base.WorldPosition - base.CameraPosition;
            _glow.Visible = base.Visible;
        }

        public void UpdateGlow(int millisecondsSinceLastUpdate)
        {
            _glow.Update(millisecondsSinceLastUpdate);
            _glow.Position = (WorldPosition - CameraPosition) + (new Vector2(Frame.Width, Frame.Height) / 2.0f);
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
