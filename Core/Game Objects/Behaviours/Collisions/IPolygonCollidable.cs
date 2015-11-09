using Leda.Core.Shapes;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface IPolygonCollidable : ICollidable
    {
        Polygon CollisionShape { get; }
        float Rotation { get; }
        float Scale { get; }
        bool Mirror { get; }
    }
}
