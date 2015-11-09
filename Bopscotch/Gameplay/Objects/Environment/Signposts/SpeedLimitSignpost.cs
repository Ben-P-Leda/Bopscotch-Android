using System.Xml.Linq;

using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Animation;
using Leda.Core.Serialization;
using Leda.Core.Asset_Management;
using Leda.Core.Game_Objects.Behaviours;

namespace Bopscotch.Gameplay.Objects.Environment.Signposts
{
    public class SpeedLimitSignpost : SignpostBase, IAnimated, ITransformationAnimatable
    {
        private bool _animationRunning;

        public Range SpeedRange { get; set; }
        public IAnimationEngine AnimationEngine { get; set; }

        public SpeedLimitSignpost()
            : base()
        {
            _animationRunning = false;

            AnimationEngine = new TransformationAnimationEngine(this);
            AnimationEngine.SequenceCompletionHandler = HandleAnimationSequenceCompletion;
        }

        public void InitiateCollisionEffect()
        {
            if (!_animationRunning)
            {
                SoundEffectManager.PlayEffect("speed-sign-trigger");
                AnimationEngine.Sequence = AnimationDataManager.Sequences["quadruple-bounce"];
                _animationRunning = true;
            }
        }

        private void HandleAnimationSequenceCompletion()
        {
            Scale = 1.0f;
            _animationRunning = false;
        }

        protected override XElement Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.AddDataItem("speed-range", SpeedRange);

            return serializer.SerializedData;
        }

        protected override Serializer Deserialize(Serializer serializer)
        {
            base.Deserialize(serializer);

            SpeedRange = serializer.GetDataItem<Range>("speed-range");

            return serializer;
        }

        public const string Data_Node_Name = "speed-limit-signpost";
    }
}