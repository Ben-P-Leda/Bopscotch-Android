using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Motion.Engines
{
    public class TrackingMotionEngine : IMotionEngine
    {
        private Vector2 _delta;

        public IWorldObject Tracker { private get; set; }
        public IWorldObject Target { private get; set; }
        public float MovementSpeed { private get; set; }

        public Vector2 Delta { get { return _delta * MovementSpeed; } }
        public float AngleToTarget { get { return Utility.PointToPointAngle(Vector2.Zero, _delta); } }

        public TrackingMotionEngine()
        {
            Tracker = null;
            Target = null;
            MovementSpeed = 0.0f;

            _delta = Vector2.Zero;
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            if ((Tracker != null) && (Target != null))
            {
                _delta = Vector2.Normalize(Target.WorldPosition - Tracker.WorldPosition) * millisecondsSinceLastUpdate;
            }
        }
    }
}
