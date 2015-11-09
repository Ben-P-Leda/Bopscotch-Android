using System.Collections.Generic;

using Leda.Core.Shapes;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface ICompoundPolygonCollidable : ICollidable
    {
        List<Polygon> CollisionShapes { get; }
        float Rotation { get; }
        float Scale { get; }
        bool Mirror { get; }
    }
}
