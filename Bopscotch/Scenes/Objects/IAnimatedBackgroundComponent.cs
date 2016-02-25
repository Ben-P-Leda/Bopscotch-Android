using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bopscotch.Scenes.Objects
{
    public interface IAnimatedBackgroundComponent
    {
        void SetOffset(Vector2 offset);
        void UpdateAbsolutePosition(Vector2 step, float wrapEdge);
        void Draw(SpriteBatch spriteBatch);
    }
}
