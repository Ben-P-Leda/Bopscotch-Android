using Microsoft.Xna.Framework;

using Leda.Core.Timing;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Shapes;

namespace Bopscotch.Gameplay.Objects.Environment.Blocks
{
    public class BombBlockBlastCollider : ICircularCollidable, IGameObject, ICameraRelative
    {
        private Timer _activityTimer;
        private bool _collidable;

        public Circle CollisionBoundingCircle { get; private set; }
        public Circle PositionedCollisionBoundingCircle { get; set; }
        public Vector2 WorldPosition { get; set; }
        public Vector2 CameraPosition { set { } }
        public bool WorldPositionIsFixed { get { return true; } }

        public bool Collidable 
        { 
            get
            {
                return _collidable;
            }
            set
            {
                _collidable = value;
                if (value)
                {
                    _activityTimer.NextActionDuration = Activity_Duration;
                }
            }
        }

        public TimerController.TickCallbackRegistrationHandler TickCallback
        {
            set
            {
                if (_activityTimer == null) { _activityTimer = new Timer("activity-timer", Reset); }
                value(_activityTimer.Tick);
            }
        }

        public BombBlockBlastCollider()
        {
            CollisionBoundingCircle = new Circle(Vector2.Zero, Collision_Radius);
        }

        public void Initialize()
        {
        }

        public void Reset()
        {
            _collidable = false;
        }

        public void HandleCollision(ICollidable collider)
        {
        }

        private const float Collision_Radius = 150.0f;
        private const float Activity_Duration = 20.0f;
    }
}
