using System;
using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;

namespace Leda.Core.Motion.Engines
{
    public class BounceExitMotionEngine : EasingMotionEnginebase
    {
        public BounceExitMotionEngine()
            : base()
        {
        }

        public override void Activate()
        {
            base.Activate();

            _movementAngle = Utility.PointToPointAngle(ObjectToTrack.WorldPosition, TargetWorldPosition);
            _movementVector = -Vector2.Normalize(TargetWorldPosition - ObjectToTrack.WorldPosition);
        }

        public override void CalculateDelta(int millisecondsSinceLastUpdate)
        {
            _delta = Vector2.Zero;

            if (_movementVector != Vector2.Zero)
            {
                _delta = _movementVector * _speed * millisecondsSinceLastUpdate;

                if (Math.Abs(Utility.AngleDifference(Utility.PointToPointAngle(TargetWorldPosition, ObjectToTrack.WorldPosition + _delta), _movementAngle)) >= MathHelper.PiOver2)
                {
                    for (int i = 0; i < RecoilMultiplier; i++)
                    {
                        if (Math.Abs(Utility.AngleDifference(Utility.VectorAngle(_movementVector), _movementAngle)) >= MathHelper.PiOver2)
                        {
                            _speed *= RecoilRate;
                            if (_speed < Direction_Switch_Speed) { _movementVector = -_movementVector; }
                        }
                        else
                        {
                            _speed = Math.Min(_speed / RecoilRate, Speed);
                        }
                    }
                }
                else if (Math.Abs(Utility.AngleDifference(Utility.PointToPointAngle(TargetWorldPosition, ObjectToTrack.WorldPosition + _delta), _movementAngle)) < MathHelper.PiOver2)
                {
                    _delta = TargetWorldPosition - ObjectToTrack.WorldPosition;
                    _speed = 0.0f;
                    _movementVector = Vector2.Zero;
                    if (CompletionCallback != null) { CompletionCallback(); }
                }
            }
        }
    }
}
