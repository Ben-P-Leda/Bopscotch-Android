using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Asset_Management;

namespace Bopscotch.Effects
{
    public class GlowBurst
    {
        private List<Vector2> _components;
        private float _scaleStep;
        private float _masterScale;

        public float RenderDepth { private get; set; }
        public Vector2 Position { get; set; }
        public bool Visible { get; set; }
        public Color Tint { private get; set; }

        public GlowBurst(int componentCount, float scale, float scaleStep)
        {
            _components = new List<Vector2>();
            _masterScale = scale;
            _scaleStep = scaleStep;

            for (int i = 0; i < componentCount; i++) { _components.Add(CreateComponent()); }

            Tint = Color.White;
            Visible = false;
        }

        private Vector2 CreateComponent()
        {
            return new Vector2(
                MathHelper.ToRadians(Random.Generator.Next(Minimum_Step_In_Degrees, Maximum_Step_In_Degrees)) * (((_components.Count % 2) * 2) - 1),
                Random.Generator.Next(MathHelper.TwoPi));
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            for (int i = 0; i < _components.Count; i++)
            {
                float updatedValue = Utility.RectifyAngle(_components[i].Y + (_components[i].X * millisecondsSinceLastUpdate));
                _components[i] = new Vector2(_components[i].X, updatedValue);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                for (int i = 0; i < _components.Count; i++)
                {
                    spriteBatch.Draw(TextureManager.Textures[Texture_Name], GameBase.ScreenPosition(Position), null, Tint, _components[i].Y,
                        new Vector2(TextureManager.Textures[Texture_Name].Width, TextureManager.Textures[Texture_Name].Height) / 2.0f,
                        GameBase.ScreenScale((1.0f - (_scaleStep * i)) * _masterScale), SpriteEffects.None, RenderDepth + (Render_Depth_Step * i));
                }
            }
        }

        private string Texture_Name = "glow-effect";
        private float Minimum_Step_In_Degrees = 0.05f;
        private float Maximum_Step_In_Degrees = 0.2f;
        private float Render_Depth_Step = 0.0001f;
    }
}
