using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Asset_Management;
using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;

namespace Bopscotch.Effects
{
    public class BackgroundSnow : ISimpleRenderable
    {
        private Flake[] _flakes;

        public int RenderLayer { get { return Render_Layer; } set { } }
        public bool Visible { get { return true; } set { } }

        public void CreateSnowflakes()
        {
            _flakes = new Flake[Snowflake_Count];
            for (int i = 0; i < Snowflake_Count; i++) { _flakes[i] = new Flake(); }
        }

        public void Initialize()
        {
        }

        public void Reset()
        {
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            for (int i = 0; i < Snowflake_Count; i++) { _flakes[i].Update(millisecondsSinceLastUpdate); }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Snowflake_Count; i++) { _flakes[i].Draw(spriteBatch); }
        }

        private class Flake : DisposableSimpleDrawableObject
        {
            private int _offset;
            private long _millisecondsSinceLaunched;
            private Vector2 _delta;

            public Flake()
                : base()
            {
                Texture = TextureManager.Textures["speedo-snowflake"];
                Frame = TextureManager.Textures["speedo-snowflake"].Bounds;
                Origin = new Vector2(Frame.Width, Frame.Height) / 2.0f;
                RenderLayer = Render_Layer;
                Tint = Color.Lerp(Color.White, Color.Transparent, 0.25f);
                Visible = true;

                Reset();
            }

            public override void Reset()
            {
                WorldPosition = new Vector2(Leda.Core.Random.Generator.Next(Definitions.Back_Buffer_Width), -Leda.Core.Random.Generator.Next(Definitions.Back_Buffer_Height));
                Scale = Leda.Core.Random.Generator.Next(Minimum_Scale, Maximum_Scale);

                _delta = Vector2.UnitY;
                _offset = Leda.Core.Random.Generator.NextInt(Offset_Seed);
                _millisecondsSinceLaunched = 0;
            }

            public void Update(int millisecondsSinceLastUpdate)
            {
                WorldPosition += ((_delta / Speed_Modifier) * millisecondsSinceLastUpdate);

                _millisecondsSinceLaunched += millisecondsSinceLastUpdate;
                _delta.X = (float)Math.Sin((float)(_millisecondsSinceLaunched + _offset) / 500) / 3;
                Rotation = _delta.X;

                if (WorldPosition.Y > Definitions.Back_Buffer_Height + Bottom_Margin) { Reset(); }
            }

            private const int Offset_Seed = 1000;
            private const float Minimum_Scale = 0.15f;
            private const float Maximum_Scale = 0.35f;
            private const float Bottom_Margin = 64.0f;
            private const float Speed_Modifier = 12.0f;
            private const float Render_Depth = 0.02f;
        }

        private const int Render_Layer = 0;
        private const int Snowflake_Count = 50;
    }
}
