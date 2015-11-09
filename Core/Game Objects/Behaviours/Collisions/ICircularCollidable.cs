using Leda.Core.Shapes;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface ICircularCollidable : ICollidable
    {
        Circle CollisionBoundingCircle { get; }
        Circle PositionedCollisionBoundingCircle { get; set; }
    }
}
