using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core.Serialization;

namespace Bopscotch.Effects.Popups
{
    public class PopupRequiringDismissal : PopupBase
    {
        private bool _isActivating;

        public delegate void AnimationCompletionCallback();
        public AnimationCompletionCallback AnimationCompletionHandler { private get; set; }

        public string EntrySequenceName { private get; set; }
        public string ExitSequenceName { private get; set; }
        public bool AwaitingDismissal { get; private set; }
		public bool BeingDismissed { get; private set; }

        public PopupRequiringDismissal()
            : base()
        {
            _isActivating = false;
            AwaitingDismissal = false;
			BeingDismissed = false;

            AnimationCompletionHandler = null;
            EntrySequenceName = Default_Entry_Sequence_Name;
            ExitSequenceName = Default_Exit_Sequence_Name;
        }

        public virtual void Activate()
        {
            _isActivating = true;
            AwaitingDismissal = false;
            Scale = 0.0f;
            Tint = Color.White;

            RunPopupSequence(EntrySequenceName, false);
        }

        public virtual void Dismiss()
        {
            _isActivating = false;
            AwaitingDismissal = false;
			BeingDismissed = true;

            RunPopupSequence(ExitSequenceName, true);
        }

        protected override void HandleAnimationSequenceCompletion()
        {
            AwaitingDismissal = _isActivating;
			BeingDismissed = false;

 	        base.HandleAnimationSequenceCompletion();
            if (AnimationCompletionHandler != null) { AnimationCompletionHandler(); }
        }

        protected override XElement Serialize(Serializer serializer)
        {
            serializer.AddDataItem("activating", _isActivating);
            serializer.AddDataItem("awaiting-dismissal", AwaitingDismissal);

            return base.Serialize(serializer);
        }

        protected override Serializer Deserialize(Serializer serializer)
        {
            _isActivating = serializer.GetDataItem<bool>("activating");
            AwaitingDismissal = serializer.GetDataItem<bool>("awaiting-dismissal");

            return base.Deserialize(serializer);
        }

        private const string Default_Entry_Sequence_Name = "image-popup-entry-with-bounce";
        private const string Default_Exit_Sequence_Name = "image-popup-exit";
    }
}
