using System.Xml.Linq;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;

namespace Bopscotch.Gameplay.Objects.Environment.Blocks
{
    public class SurvivalModeItemSmashBlock : SmashBlock, ITemporary
    {
        public bool ReadyForDisposal { get; set; }

        public SurvivalModeItemSmashBlock()
            : base()
        {
            ReadyForDisposal = false;
        }

        public override void HandleSmash()
        {
            ReadyForDisposal = true;

            base.HandleSmash();
        }

        public void PrepareForDisposal()
        {
        }
    }
}
