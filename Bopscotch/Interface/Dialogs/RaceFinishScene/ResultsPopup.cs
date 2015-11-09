using Microsoft.Xna.Framework;

using Bopscotch.Effects.Popups;

namespace Bopscotch.Interface.Dialogs.RaceFinishScene
{
    public class ResultsPopup : PopupRequiringDismissal
    {
        public Definitions.RaceOutcome Outcome { private get; set; }

        public float ParentDialogY { set { DisplayPosition = new Vector2(Definitions.Back_Buffer_Center.X, value + Offset_From_Dialog_Top); } }

        public ResultsPopup()
            : base()
        {
            RenderDepth = 0.05f;
        }

        public override void Activate()
        {
            base.Activate();

            switch (Outcome)
            {
                case Definitions.RaceOutcome.OwnPlayerWin:
                    MappingName = Own_Player_Win_Texture;
                    break;
                case Definitions.RaceOutcome.OpponentPlayerWin:
                    MappingName = Opponent_Player_Win_Texture;
                    break;
                case Definitions.RaceOutcome.Draw:
                    MappingName = Draw_Texture;
                    break;
                case Definitions.RaceOutcome.Incomplete:
                    MappingName = Incomplete_Texture;
                    break;
            }
        }

        private const float Offset_From_Dialog_Top = 200.0f;
        private const string Own_Player_Win_Texture = "popup-race-you-win";
        private const string Opponent_Player_Win_Texture = "popup-race-you-lose";
        private const string Draw_Texture = "popup-race-draw";
        private const string Incomplete_Texture = "popup-race-not-finished";
    }
}
