using System;
using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;

namespace Leda.Core.Motion.Engines
{
    public class DecelerateEntryMotionEngine : EasingMotionEnginebase
    {
        public DecelerateEntryMotionEngine()
            : base()
        {
            Speed = Default_Speed;
        }

        public override void Activate()
        {
            base.Activate();

            _movementAngle = Utility.PointToPointAngle(ObjectToTrack.WorldPosition, TargetWorldPosition);
            _movementVector = Vector2.Normalize(TargetWorldPosition - ObjectToTrack.WorldPosition);
        }

        public override void CalculateDelta(int millisecondsSinceLastUpdate)
        {
            _delta = Vector2.Zero;

            if (_movementVector != Vector2.Zero)
            {
                if ((Math.Abs(Utility.AngleDifference(Utility.VectorAngle(_movementVector), _movementAngle)) >= MathHelper.PiOver2) || (_speed <= Cutoff_Speed))
                {
                    _delta = TargetWorldPosition - ObjectToTrack.WorldPosition;
                    _speed = 0.0f;
                    _movementAngle = Utility.PointToPointAngle(TargetWorldPosition, ObjectToTrack.WorldPosition);
                    _movementVector = Vector2.Zero;
                    if (CompletionCallback != null) { CompletionCallback(); }
                }
                else
                {
                    _delta = _movementVector * _speed * millisecondsSinceLastUpdate;

                    if (Math.Abs(Utility.AngleDifference(Utility.PointToPointAngle(TargetWorldPosition, ObjectToTrack.WorldPosition + _delta), _movementAngle)) < MathHelper.PiOver2)
                    {
                        _delta *= Speed_Reduction_Rate;
                        _speed *= Speed_Reduction_Rate;
                    }
                }
            }
        }

        private const float Default_Speed = 3.5f;
        private const float Cutoff_Speed = 0.8f;
        private const float Speed_Reduction_Rate = 0.4999f;
    }
}
