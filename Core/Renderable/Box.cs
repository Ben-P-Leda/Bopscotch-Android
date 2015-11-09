using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Renderable
{
    public class Box : ISimpleRenderable
    {
        public Vector2 Position { get; set; }
        public virtual Point Dimensions { get; set; }
        public Texture2D EdgeTexture { private get; set; }
        public Texture2D CornerTexture { private get; set; }
        public virtual Color EdgeTint { protected get; set; }
        public Texture2D BackgroundTexture { private get; set; }
        public virtual Color BackgroundTint { protected get; set; }

        public int RenderLayer { get; set; }
        public virtual bool Visible { get; set; }
        public float RenderDepth { get; set; }

        public Box()
        {
            Position = Vector2.Zero;
            Dimensions = Point.Zero;
            EdgeTexture = null;
            CornerTexture = null;
            EdgeTint = Color.White;
            BackgroundTexture = null;
            BackgroundTint = Color.Black;

            RenderLayer = -1;
            Visible = false;
            RenderDepth = 0.5f;
        }

        public virtual void Initialize()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            Vector2 topLeft = new Vector2(Position.X + (EdgeTexture.Height / 2.0f), Position.Y + (EdgeTexture.Height / 2.0f));
            float right = topLeft.X + (Dimensions.X - EdgeTexture.Height);
            float bottom = topLeft.Y + (Dimensions.Y - EdgeTexture.Height);

            RenderTools.Line(spriteBatch, EdgeTexture, topLeft, new Vector2(right, topLeft.Y), 1.0f, EdgeTint, RenderDepth);
            RenderTools.Line(spriteBatch, EdgeTexture,topLeft, new Vector2(topLeft.X, bottom), 1.0f, EdgeTint, RenderDepth);
            RenderTools.Line(spriteBatch, EdgeTexture, new Vector2(topLeft.X, bottom), new Vector2(right, bottom), 1.0f, EdgeTint, RenderDepth);
            RenderTools.Line(spriteBatch, EdgeTexture, new Vector2(right, topLeft.Y), new Vector2(right, bottom), 1.0f, EdgeTint, RenderDepth);

            topLeft = GameBase.ScreenPosition(topLeft);
            right = topLeft.X + GameBase.ScreenScale(Dimensions.X - EdgeTexture.Height);
            bottom = topLeft.Y + GameBase.ScreenScale(Dimensions.Y - EdgeTexture.Height);

            if (CornerTexture != null)
            {
                spriteBatch.Draw(CornerTexture, topLeft, null, EdgeTint, 0.0f, new Vector2(CornerTexture.Width, CornerTexture.Height) / 2.0f, 1.0f, SpriteEffects.None, RenderDepth - 0.005f);
                spriteBatch.Draw(CornerTexture, new Vector2(right, topLeft.Y), null, EdgeTint, MathHelper.PiOver2, new Vector2(CornerTexture.Width, CornerTexture.Height) / 2.0f, 1.0f, SpriteEffects.None, RenderDepth - 0.005f);
                spriteBatch.Draw(CornerTexture, new Vector2(right, bottom), null, EdgeTint, MathHelper.Pi, new Vector2(CornerTexture.Width, CornerTexture.Height) / 2.0f, 1.0f, SpriteEffects.None, RenderDepth - 0.005f);
                spriteBatch.Draw(CornerTexture, new Vector2(topLeft.X, bottom), null, EdgeTint, MathHelper.Pi * 1.5f, new Vector2(CornerTexture.Width, CornerTexture.Height) / 2.0f, 1.0f, SpriteEffects.None, RenderDepth - 0.005f);
            }

            if (BackgroundTexture != null)
            {
                Rectangle area = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(right - topLeft.X),(int)(bottom - topLeft.Y));
                spriteBatch.Draw(BackgroundTexture, area, null, BackgroundTint, 0.0f, Vector2.Zero, SpriteEffects.None, RenderDepth + 0.005f);
            }
        }
    }
}
