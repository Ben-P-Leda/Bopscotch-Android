namespace Bopscotch.Effects.Popups
{
    public class AutoDismissingPopup : PopupRequiringDismissal
    {
        protected bool _appearing;

        public AutoDismissingPopup()
            : base()
        {
            EntrySequenceName = Default_Entry_Sequence_Name;
        }

        public override void Activate()
        {
            _appearing = true;

            base.Activate();
        }

        public override void Dismiss()
        {
            _appearing = false;

            base.Dismiss();
        }

        protected override void HandleAnimationSequenceCompletion()
        {
            if (Visible)
            {
                if (_appearing) { Dismiss(); }
                else { base.HandleAnimationSequenceCompletion(); }
            }
        }

        private const string Default_Entry_Sequence_Name = "image-popup-entry";
    }
}
