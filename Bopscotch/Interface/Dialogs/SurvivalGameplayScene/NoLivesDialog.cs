using Microsoft.Xna.Framework;

namespace Bopscotch.Interface.Dialogs.SurvivalGameplayScene
{
    public class NoLivesDialog : ButtonDialog
    {
        public NoLivesDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Definitions.Back_Buffer_Center.Y - (Dialog_Height / 2.0f);

            AddButton("Add Lives", new Vector2(Definitions.Left_Button_Column_X, 200), Button.ButtonIcon.Store, Color.Orange);
            AddButton("Quit", new Vector2(Definitions.Right_Button_Column_X, 200), Button.ButtonIcon.Ticket, Color.DodgerBlue);

            _activeButtonCaption = "Add Lives";
            _cancelButtonCaption = "Quit";

            _boxCaption = "No Lives Left!";
        }

        public override void Activate()
        {
            base.Activate();
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(0.0f, -Height);
        }

        private const int Dialog_Height = 420;
    }
}
