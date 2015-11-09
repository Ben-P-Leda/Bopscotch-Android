using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Leda.Core
{
    public sealed class RenderTools
    {
        public static void Line(SpriteBatch spriteBatch, Texture2D texture, Vector2 origin, Vector2 target, float thickness, Color tint, float depth)
        {
            float angle = Utility.PointToPointAngle(origin, target);
            Vector2 renderLine = new Vector2(GameBase.ScreenScale(Vector2.Distance(origin, target)), Math.Max(GameBase.ScreenScale(thickness), 1.0f));

            spriteBatch.Draw(texture, GameBase.ScreenPosition(origin), null, tint, angle, new Vector2(0.0f, (thickness * texture.Height) / 2.0f), renderLine, SpriteEffects.None, depth);
        }
    }
}
