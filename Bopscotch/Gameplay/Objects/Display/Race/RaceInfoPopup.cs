using Bopscotch.Effects.Popups;

namespace Bopscotch.Gameplay.Objects.Display.Race
{
    public class RaceInfoPopup : AutoDismissingPopup
    {
        public RaceInfoPopup()
            : base()
        {
			ID = "position-status-popup";

            EntrySequenceName = Default_Entry_Sequence_Name;
            ExitSequenceName = Default_Exit_Sequence_Name;
        }

        public void StartPopupForRaceInfo(string textureName)
        {
            Visible = true;
            Scale = 0.0f;

            MappingName = textureName;
            Activate();
        }

        private const string Default_Entry_Sequence_Name = "image-popup-entry-with-bounce";
        private const string Default_Exit_Sequence_Name = "image-popup-slow-exit";
    }
}
