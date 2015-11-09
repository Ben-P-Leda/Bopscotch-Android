using Microsoft.Xna.Framework;

using Leda.Core.Shapes;

namespace Bopscotch.Input
{
    public class InGameButtonArea : Circle
    {
        public bool Active { get; set; }

        public InGameButtonArea(Vector2 center, float radius)
            : base(center, radius)
        {
            Active = false;
        }
    }
}
