using Microsoft.Xna.Framework;

using Leda.Core.Asset_Management;

namespace Bopscotch.Effects.Popups.Ranking
{
    public class RankingLetter : RankingPopupBase
    {
        private bool _held;

        public RankingLetter() : base()
        {
            TextureName = Texture_Name;
            DisplayPosition = Definitions.Back_Buffer_Center + new Vector2(0.0f, 100.0f);
        }

        public override void Activate()
        {
            base.Activate();

            _held = false;

            AnimationEngine.Sequence = AnimationDataManager.Sequences[Entry_Animation_Sequence];
        }

        protected override void HandleAnimationSequenceComplete()
        {
            if (!_held)
            {
                _held = true;
                AnimationEngine.Sequence = AnimationDataManager.Sequences[Hold_Animation_Sequence];
            }
            else
            {
                NextAction();
            }
        }

        private const string Texture_Name = "ranking-letters";
        private const string Entry_Animation_Sequence = "image-popup-entry-with-bounce-big";
        private const string Hold_Animation_Sequence = "hold";
    }
}
