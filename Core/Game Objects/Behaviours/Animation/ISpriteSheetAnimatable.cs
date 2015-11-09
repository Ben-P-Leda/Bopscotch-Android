using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface ISpriteSheetAnimatable : ISimpleRenderable
    {
        Texture2D Texture { set; }
        Rectangle Frame { set; }
        Vector2 Origin { set; }
    }
}
