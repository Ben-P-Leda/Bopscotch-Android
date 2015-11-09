using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;

namespace Leda.Core.Effects.Particles
{
    public class EmitterFactory
    {
        private int _burstCount;
        private int _particlesPerBurst;
        private int _burstIntervalInMilliseconds;

        // Emission points - relative positions where particles are launched from
        public List<Vector2> _emissionPointOffsets;

        // How long do the particles last?
        private Range _lifespanRange;

        // Initial values when they are launched from the emitter
        private float _spread;
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
        public List<Color> _tints;

        // Fade points - life timer offsets and alpha value pairs to control shimmer effects
        private List<Vector2> _fadePoints;

        public EmitterFactory()
        {
            _emissionPointOffsets = new List<Vector2>();
            _tints = new List<Color>();
            _fadePoints = new List<Vector2>();

            _burstCount = 1;
            _particlesPerBurst = 0;
            _burstIntervalInMilliseconds = 0;

            _spread = MathHelper.Pi;

            _lifespanRange = Range.Empty;

            SetSpeedMetrics(Range.Empty, Range.One, Range.One);
            SetSpinMetrics(new Range(0.0f, 360.0f), Range.Empty, Range.One);
            SetScaleMetrics(Range.One, Range.One, Range.One);
        }

        public EmitterFactory(XElement xmlData)
            : this()
        {
            SetBehaviourFromXml(xmlData);
        }

        public void SetBurstMetrics(int burstCount, int burstIntervalInMilliseconds, int particlesPerBurst, float spread)
        {
            _burstCount = burstCount;
            _particlesPerBurst = particlesPerBurst;
            _burstIntervalInMilliseconds = burstIntervalInMilliseconds;
            _spread = spread;
        }

        public void SetParticleLifespan(int minimumDurationInMilliseconds, int maximumDurationInMilliseconds)
        {
            _lifespanRange = new Range(minimumDurationInMilliseconds, maximumDurationInMilliseconds);
        }

        public void SetSpeedMetrics(Range launch, Range delta, Range acceleration)
        {
            _speedLaunchRange = launch;
            _speedDeltaRange = delta;
            _speedAcceleratorRange = acceleration;
        }

        public void SetSpinMetrics(Range launch, Range delta, Range acceleration)
        {
            _rotationLaunchRange = launch;
            _rotationDeltaRange = delta;
            _rotationAcceleratorRange = acceleration;
        }

        public void SetScaleMetrics(Range launch, Range delta, Range acceleration)
        {
            _scaleLaunchRange = launch;
            _scaleDeltaRange = delta;
            _scaleAcceleratorRange = acceleration;
        }

        public void AddFadePoint(float lifespanFraction, float alphaFraction)
        {
            if (_fadePoints == null) { _fadePoints = new List<Vector2>(); }
            _fadePoints.Add(new Vector2(MathHelper.Clamp(lifespanFraction, 0.0f, 1.0f), MathHelper.Clamp(alphaFraction, 0.0f, 1.0f)));
        }

        public void AddEmissionPointOffset(Vector2 offset)
        {
            _emissionPointOffsets.Add(offset);
        }

        public void SetBehaviourFromXml(XElement data)
        {
            //try
            {
                float spread = MathHelper.TwoPi;
                if (data.Element("burstmetrics").Attributes("spread").Count() > 0)
                {
                    spread = MathHelper.ToRadians((float)data.Element("burstmetrics").Attribute("spread"));
                }

                SetBurstMetrics(
                    Convert.ToInt32(data.Element("burstmetrics").Attribute("burstcount").Value),
                    Convert.ToInt32(data.Element("burstmetrics").Attribute("burstinterval").Value),
                    Convert.ToInt32(data.Element("burstmetrics").Attribute("particlesperburst").Value),
                    spread);

                SetParticleLifespan(
                    Convert.ToInt32(data.Element("lifespan").Attribute("minimum").Value),
                    Convert.ToInt32(data.Element("lifespan").Attribute("maximum").Value));

                SetSpeedMetrics(
                    new Range(
                        (float)data.Element("speed").Element("launch").Attribute("minimum"),
                        (float)data.Element("speed").Element("launch").Attribute("maximum")),
                    new Range(
                        (float)data.Element("speed").Element("delta").Attribute("minimum"),
                        (float)data.Element("speed").Element("delta").Attribute("maximum")),
                    new Range(
                        (float)data.Element("speed").Element("accelerator").Attribute("minimum"),
                        (float)data.Element("speed").Element("accelerator").Attribute("maximum")));

                if (data.Elements("rotation").Count() > 0)
                {
                    SetSpinMetrics(
                        new Range(
                            (float)data.Element("rotation").Element("launch").Attribute("minimum"),
                            (float)data.Element("rotation").Element("launch").Attribute("maximum")),
                        new Range(
                            (float)data.Element("rotation").Element("delta").Attribute("minimum"),
                            (float)data.Element("rotation").Element("delta").Attribute("maximum")),
                        new Range(
                            (float)data.Element("rotation").Element("accelerator").Attribute("minimum"),
                            (float)data.Element("rotation").Element("accelerator").Attribute("maximum")));
                }

                if (data.Elements("scale").Count() > 0)
                {
                    SetScaleMetrics(
                        new Range(
                            (float)data.Element("scale").Element("launch").Attribute("minimum"),
                            (float)data.Element("scale").Element("launch").Attribute("maximum")),
                        new Range(
                            (float)data.Element("scale").Element("delta").Attribute("minimum"),
                            (float)data.Element("scale").Element("delta").Attribute("maximum")),
                        new Range(
                            (float)data.Element("scale").Element("accelerator").Attribute("minimum"),
                            (float)data.Element("scale").Element("accelerator").Attribute("maximum")));
                }

                if (data.Elements("tints").Count() > 0)
                {
                    foreach (XElement el in data.Element("tints").Elements("tint"))
                    {
                        _tints.Add(new Color(
                            Convert.ToByte(el.Attribute("red").Value),
                            Convert.ToByte(el.Attribute("green").Value),
                            Convert.ToByte(el.Attribute("blue").Value),
                            Convert.ToByte(el.Attribute("alpha").Value)));
                    }
                }

                if (data.Elements("fadepoints").Count() > 0)
                {
                    foreach (XElement el in data.Element("fadepoints").Elements("fadepoint"))
                    {
                        AddFadePoint((float)el.Attribute("lifefraction"), (float)el.Attribute("alphafraction"));
                    }
                }

                if (data.Elements("emissionoffsets").Count() > 0)
                {
                    foreach (XElement el in data.Element("emissionoffsets").Elements("emissionoffset"))
                    {
                        AddEmissionPointOffset(new Vector2((float)el.Attribute("x"), (float)el.Attribute("y")));
                    }
                }
            }
            //catch (Exception ex)
            //{
            //    throw new Exception("Emitter data is not properly formed or missing values");
            //}
        }

        public Emitter CreateEmitter()
        {
            Emitter newEmitter = new Emitter();
            newEmitter.Spread = _spread;
            newEmitter.SetBurstMetrics(_burstCount, _burstIntervalInMilliseconds, _particlesPerBurst);
            newEmitter.SetParticleLifespan((int)_lifespanRange.Minimum, (int)_lifespanRange.Maximum);
            newEmitter.SetSpeedMetrics(_speedLaunchRange, _speedDeltaRange, _speedAcceleratorRange);
            newEmitter.SetSpinMetrics(_rotationLaunchRange, _rotationDeltaRange, _rotationAcceleratorRange);
            newEmitter.SetScaleMetrics(_scaleLaunchRange, _scaleDeltaRange, _scaleAcceleratorRange);
            for (int i = 0; i < _tints.Count; i++) { newEmitter.Tints.Add(_tints[i]); }
            newEmitter.FadePoints = _fadePoints;
            newEmitter.EmissionPointOffsets = _emissionPointOffsets;

            return newEmitter;
        }
    }
}
