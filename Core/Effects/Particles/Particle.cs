using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Game_Objects.Base_Classes;

namespace Leda.Core.Effects.Particles
{
    public class Particle : DisposableSimpleDrawableObject
    {
        private Vector2 _movementUnit;
        private float _elapsedDuration;
        private int _nextFadePoint;
        private Color _masterTint;

        public float Duration { private get; set; }

        public float Direction { set { _movementUnit = Utility.RotatedNormal(value); } }
        public float Speed { private get; set; }
        public Vector2 Gravity { private get; set; }
        public float GravityAccelerator { private get; set; }

        public float SpeedDelta { private get; set; }
        public float RotationDelta { private get; set; }
        public float ScaleDelta { private get; set; }

        public float SpeedAccelerator { private get; set; }
        public float RotationAccelerator { private get; set; }
        public float ScaleAccelerator { private get; set; }

        public override Color Tint { set { _masterTint = value; base.Tint = value; } }

        public List<Vector2> FadePoints { private get; set; }

        public Particle()
            : base()
        {
            _elapsedDuration = 0;
            _nextFadePoint = 1;
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            _elapsedDuration += millisecondsSinceLastUpdate;

            if (_elapsedDuration >= Duration)
            {
                Visible = false;
            }
            else
            {
                WorldPosition += (((_movementUnit * Speed) + Gravity) * millisecondsSinceLastUpdate);
                Speed *= SpeedDelta;
                SpeedDelta *= SpeedAccelerator;
                Gravity *= GravityAccelerator;

                Rotation += RotationDelta * millisecondsSinceLastUpdate;
                RotationDelta *= RotationAccelerator;

                Scale *= ScaleDelta;
                ScaleDelta *= ScaleAccelerator;

                if ((_nextFadePoint < FadePoints.Count - 1) && (_elapsedDuration / Duration > FadePoints[_nextFadePoint].X))
                {
                    while ((_nextFadePoint < FadePoints.Count - 1) && (_elapsedDuration / Duration > FadePoints[_nextFadePoint].X)) { _nextFadePoint++; }
                }

                float fadePointDuration = FadePoints[_nextFadePoint].X - FadePoints[_nextFadePoint - 1].X;
                float fadePointElapsed = (_elapsedDuration / Duration) - FadePoints[_nextFadePoint - 1].X;

                base.Tint = _masterTint * MathHelper.Lerp(
                    FadePoints[_nextFadePoint - 1].Y,
                    FadePoints[_nextFadePoint].Y,
                    MathHelper.Clamp(fadePointElapsed / fadePointDuration, 0.0f, 1.0f));
            }
        }
    }
}
