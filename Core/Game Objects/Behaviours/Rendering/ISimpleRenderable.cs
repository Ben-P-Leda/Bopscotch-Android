using Microsoft.Xna.Framework.Graphics;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface ISimpleRenderable : IGameObject
    {
        bool Visible { get; set; }
        int RenderLayer { get; set; }

        void Draw(SpriteBatch spriteBatch);
    }
}
