using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;

namespace Leda.Core.Animation
{
    public interface IAnimationEngine : ISerializable
    {
        AnimationSequence Sequence { set; }
        AnimationEngineBase.KeyframeCompletionCallback KeyframeCompletionHandler { set; }
        AnimationEngineBase.SequenceCompletionCallback SequenceCompletionHandler { set; }

        void UpdateAnimation(int millisecondsSinceLastUpdate);
    }
}
