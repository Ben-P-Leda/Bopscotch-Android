using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Timing;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;

namespace Leda.Core.Animation
{
    public class ColourAnimationEngine : AnimationEngineBase
    {
        private IColourAnimatable _target;
        private Color _startingTint;

        public ColourAnimationEngine(IColourAnimatable target)
            : base()
        {
            _target = target;
            if (_target is ISerializable) { ID = string.Concat(((ISerializable)_target).ID, "-colour-animation-engine"); }
        }

        protected override void StartNextKeyframe()
        {
            base.StartNextKeyframe();

            _startingTint = _target.Tint;
        }

        public override void UpdateAnimation(int millisecondsSinceLastUpdate)
        {
            if (AnimationIsRunning)
            {
                base.UpdateAnimation(millisecondsSinceLastUpdate);

                if (AnimationIsRunning)
                {
                    _target.Tint = Color.Lerp(_startingTint, ((ColourKeyframe)_sequence.Keyframes[_currentKeyframeIndex]).TargetTint, KeyframeProgress);
                }
            }
        }

        protected override void HandleKeyframeCompletion()
        {
            _target.Tint = ((ColourKeyframe)_sequence.Keyframes[_currentKeyframeIndex]).TargetTint;

            base.HandleKeyframeCompletion();
        }

        protected override XElement Serialize(Serializer serializer)
        {
            serializer.AddDataItem("start-tint", _startingTint);

            return base.Serialize(serializer);
        }

        protected override void Deserialize(Serializer serializer)
        {
            _startingTint = serializer.GetDataItem<Color>("start-tint");

            base.Deserialize(serializer);
        }

        protected override IKeyframe DeserializeKeyframe(XElement serializedData)
        {
            ColourKeyframe newKeyframe = new ColourKeyframe();
            newKeyframe.Deserialize(serializedData);

            return newKeyframe;
        }
    }
}
