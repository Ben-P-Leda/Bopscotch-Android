using Leda.Core.Asset_Management;

namespace Bopscotch.Effects.Popups.Ranking
{
    public class RankingStar : RankingPopupBase
    {
        private SequenceStep _step;

        public RankingStar() : base()
        {
            TextureName = Texture_Name;
        }

        public override void Activate()
        {
            base.Activate();

            StartStep(SequenceStep.Entering, Entry_Animation_Sequence);
        }

        protected override void HandleAnimationSequenceComplete()
        {
            switch (_step)
            {
                case SequenceStep.Entering: StartStep(SequenceStep.Bounce, Bounce_Animation_Sequence); NextAction(); break;
                case SequenceStep.Bounce: StartStep(SequenceStep.End, Rock_Animation_Sequence); break;
            }
        }

        private void StartStep(SequenceStep step, string animationSequence)
        {
            _step = step;
            AnimationEngine.Sequence = AnimationDataManager.Sequences[animationSequence];
        }

        private enum SequenceStep
        {
            Entering,
            Bounce,
            End
        }

        private const string Texture_Name = "ranking-stars";
        private const string Entry_Animation_Sequence = "image-popup-entry-fast";
        private const string Bounce_Animation_Sequence = "quadruple-bounce";
        private const string Rock_Animation_Sequence = "rock";
    }
}
