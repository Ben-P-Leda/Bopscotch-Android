using Microsoft.Xna.Framework;

using Leda.Core.Motion;

namespace Bopscotch.Interface.Objects.EditCharacter
{
    public class CustomisationDisplayBlockMotionEngine : IMotionEngine
    {
        private Vector2 _delta;
        public Vector2 Delta { get { return _delta; } }

        public CustomisationDisplayBlockMotionEngine()
        {
            _delta = Vector2.Zero;
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            _delta.X = Movement_Speed * millisecondsSinceLastUpdate;
        }

        public const float Movement_Speed = -0.475f;
    }
}
