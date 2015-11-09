using System;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Motion;

namespace Bopscotch.Effects.SmashBlockItems
{
    public class SmashBlockItemMotionEngine : IMotionEngine
    {
        private Vector2 _delta;
        private float _verticalVelocity;

        public Vector2 Delta { get { return _delta; } }

        public SmashBlockItemMotionEngine()
        {
            _delta = Utility.RotatedNormal(
                (MathHelper.Pi * 1.5f) +
                MathHelper.ToRadians(
                    Leda.Core.Random.Generator.Next(Ejection_Sweep_Angle_In_Degrees) * Leda.Core.Random.Generator.NextSign())) * Ejection_Velocity;

            _verticalVelocity = _delta.Y;
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            _delta.Y = 0.0f;
            while (millisecondsSinceLastUpdate-- > 0)
            {
                _delta.Y += _verticalVelocity;
                if (_verticalVelocity <= -Definitions.Zero_Vertical_Velocity) { _verticalVelocity *= Definitions.Normal_Gravity_Value; }
                else if (_verticalVelocity >= Definitions.Zero_Vertical_Velocity) { _verticalVelocity /= Definitions.Normal_Gravity_Value; }
                else { _verticalVelocity = Definitions.Zero_Vertical_Velocity; }
            }

            _delta.Y = Math.Min(_delta.Y, Definitions.Terminal_Velocity);
        }

        private const float Ejection_Sweep_Angle_In_Degrees = 45.0f;
        private const float Ejection_Velocity = 1.0f;
    }
}
