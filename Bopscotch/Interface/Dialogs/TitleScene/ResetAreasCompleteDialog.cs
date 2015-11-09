using Microsoft.Xna.Framework;

namespace Bopscotch.Interface.Dialogs.TitleScene
{
    public class ResetAreasCompleteDialog : ButtonDialog
    {
        public ResetAreasCompleteDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = 400;

            AddButton("Back", new Vector2(Definitions.Back_Buffer_Center.X, 225), Button.ButtonIcon.Back, Color.Red,0.7f);

           _defaultButtonCaption = "Back";
            _cancelButtonCaption = "Back";

            _boxCaption = "Reset Complete!";
        }

        private const int Dialog_Height = 300;
    }
}
