using Microsoft.Xna.Framework;

using Leda.Core.Animation;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface IColourAnimatable : ISimpleRenderable
    {
        Color Tint { get; set; }
    }
}
