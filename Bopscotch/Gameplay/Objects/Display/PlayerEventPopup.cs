using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Asset_Management;

using Bopscotch.Effects.Popups;
using Bopscotch.Gameplay.Objects.Characters.Player;

namespace Bopscotch.Gameplay.Objects.Display
{
    public class PlayerEventPopup : AutoDismissingPopup
    {
        public PlayerEventPopup()
            : base()
        {
            ID = Tombstone_ID;
            EntrySequenceName = Default_Entry_Sequence_Name;
            ExitSequenceName = Default_Exit_Sequence_Name;
        }

        public void StartPopupForEvent(Player.PlayerEvent playerEvent)
        {
            Visible = true;
            Scale = 0.0f;

            if (!string.IsNullOrEmpty(PopupTextureName(playerEvent)))
            {
                MappingName = PopupTextureName(playerEvent);
                Activate();
            }
        }

        private string PopupTextureName(Player.PlayerEvent playerEvent)
        {
            List<string> textureNames = new List<string>();
            string prefix = "";

            switch (playerEvent)
            {
                case Player.PlayerEvent.Died: prefix = "death"; break;
                case Player.PlayerEvent.Goal_Passed: prefix = "clear"; break;
            }

            return string.Concat("popup-", prefix, "-1");
        }

        private const string Default_Entry_Sequence_Name = "image-popup-entry-with-bounce";
        private const string Default_Exit_Sequence_Name = "image-popup-exit";
        private const string Tombstone_ID = "player-event-popup";
    }
}
