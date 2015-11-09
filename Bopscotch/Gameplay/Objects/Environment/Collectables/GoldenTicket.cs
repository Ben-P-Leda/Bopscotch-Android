using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Serialization;

using Bopscotch.Effects;
using Bopscotch.Gameplay.Objects.Behaviours;

namespace Bopscotch.Gameplay.Objects.Environment.Collectables
{
    public class GoldenTicket : Collectable, ICanHaveGlowEffect
    {
        private GlowBurst _glow;

        public GoldenTicket()
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
            _glow.Position = WorldPosition - CameraPosition;
            _glow.Tint = Tint;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _glow.Draw(spriteBatch);
        }

        public const string Data_Node_Name = "golden-ticket";

        private const int Glow_Layers = 3;
        private const float Glow_Scale = 0.75f;
        private const float Glow_Scale_Step = 0.1f;
        private const float Glow_Render_Depth_Offset = 0.0001f;
    }
}
