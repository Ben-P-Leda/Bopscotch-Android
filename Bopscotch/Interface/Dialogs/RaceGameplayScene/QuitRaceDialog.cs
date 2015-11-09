using Microsoft.Xna.Framework;

using Leda.Core.Timing;

namespace Bopscotch.Interface.Dialogs.RaceGameplayScene
{
    public class QuitRaceDialog : ButtonDialog
    {
        public Timer CancellationTimer { private get; set; }

        public QuitRaceDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Top_Y_When_Active;

            AddButton("Cancel", new Vector2(Definitions.Right_Button_Column_X, 200), Button.ButtonIcon.Play, Color.LawnGreen);

            _cancelButtonCaption = "Cancel";

            _boxCaption = "";
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(0.0f, -Height);
        }

        public override void Activate()
        {
            UpdateCaption();
            SetInputThreshold(Top_Y_When_Active + Dialog_Height);
            base.Activate();
        }

        private void UpdateCaption()
        {
            _boxCaption = Translator.Translation("race-quit").Replace("[TIME]", ((int)(4 - (CancellationTimer.CurrentActionProgress * 3))).ToString());
        }

        private void SetInputThreshold(float topY)
        {
            for (int i = 0; i < InputSources.Count; i++)
            {
                if (InputSources[i] is Input.TouchControls) { ((Input.TouchControls)InputSources[i]).InGameActionBoundary = topY; }
            }
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            UpdateCaption();
            base.Update(millisecondsSinceLastUpdate);
        }

        protected override void Dismiss()
        {
            SetInputThreshold(0.0f);

            base.Dismiss();
        }

        private const float Top_Y_When_Active = 50.0f;
        private const int Dialog_Height = 300;

        public const int Cancellation_Duration_In_Milliseconds = 3000;
    }
}
