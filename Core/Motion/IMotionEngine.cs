using Microsoft.Xna.Framework;

namespace Leda.Core.Motion
{
    public interface IMotionEngine
    {
        Vector2 Delta { get; }

        void Update(int millisecondsSinceLastUpdate);
    }
}
