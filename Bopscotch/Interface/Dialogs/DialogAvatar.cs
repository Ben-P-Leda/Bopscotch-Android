using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Animation;
using Leda.Core.Asset_Management;

namespace Bopscotch.Interface.Dialogs
{
    public class DialogAvatar : DisposableSkeleton, IAnimated
    {
        private SkeletalAnimationEngine _animationEngine;
        public IAnimationEngine AnimationEngine { get { return _animationEngine; } }

        public DialogAvatar()
            : base()
        {
            _animationEngine = new SkeletalAnimationEngine(this);

            Visible = true;
        }

        public void StartRestingAnimationSequence()
        {
            _animationEngine.Sequence = AnimationDataManager.Sequences[Resting_Animation_Sequence];
        }

        private const string Resting_Animation_Sequence = "player-front-resting";
    }
}
