using Microsoft.Xna.Framework;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface IBoxCollidable : ICollidable
    {
        Rectangle CollisionBoundingBox { get; }
        Rectangle PositionedCollisionBoundingBox { get; set; }
    }
}
