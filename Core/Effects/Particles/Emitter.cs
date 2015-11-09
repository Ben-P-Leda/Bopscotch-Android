using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Gamestate_Management;
using Leda.Core.Timing;
using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Effects.Particles
{
    public class Emitter : IWorldObject, ICameraRelative, IHasParticleEffects
    {
        private Timer _burstTimer;
        private int _burstCount;
        private int _particlesPerBurst;
        private int _burstIntervalInMilliseconds;
        private bool _timerRegistered;
        private Vector2 _gravity;
        private float _gravityAccelerator;

        public Vector2 WorldPosition { get; set; }
        public Vector2 CameraPosition { private get; set; }
        public Texture2D ParticleTexture { private get; set; }
        public float RenderDepth { private get; set; }

        public float Direction { private get; set; }
        public float Spread { private get; set; }

        public TimerController.TickCallbackRegistrationHandler TickCallback { private get; set; }
        public ParticleController.ParticleRegistrationHandler ParticleRegistrationCallback { private get; set; }

        // Emission points - relative positions where particles are launched from
        public List<Vector2> EmissionPointOffsets { get; set; }

        // How long do the particles last?
        private Range _lifespanRange;

        // Initial values when they are launched from the emitter
        private Range _speedLaunchRange;
        private Range _rotationLaunchRange;
        private Range _scaleLaunchRange;

        // Delta values - amounts current values change by per millisecond elapsed
        private Range _speedDeltaRange;
        private Range _rotationDeltaRange;
        private Range _scaleDeltaRange;

        // Accelerators - amounts delta values change by per millisecond elapsed
        private Range _speedAcceleratorRange;
        private Range _rotationAcceleratorRange;
        private Range _scaleAcceleratorRange;

        // Tints - colors the particles can be
        public List<Color> Tints { get; set; }

        // Fade points - life timer offsets and alpha value pairs to control shimmer effects
        private List<Vector2> _fadePoints;
        private List<Vector2> _sortedFadePoints;
        public List<Vector2> FadePoints { set { _sortedFadePoints = value; } }

        public bool WorldPositionIsFixed { get; set; }

        public Emitter()
        {
            EmissionPointOffsets = new List<Vector2>();
            Tints = new List<Color>();
            RenderDepth = 0.5f;

            TickCallback = null;
            ParticleRegistrationCallback = null;

            WorldPositionIsFixed = true;

            _fadePoints = null;
            _sortedFadePoints = new List<Vector2>();

            _burstTimer = null;
            _burstCount = 1;
            _particlesPerBurst = 0;
            _burstIntervalInMilliseconds = 0;
            _timerRegistered = false;

            Direction = 0.0f;
            Spread = MathHelper.Pi;

            _lifespanRange = Range.Empty;

            _gravity = Vector2.Zero;
            _gravityAccelerator = 1.0f;

            SetSpeedMetrics(Range.Empty, Range.One, Range.One);
            SetSpinMetrics(new Range(0.0f, 360.0f), Range.Empty, Range.One);
            SetScaleMetrics(Range.One, Range.One, Range.One);
        }

        public void SetBurstMetrics(int burstCount, int burstIntervalInMilliseconds, int particlesPerBurst)
        {
            if ((_burstTimer == null) && (burstCount != 1)) { _burstTimer = new Timer("bursttimer", CreateBurst); }

            _burstCount = burstCount;
            _particlesPerBurst = particlesPerBurst;
            _burstIntervalInMilliseconds = burstIntervalInMilliseconds;
        }

        public void SetParticleLifespan(int minimumDurationInMilliseconds, int maximumDurationInMilliseconds)
        {
            _lifespanRange = new Range(minimumDurationInMilliseconds, maximumDurationInMilliseconds);
        }

        public void SetGravity(float angle, float force, float acceleration)
        {
            _gravity = Utility.RotatedNormal(angle) * force;
            _gravityAccelerator = acceleration;
        }

        public void SetSpeedMetrics(Range launch, Range delta, Range acceleration)
        {
            _speedLaunchRange = launch;
            _speedDeltaRange = delta;
            _speedAcceleratorRange = acceleration;
        }

        public void SetSpinMetrics(Range launch, Range delta, Range acceleration)
        {
            _rotationLaunchRange = new Range(MathHelper.ToRadians(launch.Minimum), MathHelper.ToRadians(launch.Maximum));
            _rotationDeltaRange = new Range(MathHelper.ToRadians(delta.Minimum), MathHelper.ToRadians(delta.Maximum));
            _rotationAcceleratorRange = acceleration;
        }

        public void SetScaleMetrics(Range launch, Range delta, Range acceleration)
        {
            _scaleLaunchRange = launch;
            _scaleDeltaRange = delta; ;
            _scaleAcceleratorRange = acceleration;
        }

        public void AddFadePoint(float lifespanFraction, float alphaFraction)
        {
            if (_fadePoints == null) { _fadePoints = new List<Vector2>(); }
            _fadePoints.Add(new Vector2(MathHelper.Clamp(lifespanFraction, 0.0f, 1.0f), MathHelper.Clamp(alphaFraction, 0.0f, 1.0f)));
        }

        public void Activate()
        {
            if (_burstTimer != null)
            {
                if ((!_timerRegistered) && (TickCallback != null)) { TickCallback(_burstTimer.Tick); _timerRegistered = true; }
                _burstTimer.ActionSpeed = 1.0f;
            }

            CreateBurst();
        }

        public void Deactivate()
        {
            if (_burstTimer != null) { _burstTimer.ActionSpeed = 0.0f; }
        }

        private void CreateBurst()
        {
            if (ParticleRegistrationCallback != null)
            {
                // Check if we have not set up any tints/fadepoints/emission offsets and put in some defaults if we have not
                if (Tints.Count < 1) { Tints.Add(Color.White); }
                if (_sortedFadePoints.Count < 1)
                {
                    if (_fadePoints != null) { _sortedFadePoints = _fadePoints.OrderBy(fp => fp.X).ToList(); }
                    else { _sortedFadePoints.Add(new Vector2(0.0f, 1.0f)); _sortedFadePoints.Add(new Vector2(1.0f, 0.0f)); }
                }
                if (EmissionPointOffsets.Count < 1) { EmissionPointOffsets.Add(Vector2.Zero); }

                for (int i = 0; i < _particlesPerBurst; i++)
                {
                    Particle newParticle = new Particle();
                    newParticle.Duration = _lifespanRange.RandomValue;
                    newParticle.WorldPosition = WorldPosition + EmissionPointOffsets[(int)Random.Generator.Next(EmissionPointOffsets.Count)];
                    newParticle.CameraPosition = CameraPosition;

                    newParticle.Texture = ParticleTexture;
                    newParticle.Frame = ParticleTexture.Bounds;
                    newParticle.Origin = new Vector2(ParticleTexture.Width, ParticleTexture.Height) / 2.0f;
                    newParticle.RenderDepth = RenderDepth;
                    newParticle.Tint = Tints[(int)Random.Generator.Next(Tints.Count)] * _sortedFadePoints[0].Y;
                    newParticle.FadePoints = _sortedFadePoints;
                    newParticle.Visible = true;

                    newParticle.Direction = Direction + Random.Generator.Next(-Spread, Spread);
                    newParticle.Gravity = _gravity;
                    newParticle.GravityAccelerator = _gravityAccelerator;
                    newParticle.Speed = _speedLaunchRange.RandomValue;
                    newParticle.SpeedDelta = _speedDeltaRange.RandomValue;
                    newParticle.SpeedAccelerator = _speedAcceleratorRange.RandomValue;

                    newParticle.Rotation = _rotationLaunchRange.RandomValue;
                    newParticle.RotationDelta = _rotationDeltaRange.RandomValue;
                    newParticle.RotationAccelerator = _rotationAcceleratorRange.RandomValue;

                    newParticle.Scale = _scaleLaunchRange.RandomValue;
                    newParticle.ScaleDelta = _scaleDeltaRange.RandomValue;
                    newParticle.ScaleAccelerator = _scaleDeltaRange.RandomValue;

                    ParticleRegistrationCallback(newParticle);
                }

                _burstCount = Math.Max(-1, _burstCount - 1);
                if ((_burstCount != 0) && (_timerRegistered)) { _burstTimer.NextActionDuration = _burstIntervalInMilliseconds; }
            }
        }
    }
}
