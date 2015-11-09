using System;
using Microsoft.Xna.Framework;

using Leda.Core.Motion;

namespace Bopscotch.Gameplay.Objects.Environment.Collectables
{
    public class CollectableExitMotionEngine : IMotionEngine
    {
        private Vector2 _delta;
        private float _verticalVelocity;
        private float _totalMovementDuration;
        private float _elapsedMovementDuration;

        public float Progress { get { return Math.Min(1.0f, _elapsedMovementDuration / _totalMovementDuration); } }

        public Vector2 Delta { get { return _delta; } }

        public CollectableExitMotionEngine()
        {
            _delta = Vector2.Zero;
            _verticalVelocity = 0.0f;
            _totalMovementDuration = 1.0f;
            _elapsedMovementDuration = 0.0f;
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            if (_verticalVelocity != 0.0f)
            {
                _elapsedMovementDuration += millisecondsSinceLastUpdate;
                _delta.Y = 0.0f;

                while (millisecondsSinceLastUpdate-- > 0)
                {
                    _delta.Y += _verticalVelocity;
                    _verticalVelocity *= Definitions.Normal_Gravity_Value;
                }
            }
        }

        public void Activate(float speed, int duration)
        {
            _verticalVelocity = -speed;
            _totalMovementDuration = duration;
        }
    }
}
