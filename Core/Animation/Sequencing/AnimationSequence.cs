using System.Collections.Generic;

namespace Leda.Core.Animation
{
    public class AnimationSequence
    {
        public List<IKeyframe> Keyframes { get; private set; }
        public AnimationSequenceType SequenceType { get; private set; }
        public bool Loops { get; set; }
        public int FrameCount { get { return Keyframes.Count; } }

        public AnimationSequence(AnimationSequenceType sequenceType)
        {
            SequenceType = sequenceType;
            Keyframes = new List<IKeyframe>();
        }

        public enum AnimationSequenceType
        {
            Transformation,
            SpriteSheet,
            Colour,
            Skeletal
        }
    }
}
