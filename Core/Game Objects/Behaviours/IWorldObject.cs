using Microsoft.Xna.Framework;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface IWorldObject
    {
        Vector2 WorldPosition { get; set; }
        bool WorldPositionIsFixed { get; }
    }
}
