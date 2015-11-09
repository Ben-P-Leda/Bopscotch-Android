using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Timing;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;

namespace Leda.Core.Animation
{
    public class TransformationAnimationEngine : AnimationEngineBase
    {
        private ITransformationAnimatable _target;
        private float _startingRotation;
        private float _targetAngle;
        private float _startingScale;

        public TransformationAnimationEngine(ITransformationAnimatable target)
            : base()
        {
            _target = target;
            if (_target is ISerializable) { ID = string.Concat(((ISerializable)_target).ID, "-transformation-animation-engine"); }
        }

        protected override void StartNextKeyframe()
        {
            base.StartNextKeyframe();

            // At the start of the keyframe, we need to break out the start points so we know what we're LERPing from
            _startingRotation = Utility.RectifyAngle(_target.Rotation);
            _startingScale = _target.Scale;

            // Calculate target angle relative to start angle to ensure no problems with LERP if we cross the 0/360 boundary
            _targetAngle = _startingRotation +
                Utility.AngleDifference(_startingRotation, ((TransformationKeyframe)_sequence.Keyframes[_currentKeyframeIndex]).TargetRotation);
        }

        public override void UpdateAnimation(int millisecondsSinceLastUpdate)
        {
            if (AnimationIsRunning)
            {
                base.UpdateAnimation(millisecondsSinceLastUpdate);

                if (AnimationIsRunning)
                {
                    _target.Rotation = MathHelper.Lerp(_startingRotation, _targetAngle, KeyframeProgress);
                    _target.Scale = MathHelper.Lerp(_startingScale, ((TransformationKeyframe)_sequence.Keyframes[_currentKeyframeIndex]).TargetScale, KeyframeProgress);
                }
            }
        }

        protected override void HandleKeyframeCompletion()
        {
            _target.Rotation = _targetAngle;
            _target.Scale = ((TransformationKeyframe)_sequence.Keyframes[_currentKeyframeIndex]).TargetScale;

            base.HandleKeyframeCompletion();
        }

        protected override XElement Serialize(Serializer serializer)
        {
            serializer.AddDataItem("startangle", _startingRotation);
            serializer.AddDataItem("targetangle", _targetAngle);
            serializer.AddDataItem("startscale", _startingScale);

            return base.Serialize(serializer);
        }

        protected override void Deserialize(Serializer serializer)
        {
            _startingRotation = serializer.GetDataItem<float>("startangle");
            _targetAngle = serializer.GetDataItem<float>("targetangle");
            _startingScale = serializer.GetDataItem<float>("startscale");

            base.Deserialize(serializer);
        }

        protected override IKeyframe DeserializeKeyframe(XElement serializedData)
        {
            TransformationKeyframe newKeyframe = new TransformationKeyframe();
            newKeyframe.Deserialize(serializedData);

            return newKeyframe;
        }
    }
}
