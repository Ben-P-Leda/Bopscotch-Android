using Microsoft.Xna.Framework;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface ICollidable : IWorldObject
    {
        bool Collidable { get; }

        void HandleCollision(ICollidable collider);
    }
}
